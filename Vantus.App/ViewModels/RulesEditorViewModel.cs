using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Vantus.Core.Engine;
using Vantus.Core.Models;

namespace Vantus.App.ViewModels;

public partial class RulesEditorViewModel : ObservableObject
{
    private readonly IEngineClient _engineClient;

    [ObservableProperty]
    private ObservableCollection<Rule> _rules = new();

    [ObservableProperty]
    private string _newRuleName = "";
    [ObservableProperty]
    private string _newRuleConditionType = "extension";
    [ObservableProperty]
    private string _newRuleConditionValue = "";
    [ObservableProperty]
    private string _newRuleActionType = "tag";
    [ObservableProperty]
    private string _newRuleActionValue = "";

    public RulesEditorViewModel(IEngineClient engineClient)
    {
        _engineClient = engineClient;
        _ = LoadRulesAsync();
    }

    public RulesEditorViewModel() : this(new StubEngineClient()) { }

    [RelayCommand]
    private async Task LoadRulesAsync()
    {
        var rules = await _engineClient.GetRulesAsync();
        Rules.Clear();
        foreach (var r in rules) Rules.Add(r);
    }

    [RelayCommand]
    private async Task AddRuleAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRuleName)) return;
        var r = new Rule
        {
            Name = NewRuleName,
            ConditionType = NewRuleConditionType,
            ConditionValue = NewRuleConditionValue,
            ActionType = NewRuleActionType,
            ActionValue = NewRuleActionValue,
            IsActive = true
        };
        await _engineClient.AddRuleAsync(r);
        NewRuleName = "";
        NewRuleConditionValue = "";
        NewRuleActionValue = "";
        await LoadRulesAsync();
    }

    [RelayCommand]
    private async Task DeleteRuleAsync(Rule rule)
    {
        if (rule == null) return;
        await _engineClient.DeleteRuleAsync(rule.Id);
        await LoadRulesAsync();
    }
}
