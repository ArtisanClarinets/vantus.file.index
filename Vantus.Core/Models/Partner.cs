using System.Text.Json.Serialization;

namespace Vantus.Core.Models;

public class Partner
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "Client";

    [JsonPropertyName("default_destination")]
    public string DefaultDestination { get; set; } = "";

    [JsonPropertyName("is_pinned")]
    public bool IsPinned { get; set; }
}
