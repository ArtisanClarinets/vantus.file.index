using System.Text.Json.Serialization;

namespace Vantus.Core.Models;

public class IndexStats
{
    [JsonPropertyName("files_indexed")]
    public int FilesIndexed { get; set; }

    [JsonPropertyName("total_files")]
    public int TotalFiles { get; set; }
}
