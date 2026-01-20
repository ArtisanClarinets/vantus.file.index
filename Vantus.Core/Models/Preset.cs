using System.Text.Json.Serialization;

namespace Vantus.Core.Models;

public class Preset
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("intent")]
    public string Intent { get; set; } = string.Empty;

    [JsonPropertyName("settings")]
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class PresetDefinition
{
    [JsonPropertyName("presets")]
    public List<Preset> Presets { get; set; } = new();
}

public class PresetChange
{
    public string SettingId { get; set; } = string.Empty;
    public string SettingLabel { get; set; } = string.Empty;
    public object? CurrentValue { get; set; }
    public object? NewValue { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
}

public class PresetDiff
{
    public string PresetId { get; set; } = string.Empty;
    public string PresetName { get; set; } = string.Empty;
    public List<PresetChange> Changes { get; set; } = new();
    public int TotalChanges => Changes.Count;
    public int AffectedPages => Changes.Select(c => c.Page).Distinct().Count();
}
