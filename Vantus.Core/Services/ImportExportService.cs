using System.Text.Json;
using System.Text.Json.Serialization;
using Vantus.Core.Models;

namespace Vantus.Core.Services;

public class ImportExportService
{
    private readonly SettingsStore _settingsStore;
    private readonly SettingsSchema _schema;
    private readonly ITelemetryService _telemetry;
    private readonly IRetryPolicy _retryPolicy;

    public ImportExportService(
        SettingsStore settingsStore, 
        SettingsSchema schema, 
        ITelemetryService? telemetry = null,
        IRetryPolicy? retryPolicy = null)
    {
        _settingsStore = settingsStore;
        _schema = schema;
        _telemetry = telemetry ?? new NullTelemetryService();
        _retryPolicy = retryPolicy ?? new ExponentialBackoffRetryPolicy();
    }

    public async Task<ExportResult> ExportSettingsAsync(string? workspaceId = null)
    {
        try
        {
            var snapshot = _settingsStore.GetSnapshot();
            var export = new SettingsExport
            {
                Version = "1.0",
                SchemaVersion = _schema.SchemaVersion,
                ExportedAt = DateTime.UtcNow,
                ActivePreset = snapshot.ActivePreset
            };

            if (workspaceId == null)
            {
                export.GlobalSettings = snapshot.GlobalSettings;
            }
            else
            {
                export.WorkspaceSettings = new Dictionary<string, Dictionary<string, object>>
                {
                    { workspaceId, snapshot.WorkspaceSettings.TryGetValue(workspaceId, out var ws) ? ws : new Dictionary<string, object>() }
                };
            }

            var json = JsonSerializer.Serialize(export, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await _telemetry.TrackEventAsync("SettingsExported", new Dictionary<string, string>
            {
                { "WorkspaceId", workspaceId ?? "global" },
                { "SettingCount", export.GlobalSettings.Count.ToString() }
            });

            return new ExportResult
            {
                Success = true,
                Content = json,
                FileName = $"vantus-settings-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json"
            };
        }
        catch (Exception ex)
        {
            await _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
            {
                { "Operation", "ExportSettingsAsync" }
            });
            return new ExportResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public ImportPreview PreviewImport(string jsonContent)
    {
        var preview = new ImportPreview();

        try
        {
            var export = JsonSerializer.Deserialize<SettingsExport>(jsonContent);
            if (export == null)
            {
                preview.IsValid = false;
                preview.ErrorMessage = "Invalid file format";
                return preview;
            }

            preview.IsValid = true;
            preview.SchemaVersion = export.SchemaVersion;
            preview.ExportedAt = export.ExportedAt;

            var currentSnapshot = _settingsStore.GetSnapshot();
            var changes = new List<ImportChange>();

            foreach (var kvp in export.GlobalSettings)
            {
                var currentValue = currentSnapshot.GlobalSettings.TryGetValue(kvp.Key, out var cv) ? cv : null;
                if (!ValuesEqual(currentValue, kvp.Value))
                {
                    changes.Add(new ImportChange
                    {
                        SettingId = kvp.Key,
                        SettingLabel = GetSettingLabel(kvp.Key),
                        Scope = "global",
                        CurrentValue = currentValue,
                        NewValue = kvp.Value,
                        ChangeType = currentValue == null ? ChangeType.Added : ChangeType.Modified
                    });
                }
            }

            foreach (var wsKvp in export.WorkspaceSettings)
            {
                foreach (var kvp in wsKvp.Value)
                {
                    var currentWs = currentSnapshot.WorkspaceSettings.TryGetValue(wsKvp.Key, out var cws) ? cws : new Dictionary<string, object>();
                    var currentValue = currentWs.TryGetValue(kvp.Key, out var cv) ? cv : null;

                    if (!ValuesEqual(currentValue, kvp.Value))
                    {
                        changes.Add(new ImportChange
                        {
                            SettingId = kvp.Key,
                            SettingLabel = GetSettingLabel(kvp.Key),
                            Scope = "workspace",
                            WorkspaceId = wsKvp.Key,
                            CurrentValue = currentValue,
                            NewValue = kvp.Value,
                            ChangeType = currentValue == null ? ChangeType.Added : ChangeType.Modified
                        });
                    }
                }
            }

            preview.Changes = changes;
            preview.TotalChanges = changes.Count;
            preview.AffectedSettings = changes.Select(c => c.SettingId).Distinct().Count();
        }
        catch (Exception ex)
        {
            preview.IsValid = false;
            preview.ErrorMessage = $"Failed to parse file: {ex.Message}";
        }

        return preview;
    }

    public async Task<ImportResult> ImportSettingsAsync(string jsonContent, bool apply = true)
    {
        var result = new ImportResult();

        try
        {
            var export = JsonSerializer.Deserialize<SettingsExport>(jsonContent);
            if (export == null)
            {
                result.Success = false;
                result.ErrorMessage = "Invalid file format";
                await _telemetry.TrackEventAsync("ImportFailed", new Dictionary<string, string>
                {
                    { "Reason", "InvalidFileFormat" }
                });
                return result;
            }

            if (apply)
            {
                var snapshot = _settingsStore.GetSnapshot();

                foreach (var kvp in export.GlobalSettings)
                {
                    snapshot.GlobalSettings[kvp.Key] = kvp.Value;
                }

                foreach (var wsKvp in export.WorkspaceSettings)
                {
                    if (!snapshot.WorkspaceSettings.ContainsKey(wsKvp.Key))
                    {
                        snapshot.WorkspaceSettings[wsKvp.Key] = new Dictionary<string, object>();
                    }
                    foreach (var kvp in wsKvp.Value)
                    {
                        snapshot.WorkspaceSettings[wsKvp.Key][kvp.Key] = kvp.Value;
                    }
                }

                snapshot.ActivePreset = export.ActivePreset;
                _settingsStore.ApplySnapshot(snapshot);
                await _settingsStore.SaveSettingsAsync();

                await _telemetry.TrackEventAsync("SettingsImported", new Dictionary<string, string>
                {
                    { "Apply", apply.ToString() },
                    { "GlobalSettingsCount", export.GlobalSettings.Count.ToString() },
                    { "WorkspaceCount", export.WorkspaceSettings.Count.ToString() }
                });
            }

            result.Success = true;
            result.ImportedSettings = export.GlobalSettings.Count +
                export.WorkspaceSettings.Values.Sum(ws => ws.Count);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Import failed: {ex.Message}";
            await _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
            {
                { "Operation", "ImportSettingsAsync" }
            });
        }

        return result;
    }

    private string GetSettingLabel(string settingId)
    {
        foreach (var category in _schema.Categories)
        {
            foreach (var page in category.Value.Pages)
            {
                var setting = page.Value.Settings.FirstOrDefault(s => s.Id == settingId);
                if (setting != null)
                {
                    return setting.Label;
                }
            }
        }
        return settingId;
    }

    private bool ValuesEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        if (a is List<object> listA && b is List<object> listB)
        {
            if (listA.Count != listB.Count) return false;
            return listA.SequenceEqual(listB);
        }

        return a.Equals(b);
    }
}

public class SettingsExport
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = "1.0";

    [JsonPropertyName("exported_at")]
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("active_preset")]
    public string ActivePreset { get; set; } = "personal";

    [JsonPropertyName("global_settings")]
    public Dictionary<string, object> GlobalSettings { get; set; } = new();

    [JsonPropertyName("workspace_settings")]
    public Dictionary<string, Dictionary<string, object>> WorkspaceSettings { get; set; } = new();
}

public class ImportPreview
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string SchemaVersion { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public List<ImportChange> Changes { get; set; } = new();
    public int TotalChanges { get; set; }
    public int AffectedSettings { get; set; }
}

public class ImportChange
{
    public string SettingId { get; set; } = string.Empty;
    public string SettingLabel { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string? WorkspaceId { get; set; }
    public object? CurrentValue { get; set; }
    public object? NewValue { get; set; }
    public ChangeType ChangeType { get; set; }
}

public enum ChangeType
{
    Added,
    Modified,
    Removed
}

public class ImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int ImportedSettings { get; set; }
}

public class ExportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Content { get; set; }
    public string? FileName { get; set; }
}
