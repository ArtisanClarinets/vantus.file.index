using System.Collections.Concurrent;
using System.Text.Json;
using Vantus.Core.Models;

namespace Vantus.Core.Services;

public class SettingsStore
{
    private readonly string _filePath;
    private readonly SettingsSchema _schema;
    private readonly PolicyEngine _policyEngine;
    private ConcurrentDictionary<string, object> _userValues = new();
    
    public event EventHandler<string>? SettingChanged;

    public SettingsStore(string dataPath, SettingsSchema schema, PolicyEngine policyEngine)
    {
        _filePath = Path.Combine(dataPath, "settings.json");
        _schema = schema;
        _policyEngine = policyEngine;
    }

    public async Task InitializeAsync()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (data != null)
                {
                    foreach (var kvp in data)
                    {
                        if (kvp.Key == "schema_version") continue;
                        _userValues[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch
            {
                // Ignore corruption
            }
        }
    }

    public object? GetUserValue(string settingId)
    {
        if (_userValues.TryGetValue(settingId, out var val)) return val;
        return null;
    }

    public void SetUserValue(string settingId, object value)
    {
        // Check policy?
        var policy = _policyEngine.GetPolicy(settingId);
        if (policy != null && policy.Locked) return; // Cannot change if locked

        _userValues[settingId] = value;
        SettingChanged?.Invoke(this, settingId);
        _ = SaveAsync();
    }

    public async Task SaveAsync()
    {
        var data = new Dictionary<string, object>(_userValues);
        data["schema_version"] = _schema.SchemaVersion;
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        
        await File.WriteAllTextAsync(_filePath, json);
    }
    
    public Dictionary<string, object> GetAllUserValues() => new(_userValues);
    
    public void ImportValues(Dictionary<string, object> values)
    {
        foreach(var kvp in values)
        {
            SetUserValue(kvp.Key, kvp.Value);
        }
        _ = SaveAsync();
        SettingChanged?.Invoke(this, string.Empty);
    }
    
    public void ResetToDefaults()
    {
        _userValues.Clear();
        _ = SaveAsync();
        SettingChanged?.Invoke(this, string.Empty);
    }
    
    // Compatibility & Logic
    public object? GetValue(string id)
    {
        // 1. Policy
        var policy = _policyEngine.GetPolicy(id);
        if (policy != null && policy.Locked) return policy.Value;
        
        // 2. User
        if (_userValues.TryGetValue(id, out var val)) return val;
        
        // 3. Default (Personal)
        return _schema.GetDefault(id, "personal");
    }

    public (object? Value, bool IsLocked, string LockReason) GetEffectiveSetting(string id)
    {
        var policy = _policyEngine.GetPolicy(id);
        if (policy != null && policy.Locked)
        {
             return (policy.Value, true, policy.Reason);
        }
        
        return (GetValue(id), false, "");
    }

    public void SetValue(string id, object val) => SetUserValue(id, val);
    public Task SaveSettingsAsync() => SaveAsync();
    public Dictionary<string, object> GetSnapshot() => GetAllUserValues();
    public void ApplySnapshot(Dictionary<string, object> snapshot) => ImportValues(snapshot);
}
