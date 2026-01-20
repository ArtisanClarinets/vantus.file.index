using System.Text.Json;
using Vantus.Core.Models;

namespace Vantus.Core.Services;

public class SettingsStore : IDisposable
{
    private readonly string _settingsPath;
    private readonly string _backupPath;
    private readonly string _dataPath;
    private readonly string _lockPath;
    private readonly IFileLockProvider _lockProvider;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IValidationService _validationService;
    private readonly ISettingsMigrationService _migrationService;
    private readonly ITelemetryService _telemetry;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private SettingsSnapshot _currentSettings;
    private SettingsSchema _schema = new();
    private const string SettingsFileName = "settings.json";
    private const string BackupSuffix = ".backup";
    private const string LockSuffix = ".lock";
    private const string SchemaFileName = "settings_definitions.json";
    private const string TempSuffix = ".tmp";

    public event EventHandler<SettingChangedEventArgs>? SettingChanged;
    public event EventHandler<SettingsErrorEventArgs>? SettingsError;

    public SettingsStore(
        string dataPath,
        IFileLockProvider? lockProvider = null,
        IRetryPolicy? retryPolicy = null,
        IValidationService? validationService = null,
        ISettingsMigrationService? migrationService = null,
        ITelemetryService? telemetry = null)
    {
        _dataPath = dataPath;
        _settingsPath = Path.Combine(dataPath, SettingsFileName);
        _backupPath = _settingsPath + BackupSuffix;
        _lockPath = _settingsPath + LockSuffix;
        _lockProvider = lockProvider ?? new FileLockProvider();
        _retryPolicy = retryPolicy ?? new ExponentialBackoffRetryPolicy();
        _validationService = validationService ?? new ValidationService();
        _migrationService = migrationService ?? new SettingsMigrationService(new NullTelemetryService());
        _telemetry = telemetry ?? new NullTelemetryService();
        _currentSettings = new SettingsSnapshot();
    }

    public async Task InitializeAsync()
    {
        await _telemetry.TrackDependencyAsync(
            "FileSystem",
            "CreateDirectory",
            _dataPath,
            true,
            TimeSpan.Zero);

        Directory.CreateDirectory(_dataPath);
        await LoadSchemaAsync();
        await LoadSettingsAsync();
    }

    private async Task LoadSchemaAsync()
    {
        var startTime = DateTime.UtcNow;
        bool success = false;

        try
        {
            var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SchemaFileName);
            if (!File.Exists(schemaPath))
            {
                schemaPath = Path.Combine(Directory.GetCurrentDirectory(), SchemaFileName);
            }

            if (File.Exists(schemaPath))
            {
                _schema = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var json = await File.ReadAllTextAsync(schemaPath);
                    return JsonSerializer.Deserialize<SettingsSchema>(json) ?? new SettingsSchema();
                });

