using System.Text.Json.Serialization;

namespace Vantus.Core.Models;

public class SettingsSchema
{
    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = "1.0";

    [JsonPropertyName("last_updated")]
    public string LastUpdated { get; set; } = string.Empty;

    [JsonPropertyName("presets")]
    public Dictionary<string, string> Presets { get; set; } = new();

    [JsonPropertyName("categories")]
    public Dictionary<string, SettingsCategory> Categories { get; set; } = new();
}

public class SettingsCategory
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("visibility")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Visibility { get; set; }

    [JsonPropertyName("pages")]
    public Dictionary<string, SettingsPage> Pages { get; set; } = new();
}

public class SettingsPage
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("settings")]
    public List<SettingDefinition> Settings { get; set; } = new();
}

public class SettingDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("control_type")]
    public string ControlType { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("helper_text")]
    public string HelperText { get; set; } = string.Empty;

    [JsonPropertyName("button_text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ButtonText { get; set; }

    [JsonPropertyName("allowed_values")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? AllowedValues { get; set; }

    [JsonPropertyName("min_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MinValue { get; set; }

    [JsonPropertyName("max_value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MaxValue { get; set; }

    [JsonPropertyName("step")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Step { get; set; }

    [JsonPropertyName("suffix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Suffix { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "global";

    [JsonPropertyName("requires_restart")]
    public bool RequiresRestart { get; set; }

    [JsonPropertyName("policy_lockable")]
    public bool PolicyLockable { get; set; }

    [JsonPropertyName("visibility")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Visibility { get; set; }

    [JsonPropertyName("defaults")]
    public Dictionary<string, object> Defaults { get; set; } = new();
}
