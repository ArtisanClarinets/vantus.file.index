using System.Text.Json.Serialization;

namespace Vantus.Core.Models;

public class AutomationRule
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "New Rule";

    [JsonPropertyName("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [JsonPropertyName("conditions")]
    public List<RuleCondition> Conditions { get; set; } = new();

    [JsonPropertyName("actions")]
    public List<RuleAction> Actions { get; set; } = new();

    [JsonPropertyName("logic")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RuleLogic Logic { get; set; } = RuleLogic.And;
}

public enum RuleLogic
{
    And,
    Or
}

public class RuleCondition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("field")]
    public string Field { get; set; } = "FileName"; // e.g., "FileName", "Extension", "Size", "Content"

    [JsonPropertyName("operator")]
    public string Operator { get; set; } = "Contains"; // e.g., "Contains", "Equals", "StartsWith", "GreaterThan"

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";
}

public class RuleAction
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("type")]
    public string Type { get; set; } = "Move"; // e.g., "Move", "Copy", "Delete", "Tag"

    [JsonPropertyName("target")]
    public string Target { get; set; } = ""; // e.g., Destination path or Tag name
}