                success = true;
            }
            else
            {
                _ = _telemetry.TrackExceptionAsync(new FileNotFoundException(
                    $"Schema file not found at {schemaPath}"));
            }
        }
        catch (Exception ex)
        {
            await _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
            {
                { "Operation", "LoadSchemaAsync" },
                { "SchemaPath", _settingsPath }
            });

            OnSettingsError(new SettingsErrorEventArgs
            {
                Operation = "LoadSchema",
                Message = ex.Message,
                Exception = ex
            });
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;
            await _telemetry.TrackDependencyAsync(
                "FileSystem",
                "LoadSchema",
                SchemaFileName,
                success,
                duration);
        }
    }

    private async Task LoadSettingsAsync()
    {
        var startTime = DateTime.UtcNow;
        bool success = false;

        await _semaphore.WaitAsync();

        try
        {
            if (File.Exists(_settingsPath))
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var json = await File.ReadAllTextAsync(_settingsPath);
                    var loaded = JsonSerializer.Deserialize<SettingsSnapshot>(json);

                    if (loaded != null)
                    {
                        var validation = _validationService.ValidateSettingsSnapshot(loaded);

                        if (!validation.IsValid)
                        {
                            _ = _telemetry.TrackEventAsync("SettingsValidationFailed", new Dictionary<string, string>
                            {
                                { "Errors", string.Join("; ", validation.Errors) }
                            });

                            await RestoreFromBackupAsync();
                            return;
                        }

                        if (_migrationService.NeedsMigration(loaded.SchemaVersion ?? string.Empty))
                        {
                            loaded = await _migrationService.MigrateAsync(loaded, _schema.SchemaVersion);
                        }

                        _currentSettings = loaded;
                        success = true;
                    }
                });
            }
            else
            {
                await CreateDefaultSettingsAsync();
                success = true;
            }
        }
        catch (Exception ex)
        {
            await _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
            {
                { "Operation", "LoadSettingsAsync" },
                { "SettingsPath", _settingsPath }
            });

            await RestoreFromBackupAsync();
        }
        finally
        {
            _semaphore.Release();

            var duration = DateTime.UtcNow - startTime;
            await _telemetry.TrackDependencyAsync(
                "FileSystem",
                "LoadSettings",
                SettingsFileName,
                success,
                duration);
        }
    }

    private async Task CreateDefaultSettingsAsync()
    {
        _currentSettings = new SettingsSnapshot
        {
            Version = _schema.SchemaVersion,
            SchemaVersion = _schema.SchemaVersion,
            ActivePreset = "personal",
            CreatedAt = DateTime.UtcNow,
            GlobalSettings = new Dictionary<string, object>(),
            WorkspaceSettings = new Dictionary<string, Dictionary<string, object>>(),
            LocationSettings = new Dictionary<string, Dictionary<string, object>>()
        };

        await SaveSettingsAsync();
    }

    private async Task RestoreFromBackupAsync()
    {
        if (File.Exists(_backupPath))
        {
            try
            {
                File.Copy(_backupPath, _settingsPath, true);

                var json = await File.ReadAllTextAsync(_settingsPath);
                _currentSettings = JsonSerializer.Deserialize<SettingsSnapshot>(json) ?? new SettingsSnapshot();

                await _telemetry.TrackEventAsync("SettingsRestoredFromBackup");
            }
            catch (Exception ex)
            {
                await _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
                {
                    { "Operation", "RestoreFromBackup" }
                });

                await CreateDefaultSettingsAsync();
            }
        }
        else
        {
            await CreateDefaultSettingsAsync();
        }
    }

    public async Task SaveSettingsAsync(bool createBackup = true)
    {
        var startTime = DateTime.UtcNow;
        bool success = false;

        await _semaphore.WaitAsync();

        try
        {
            var validation = _validationService.ValidateSettingsSnapshot(_currentSettings);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Settings validation failed: {string.Join(", ", validation.Errors)}");
            }

            var tempPath = _settingsPath + TempSuffix;
            var json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(tempPath, json);

            if (createBackup && File.Exists(_settingsPath))
            {
                File.Copy(_settingsPath, _backupPath, true);
            }

            using var _ = _lockProvider.AcquireLock(_lockPath, TimeSpan.FromSeconds(5));

            if (File.Exists(_settingsPath))
            {
                File.Delete(_settingsPath);
            }

            File.Move(tempPath, _settingsPath);

            success = true;

            await _telemetry.TrackMetricAsync("SettingsSaved", 1);
        }
        catch (Exception ex)
        {
            await _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
            {
                { "Operation", "SaveSettingsAsync" },
                { "SettingsPath", _settingsPath }
            });

            OnSettingsError(new SettingsErrorEventArgs
            {
                Operation = "SaveSettings",
                Message = ex.Message,
                Exception = ex
            });

            throw;
        }
        finally
        {
            _semaphore.Release();

            var duration = DateTime.UtcNow - startTime;
            await _telemetry.TrackDependencyAsync(
                "FileSystem",
                "SaveSettings",
                SettingsFileName,
                success,
                duration);
        }
    }

    public T? GetValue<T>(string settingId, string scope = "global", string? workspaceId = null, string? locationPath = null)
    {
        var key = GetKey(settingId, scope, workspaceId, locationPath);

        if (scope == "global" && _currentSettings.GlobalSettings.TryGetValue(settingId, out var value))
        {
            return ConvertValue<T>(value);
        }

        if (scope == "workspace" && workspaceId != null)
        {
            if (_currentSettings.WorkspaceSettings.TryGetValue(workspaceId, out var wsSettings) &&
                wsSettings.TryGetValue(settingId, out value))
            {
                return ConvertValue<T>(value);
            }
        }

        if (scope == "location" && locationPath != null)
        {
            if (_currentSettings.LocationSettings.TryGetValue(locationPath, out var lsSettings) &&
                lsSettings.TryGetValue(settingId, out value))
            {
                return ConvertValue<T>(value);
            }
        }

        var defaultValue = GetDefaultValue<T>(settingId);
        _ = _telemetry.TrackMetricAsync("SettingsDefaultUsed", 1, new Dictionary<string, string>
        {
            { "SettingId", settingId }
        });

        return defaultValue;
    }

    private T? GetDefaultValue<T>(string settingId)
    {
        foreach (var category in _schema.Categories)
        {
            foreach (var page in category.Value.Pages)
            {
                var setting = page.Value.Settings.FirstOrDefault(s => s.Id == settingId);
                if (setting != null && setting.Defaults.TryGetValue(_currentSettings.ActivePreset ?? "personal", out var defaultValue))
                {
                    return ConvertValue<T>(defaultValue);
                }
            }
        }

        return default;
    }

    public void SetValue<T>(string settingId, T value, string scope = "global", string? workspaceId = null, string? locationPath = null)
    {
        var key = GetKey(settingId, scope, workspaceId, locationPath);
        var oldValue = GetValue<T>(settingId, scope, workspaceId, locationPath);

        var definition = FindSettingDefinition(settingId);
        if (definition != null)
        {
            var validation = _validationService.ValidateSettingValue(definition, value);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"Invalid value for {settingId}: {string.Join(", ", validation.Errors)}");
            }
        }

        if (scope == "global")
        {
            _currentSettings.GlobalSettings[settingId] = value!;
        }
        else if (scope == "workspace" && workspaceId != null)
        {
            if (!_currentSettings.WorkspaceSettings.ContainsKey(workspaceId))
            {
                _currentSettings.WorkspaceSettings[workspaceId] = new Dictionary<string, object>();
            }
            _currentSettings.WorkspaceSettings[workspaceId][settingId] = value!;
        }
        else if (scope == "location" && locationPath != null)
        {
            if (!_currentSettings.LocationSettings.ContainsKey(locationPath))
            {
                _currentSettings.LocationSettings[locationPath] = new Dictionary<string, object>();
            }
            _currentSettings.LocationSettings[locationPath][settingId] = value!;
        }

        SettingChanged?.Invoke(this, new SettingChangedEventArgs
        {
            SettingId = settingId,
            OldValue = oldValue,
            NewValue = value,
            Scope = scope
        });

        _ = SaveSettingsAsync();
    }

    private SettingDefinition? FindSettingDefinition(string settingId)
    {
        foreach (var category in _schema.Categories)
        {
            foreach (var page in category.Value.Pages)
            {
                var setting = page.Value.Settings.FirstOrDefault(s => s.Id == settingId);
                if (setting != null)
                {
                    return setting;
                }
            }
        }
        return null;
    }

    public object? GetValueWithPolicy(string settingId, PolicyState policyState, string scope = "global")
    {
        var lockInfo = GetLockInfo(settingId, policyState);
        if (lockInfo.IsLocked)
        {
            return lockInfo.LockedValue;
        }

        return GetValue<object>(settingId, scope);
    }

    public LockedSettingState GetLockInfo(string settingId, PolicyState policyState)
    {
        if (policyState.ActiveLocks.TryGetValue(settingId, out var lockInfo))
        {
            return new LockedSettingState
            {
                SettingId = settingId,
                LockedValue = lockInfo.LockedValue,
                Reason = lockInfo.Reason,
                Source = lockInfo.Source
            };
        }

        return new LockedSettingState { SettingId = settingId };
    }

    public string GetKey(string settingId, string scope, string? workspaceId, string? locationPath)
    {
        return scope switch
        {
            "global" => settingId,
            "workspace" => $"{workspaceId}:{settingId}",
            "location" => $"{locationPath}:{settingId}",
            _ => settingId
        };
    }

    private T? ConvertValue<T>(object value)
    {
        if (value is T typedValue)
        {
            return typedValue;
        }

        if (value is JsonElement element)
        {
            return ConvertJsonElement<T>(element);
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    private static T? ConvertJsonElement<T>(JsonElement element)
    {
        if (typeof(T) == typeof(List<object>) || typeof(T) == typeof(object))
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                var list = new List<object>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(item.ToString()!);
                }
                return (T)(object)list;
            }
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => (T)(object)element.GetString()!,
            JsonValueKind.Number when typeof(T) == typeof(int) => (T)(object)element.GetInt32(),
            JsonValueKind.Number when typeof(T) == typeof(long) => (T)(object)element.GetInt64(),
            JsonValueKind.Number when typeof(T) == typeof(double) => (T)(object)element.GetDouble(),
            JsonValueKind.True => (T)(object)true,
            JsonValueKind.False => (T)(object)false,
            JsonValueKind.Null => default,
            _ => (T)(object)element.ToString()!
        };
    }

    public SettingsSchema GetSchema() => _schema;
    public SettingsSnapshot GetSnapshot() => _currentSettings;

    public void ApplySnapshot(SettingsSnapshot snapshot)
    {
        var validation = _validationService.ValidateSettingsSnapshot(snapshot);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException($"Cannot apply invalid snapshot: {string.Join(", ", validation.Errors)}");
        }

        _currentSettings = snapshot;
        SettingChanged?.Invoke(this, new SettingChangedEventArgs
        {
            SettingId = "*",
            OldValue = null,
            NewValue = snapshot,
            Scope = "global"
        });
    }

    public async Task ResetToDefaultsAsync(string presetId = "personal")
    {
        await _telemetry.TrackEventAsync("SettingsReset", new Dictionary<string, string>
        {
            { "PresetId", presetId }
        });

        _currentSettings = new SettingsSnapshot
        {
            ActivePreset = presetId,
            CreatedAt = DateTime.UtcNow,
            SchemaVersion = _schema.SchemaVersion,
            GlobalSettings = new Dictionary<string, object>(),
            WorkspaceSettings = new Dictionary<string, Dictionary<string, object>>(),
            LocationSettings = new Dictionary<string, Dictionary<string, object>>()
        };

        await SaveSettingsAsync();
    }

    public async Task RefreshAsync()
    {
        await LoadSettingsAsync();
        SettingChanged?.Invoke(this, new SettingChangedEventArgs
        {
            SettingId = "*",
            OldValue = null,
            NewValue = _currentSettings,
            Scope = "global"
        });
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            if (!Directory.Exists(_dataPath))
                return false;

            if (!File.Exists(_settingsPath))
            {
                await CreateDefaultSettingsAsync();
            }

            var validation = _validationService.ValidateSettingsSnapshot(_currentSettings);
            return validation.IsValid;
        }
        catch
        {
            return false;
        }
    }

    private void OnSettingsError(SettingsErrorEventArgs args)
    {
        SettingsError?.Invoke(this, args);
    }

    public void Dispose()
    {
        _semaphore?.Wait(TimeSpan.FromSeconds(1));

        try
        {
            SaveSettingsAsync().Wait(TimeSpan.FromSeconds(2));
        }
        catch { }

        _semaphore?.Release();
        _semaphore?.Dispose();
    }
}

