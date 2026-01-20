using System.Text.Json;
using Vantus.Core.Models;

namespace Vantus.Core.Services;

public class PolicyEngine
{
    private readonly string _dataPath;
    private readonly ITelemetryService _telemetry;
    private readonly IFileLockProvider? _fileLockProvider;
    private PolicyState _currentState = new();
    private const string PolicyFileName = "policies.json";

    public event EventHandler<PolicyChangedEventArgs>? PolicyChanged;

    public PolicyEngine(string dataPath, ITelemetryService? telemetry = null, IFileLockProvider? fileLockProvider = null)
    {
        _dataPath = dataPath;
        _telemetry = telemetry ?? new NullTelemetryService();
        _fileLockProvider = fileLockProvider;
    }

    public async Task InitializeAsync()
    {
        await LoadPolicyAsync();
    }

    private async Task LoadPolicyAsync()
    {
        var policyPath = Path.Combine(_dataPath, PolicyFileName);

        if (File.Exists(policyPath))
        {
            try
            {
                IDisposable? lockHandle = null;
                if (_fileLockProvider != null)
                {
                    lockHandle = _fileLockProvider.AcquireLock(policyPath, TimeSpan.FromSeconds(5));
                }

                var json = await File.ReadAllTextAsync(policyPath);
                var policyFile = JsonSerializer.Deserialize<PolicyFile>(json);
                if (policyFile != null)
                {
                    var previousState = _currentState;
                    _currentState = ConvertToState(policyFile);

                    if (!previousState.IsManaged && _currentState.IsManaged)
                    {
                        await _telemetry.TrackEventAsync("ManagedModeEnabled");
                        PolicyChanged?.Invoke(this, new PolicyChangedEventArgs
                        {
                            NewState = _currentState,
                            ChangeType = PolicyChangeType.ManagedModeEnabled
                        });
                    }
                }

                lockHandle?.Dispose();
            }
            catch (Exception ex)
            {
                _currentState = new PolicyState();
                await _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
                {
                    { "Operation", "LoadPolicyAsync" }
                });
            }
        }
    }

    public async Task LoadPolicyFromPathAsync(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                IDisposable? lockHandle = null;
                if (_fileLockProvider != null)
                {
                    lockHandle = _fileLockProvider.AcquireLock(path, TimeSpan.FromSeconds(5));
                }

                var json = await File.ReadAllTextAsync(path);
                var policyFile = JsonSerializer.Deserialize<PolicyFile>(json);
                if (policyFile != null)
                {
                    var previousState = _currentState;
                    _currentState = ConvertToState(policyFile);

                    if (previousState.IsManaged != _currentState.IsManaged)
                    {
                        await _telemetry.TrackEventAsync("PolicyLoaded", new Dictionary<string, string>
                        {
                            { "IsManaged", _currentState.IsManaged.ToString() },
                            { "LockCount", _currentState.ActiveLocks.Count.ToString() }
                        });
                    }
                }

                lockHandle?.Dispose();
            }
            catch (Exception ex)
            {
                await _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
                {
                    { "Operation", "LoadPolicyFromPathAsync" }
                });
                throw new InvalidOperationException("Failed to load policy file", ex);
            }
        }
    }

    private PolicyState ConvertToState(PolicyFile policyFile)
    {
        var locks = new Dictionary<string, PolicyLock>();

        foreach (var lockItem in policyFile.Locks)
        {
            locks[lockItem.SettingId] = lockItem;
        }

        return new PolicyState
        {
            IsManaged = policyFile.Managed,
            ActiveLocks = locks,
            AllowedLocations = policyFile.AllowedLocations ?? new List<string>(),
            BlockedExtensions = policyFile.BlockedExtensions ?? new List<string>(),
            UpdateChannel = policyFile.UpdateChannel,
            UpdateDeferralDays = policyFile.UpdateDeferralDays,
            LoadedAt = DateTime.UtcNow
        };
    }

    public PolicyState GetCurrentState() => _currentState;

    public bool IsSettingLocked(string settingId)
    {
        return _currentState.ActiveLocks.ContainsKey(settingId);
    }

    public LockedSettingState GetLockState(string settingId)
    {
        if (_currentState.ActiveLocks.TryGetValue(settingId, out var lockInfo))
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

    public object? GetEffectiveValue(string settingId, object? userValue)
    {
        if (_currentState.ActiveLocks.TryGetValue(settingId, out var lockInfo))
        {
            return lockInfo.LockedValue;
        }

        return userValue;
    }

    public bool CanUserChange(string settingId)
    {
        return !IsSettingLocked(settingId);
    }

    public List<PolicyLock> GetAllLocks()
    {
        return _currentState.ActiveLocks.Values.ToList();
    }

    public List<string> GetAllowedLocations()
    {
        return _currentState.AllowedLocations;
    }

    public List<string> GetBlockedExtensions()
    {
        return _currentState.BlockedExtensions;
    }

    public string? GetUpdateChannel()
    {
        return _currentState.UpdateChannel;
    }

    public int? GetUpdateDeferralDays()
    {
        return _currentState.UpdateDeferralDays;
    }

    public async Task ExportPolicyAsync(string path)
    {
        var policyFile = new PolicyFile
        {
            Managed = _currentState.IsManaged,
            Locks = _currentState.ActiveLocks.Values.ToList(),
            AllowedLocations = _currentState.AllowedLocations,
            BlockedExtensions = _currentState.BlockedExtensions,
            UpdateChannel = _currentState.UpdateChannel,
            UpdateDeferralDays = _currentState.UpdateDeferralDays
        };

        var json = JsonSerializer.Serialize(policyFile, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(path, json);
    }

    public PolicyFile ToPolicyFile()
    {
        return new PolicyFile
        {
            Managed = _currentState.IsManaged,
            Locks = _currentState.ActiveLocks.Values.ToList(),
            AllowedLocations = _currentState.AllowedLocations,
            BlockedExtensions = _currentState.BlockedExtensions,
            UpdateChannel = _currentState.UpdateChannel,
            UpdateDeferralDays = _currentState.UpdateDeferralDays
        };
    }
}

public class PolicyChangedEventArgs : EventArgs
{
    public PolicyState NewState { get; set; } = new();
    public PolicyChangeType ChangeType { get; set; }
}

public enum PolicyChangeType
{
    ManagedModeEnabled,
    ManagedModeDisabled,
    LocksUpdated,
    SettingsChanged
}
