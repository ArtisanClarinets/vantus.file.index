using System.Text.Json.Serialization;

namespace Vantus.Core.Models;

public class FileResult
{
    [JsonPropertyName("file_path")]
    public string FilePath { get; set; } = "";

    [JsonPropertyName("score")]
    public double Score { get; set; }
}