public class SettingsErrorEventArgs : EventArgs
{
    public string Operation { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}

public class ValidationService : IValidationService
{
    public ValidationResult ValidateSettingValue(SettingDefinition definition, object? value)
    {
        var errors = new List<string>();

        if (value == null && definition.ControlType != "button")
        {
            errors.Add($"Setting {definition.Id} cannot be null");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        switch (definition.ControlType.ToLowerInvariant())
        {
            case "dropdown":
            case "segmented":
                if (definition.AllowedValues != null && value is string str)
                {
                    if (!definition.AllowedValues.Contains(str))
                    {
                        errors.Add($"Value '{str}' is not in allowed values: {string.Join(", ", definition.AllowedValues)}");
                    }
                }
                break;

            case "slider":
                if (value is double d)
                {
                    if (definition.MinValue.HasValue && d < definition.MinValue.Value)
                    {
                        errors.Add($"Value {d} is below minimum {definition.MinValue}");
                    }
                    if (definition.MaxValue.HasValue && d > definition.MaxValue.Value)
                    {
                        errors.Add($"Value {d} is above maximum {definition.MaxValue}");
                    }
                }
                break;

            case "multi_select":
                if (value is List<object> list)
                {
                    if (definition.AllowedValues != null)
                    {
                        var invalid = list.OfType<string>().Where(v => !definition.AllowedValues.Contains(v)).ToList();
                        if (invalid.Any())
                        {
                            errors.Add($"Invalid values: {string.Join(", ", invalid)}");
                        }
                    }
                }
                break;
        }

        return errors.Any()
            ? new ValidationResult { IsValid = false, Errors = errors }
            : ValidationResult.Valid();
    }

    public ValidationResult ValidateSettingsSnapshot(SettingsSnapshot snapshot)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (snapshot == null)
        {
            errors.Add("Snapshot is null");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        if (string.IsNullOrEmpty(snapshot.SchemaVersion))
        {
            warnings.Add("Schema version is not set");
        }

        if (snapshot.GlobalSettings == null)
        {
            warnings.Add("Global settings collection is null");
        }

        if (snapshot.WorkspaceSettings == null)
        {
            warnings.Add("Workspace settings collection is null");
        }

        if (snapshot.LocationSettings == null)
        {
            warnings.Add("Location settings collection is null");
        }

        if (snapshot.CreatedAt == default)
        {
            warnings.Add("Created timestamp is not set");
        }

        return errors.Any()
            ? new ValidationResult { IsValid = false, Errors = errors, Warnings = warnings }
            : new ValidationResult { IsValid = true, Warnings = warnings };
    }
}

public class SettingChangedEventArgs : EventArgs
{
    public string SettingId { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public string Scope { get; set; } = string.Empty;
}
