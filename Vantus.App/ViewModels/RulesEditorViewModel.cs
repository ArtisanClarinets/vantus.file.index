using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Vantus.Core.Models;
using Vantus.Core.Services;

namespace Vantus.App.ViewModels;

public partial class RulesEditorViewModel : ObservableObject
{
    private readonly RuleService _ruleService;

    [ObservableProperty]
    private ObservableCollection<AutomationRule> _rules = new();

    [ObservableProperty]
    private AutomationRule? _selectedRule;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private ObservableCollection<RuleCondition> _currentConditions = new();

    [ObservableProperty]
    private ObservableCollection<RuleAction> _currentActions = new();

    public ObservableCollection<string> AvailableFields { get; } = new() 
    { 
        "FileName", "Extension", "Size", "Content", "DateModified", "DateCreated" 
    };

    public ObservableCollection<string> AvailableOperators { get; } = new() 
    { 
        "Contains", "Equals", "StartsWith", "EndsWith", "GreaterThan", "LessThan", "Regex" 
    };

    public ObservableCollection<string> AvailableActionTypes { get; } = new() 
    { 
        "Move", "Copy", "Delete", "Rename", "Tag", "Archive" 
    };

    public RulesEditorViewModel(RuleService ruleService)
    {
        _ruleService = ruleService;
        LoadRules();
    }

    private void LoadRules()
    {
        Rules = new ObservableCollection<AutomationRule>(_ruleService.GetRules());
    }

    partial void OnSelectedRuleChanged(AutomationRule? value)
    {
        if (value != null)
        {
            CurrentConditions = new ObservableCollection<RuleCondition>(value.Conditions);
            CurrentActions = new ObservableCollection<RuleAction>(value.Actions);
        }
        else
        {
            CurrentConditions.Clear();
            CurrentActions.Clear();
        }
    }

    [RelayCommand]
    private void AddRule()
    {
        var newRule = new AutomationRule { Name = "New Rule " + (Rules.Count + 1) };
        _ruleService.AddRule(newRule);
        Rules.Add(newRule);
        SelectedRule = newRule;
    }

    [RelayCommand]
    private void DeleteRule(AutomationRule? rule)
    {
        if (rule == null) return;
        _ruleService.DeleteRule(rule.Id);
        Rules.Remove(rule);
        if (SelectedRule == rule) SelectedRule = null;
    }

    [RelayCommand]
    private void AddNewCondition()
    {
        if (SelectedRule == null) return;
        var condition = new RuleCondition();
        SelectedRule.Conditions.Add(condition);
        CurrentConditions.Add(condition);
    }

    [RelayCommand]
    private void RemoveCondition(RuleCondition? condition)
    {
        if (SelectedRule == null || condition == null) return;
        SelectedRule.Conditions.Remove(condition);
        CurrentConditions.Remove(condition);
    }

    [RelayCommand]
    private void AddNewAction()
    {
        if (SelectedRule == null) return;
        var action = new RuleAction();
        SelectedRule.Actions.Add(action);
        CurrentActions.Add(action);
    }

    [RelayCommand]
    private void RemoveAction(RuleAction? action)
    {
        if (SelectedRule == null || action == null) return;
        SelectedRule.Actions.Remove(action);
        CurrentActions.Remove(action);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var error = ValidateRules();
        if (error != null)
        {
            StatusMessage = error;
            return;
        }

        await _ruleService.SaveRulesAsync();
        StatusMessage = "Rules saved successfully.";
        await Task.Delay(2000);
        StatusMessage = "";
    }

    private string? ValidateRules()
    {
        foreach (var rule in Rules)
        {
            if (string.IsNullOrWhiteSpace(rule.Name))
                return "Rule name cannot be empty.";

            foreach (var condition in rule.Conditions)
            {
                if (string.IsNullOrWhiteSpace(condition.Operator))
                    return $"Rule '{rule.Name}': Missing operator in condition.";
            }

            foreach (var action in rule.Actions)
            {
                if (string.IsNullOrWhiteSpace(action.Type))
                    return $"Rule '{rule.Name}': Missing action type.";
            }
        }
        return null;
    }
}
