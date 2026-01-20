using System.Text.Json;
using Vantus.Core.Models;

namespace Vantus.Core.Services;

public class RuleService
{
    private readonly string _rulesPath;
    private List<AutomationRule> _rules = new();

    public RuleService(string dataPath)
    {
        _rulesPath = Path.Combine(dataPath, "rules.json");
    }

    public async Task InitializeAsync()
    {
        await LoadRulesAsync();
    }

    public async Task LoadRulesAsync()
    {
        if (File.Exists(_rulesPath))
        {
            try {
                var json = await File.ReadAllTextAsync(_rulesPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _rules = JsonSerializer.Deserialize<List<AutomationRule>>(json, options) ?? new();
            } catch { _rules = new(); }
        }
    }

    public async Task SaveRulesAsync()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_rules, options);
        await File.WriteAllTextAsync(_rulesPath, json);
    }

    public List<AutomationRule> GetRules() => _rules;
    
    public void AddRule(AutomationRule rule) {
        _rules.Add(rule);
    }
    
    public void UpdateRule(AutomationRule rule) {
        var idx = _rules.FindIndex(r => r.Id == rule.Id);
        if (idx >= 0) _rules[idx] = rule;
    }
    
    public void DeleteRule(string id) {
        _rules.RemoveAll(r => r.Id == id);
    }
}
