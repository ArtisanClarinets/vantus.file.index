using System.Text.Json.Serialization;

namespace Vantus.Core.Models;

public class PolicyFile
{
    [JsonPropertyName("managed")]
    public bool Managed { get; set; }

    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = "1.0";

    [JsonPropertyName("locks")]
    public List<PolicyLock> Locks { get; set; } = new();

    [JsonPropertyName("allowed_locations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? AllowedLocations { get; set; }

    [JsonPropertyName("blocked_extensions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? BlockedExtensions { get; set; }

    [JsonPropertyName("update_channel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UpdateChannel { get; set; }

    [JsonPropertyName("update_deferral_days")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? UpdateDeferralDays { get; set; }
}

public class PolicyLock
{
    [JsonPropertyName("setting_id")]
    public string SettingId { get; set; } = string.Empty;

    [JsonPropertyName("locked_value")]
    public object LockedValue { get; set; } = null!;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}

public class PolicyState
{
    public bool IsManaged { get; set; }
    public Dictionary<string, PolicyLock> ActiveLocks { get; set; } = new();
    public List<string> AllowedLocations { get; set; } = new();
    public List<string> BlockedExtensions { get; set; } = new();
    public string? UpdateChannel { get; set; }
    public int? UpdateDeferralDays { get; set; }
    public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
}

public class LockedSettingState
{
    public string SettingId { get; set; } = string.Empty;
    public object LockedValue { get; set; } = null!;
    public string Reason { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool IsLocked => !string.IsNullOrEmpty(Source);
}
