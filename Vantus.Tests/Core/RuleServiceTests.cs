using Vantus.Core.Models;
using Vantus.Core.Services;
using Xunit;
using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace Vantus.Tests.Core;

public class RuleServiceTests : IDisposable
{
    private readonly string _testPath;

    public RuleServiceTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "VantusTests_Rules_" + Guid.NewGuid());
        Directory.CreateDirectory(_testPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
            try { Directory.Delete(_testPath, true); } catch {}
    }

    [Fact]
    public async Task Can_Save_And_Load_Rules()
    {
        var service = new RuleService(_testPath);
        var rule = new AutomationRule { Name = "Test Rule" };
        rule.Conditions.Add(new RuleCondition { Field = "FileName", Operator = "Contains", Value = "test" });
        rule.Actions.Add(new RuleAction { Type = "Move", Target = "C:\\Test" });

        service.AddRule(rule);
        await service.SaveRulesAsync();

        var newService = new RuleService(_testPath);
        await newService.LoadRulesAsync();
        var rules = newService.GetRules();

        Assert.Single(rules);
        Assert.Equal("Test Rule", rules[0].Name);
        Assert.Single(rules[0].Conditions);
        Assert.Single(rules[0].Actions);
        Assert.Equal("FileName", rules[0].Conditions[0].Field);
        Assert.Equal("Move", rules[0].Actions[0].Type);
    }

    [Fact]
    public void Can_Update_Rule()
    {
        var service = new RuleService(_testPath);
        var rule = new AutomationRule { Name = "Old Name" };
        service.AddRule(rule);

        rule.Name = "New Name";
        service.UpdateRule(rule);

        Assert.Equal("New Name", service.GetRules()[0].Name);
    }

    [Fact]
    public void Can_Delete_Rule()
    {
        var service = new RuleService(_testPath);
        var rule = new AutomationRule();
        service.AddRule(rule);
        Assert.Single(service.GetRules());

        service.DeleteRule(rule.Id);
        Assert.Empty(service.GetRules());
    }
}
