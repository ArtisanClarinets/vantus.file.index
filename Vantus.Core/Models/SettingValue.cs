using System.Text.Json.Serialization;

namespace Vantus.Core.Models;

public class SettingValue
{
    [JsonPropertyName("setting_id")]
    public string SettingId { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "global";

    [JsonPropertyName("workspace_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("location_path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LocationPath { get; set; }

    [JsonPropertyName("last_modified")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("source")]
    public SettingSource Source { get; set; } = SettingSource.User;
}

public enum SettingSource
{
    User,
    Preset,
    Policy,
    Default
}

public class SettingsSnapshot
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = "1.0";

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("active_preset")]
    public string ActivePreset { get; set; } = "personal";

    [JsonPropertyName("global_settings")]
    public Dictionary<string, object> GlobalSettings { get; set; } = new();

    [JsonPropertyName("workspace_settings")]
    public Dictionary<string, Dictionary<string, object>> WorkspaceSettings { get; set; } = new();

    [JsonPropertyName("location_settings")]
    public Dictionary<string, Dictionary<string, object>> LocationSettings { get; set; } = new();
}
