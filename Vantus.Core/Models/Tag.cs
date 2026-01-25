using System.Text.Json.Serialization;

namespace Vantus.Core.Models;

public class Tag
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}
