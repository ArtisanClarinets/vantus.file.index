using System.Text.Json.Serialization;

namespace Vantus.Core.Models;

public class SettingsRegistry
{
    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = "1.0";

    [JsonPropertyName("presets")]
    public Dictionary<string, string> Presets { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<SettingsCategory> Categories { get; set; } = new();

    [JsonPropertyName("pages")]
    public List<SettingsPageDefinition> Pages { get; set; } = new();

    [JsonPropertyName("settings")]
    public List<SettingDefinition> Settings { get; set; } = new();
}

public class SettingsCategory
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("page_ids")]
    public List<string> PageIds { get; set; } = new();
}

public class SettingsPageDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("intro")]
    public string Intro { get; set; } = "";
}

public class SettingDefinition
{
    [JsonPropertyName("setting_id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("page")]
    public string PageId { get; set; } = "";

    [JsonPropertyName("section")]
    public string Section { get; set; } = "";

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("helper_text")]
    public string HelperText { get; set; } = "";

    [JsonPropertyName("control_type")]
    public string ControlType { get; set; } = "";

    [JsonPropertyName("value_type")]
    public string ValueType { get; set; } = "";

    [JsonPropertyName("allowed_values")]
    public object? AllowedValues { get; set; }

    [JsonPropertyName("defaults")]
    public Dictionary<string, object> Defaults { get; set; } = new();

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "global";

    [JsonPropertyName("requires_restart")]
    public bool RequiresRestart { get; set; }

    [JsonPropertyName("policy_lockable")]
    public bool PolicyLockable { get; set; }

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; } = "all";
}
