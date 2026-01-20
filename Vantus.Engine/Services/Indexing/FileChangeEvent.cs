namespace Vantus.Engine.Services.Indexing;

public enum ChangeType
{
    Created,
    Modified,
    Deleted,
    Renamed
}

public class FileChangeEvent
{
    public string FilePath { get; set; } = string.Empty;
    public string? OldFilePath { get; set; } // For Renamed
    public ChangeType ChangeType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
