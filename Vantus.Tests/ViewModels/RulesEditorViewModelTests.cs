using Vantus.App.ViewModels;
using Vantus.Core.Models;
using Vantus.Core.Services;
using Xunit;
using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace Vantus.Tests.ViewModels;

public class RulesEditorViewModelTests : IDisposable
{
    private readonly string _testPath;
    private readonly RuleService _ruleService;

    public RulesEditorViewModelTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "VantusTests_RulesVM_" + Guid.NewGuid());
        Directory.CreateDirectory(_testPath);
        _ruleService = new RuleService(_testPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
            try { Directory.Delete(_testPath, true); } catch { }
    }

    [Fact]
    public async Task Save_Should_Fail_If_Rule_Name_Is_Empty()
    {
        var vm = new RulesEditorViewModel(_ruleService);
        vm.AddRuleCommand.Execute(null);
        var rule = vm.Rules[0];
        rule.Name = ""; // Invalid

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Rule name cannot be empty.", vm.StatusMessage);
    }

    [Fact]
    public async Task Save_Should_Fail_If_Condition_Operator_Missing()
    {
        var vm = new RulesEditorViewModel(_ruleService);
        vm.AddRuleCommand.Execute(null);
        var rule = vm.Rules[0];
        
        vm.AddNewConditionCommand.Execute(null);
        // Force empty operator to test validation
        rule.Conditions[0].Operator = "";
        
        await vm.SaveCommand.ExecuteAsync(null);

        Assert.StartsWith($"Rule '{rule.Name}': Missing operator", vm.StatusMessage);
    }

    [Fact]
    public async Task Save_Should_Fail_If_Action_Type_Missing()
    {
        var vm = new RulesEditorViewModel(_ruleService);
        vm.AddRuleCommand.Execute(null);
        var rule = vm.Rules[0];

        vm.AddNewActionCommand.Execute(null);
        // Force empty type to test validation
        rule.Actions[0].Type = "";
        
        await vm.SaveCommand.ExecuteAsync(null);

        Assert.StartsWith($"Rule '{rule.Name}': Missing action type", vm.StatusMessage);
    }

    [Fact]
    public async Task Save_Should_Succeed_If_Valid()
    {
        var vm = new RulesEditorViewModel(_ruleService);
        vm.AddRuleCommand.Execute(null);
        var rule = vm.Rules[0];

        vm.AddNewConditionCommand.Execute(null);
        rule.Conditions[0].Operator = "Contains";
        rule.Conditions[0].Field = "FileName";
        rule.Conditions[0].Value = "test";

        vm.AddNewActionCommand.Execute(null);
        rule.Actions[0].Type = "Delete";
        
        await vm.SaveCommand.ExecuteAsync(null);

        // Verify persistence
        var newService = new RuleService(_testPath);
        await newService.LoadRulesAsync();
        var savedRules = newService.GetRules();
        
        Assert.Single(savedRules);
        Assert.Equal(rule.Name, savedRules[0].Name);
        Assert.Equal("Delete", savedRules[0].Actions[0].Type);
    }
}