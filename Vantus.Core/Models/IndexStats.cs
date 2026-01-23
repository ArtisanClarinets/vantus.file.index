namespace Vantus.Core.Models;

public class IndexStats
{
    public long FilesIndexed { get; set; }
    public long TotalTags { get; set; }
    public long TotalPartners { get; set; }
    public int QueueLength { get; set; }
    public string? LastError { get; set; }
    public string Status { get; set; } = "Idle";
}
