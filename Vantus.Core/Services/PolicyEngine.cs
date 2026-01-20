using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vantus.Core.Services;

public class PolicyEngine
{
    private readonly string _policyPath;
    private Dictionary<string, PolicyRule> _policies = new();

    public PolicyEngine(string dataPath)
    {
        _policyPath = Path.Combine(dataPath, "policies.json");
    }

    public async Task InitializeAsync()
    {
        if (File.Exists(_policyPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_policyPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _policies = JsonSerializer.Deserialize<Dictionary<string, PolicyRule>>(json, options) 
                           ?? new Dictionary<string, PolicyRule>();
            }
            catch
            {
                // Ignore
            }
        }
    }

    public PolicyRule? GetPolicy(string settingId)
    {
        if (_policies.TryGetValue(settingId, out var policy)) return policy;
        return null;
    }
    
    public bool IsManaged => _policies.Count > 0;
}

public class PolicyRule
{
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = "";
}
