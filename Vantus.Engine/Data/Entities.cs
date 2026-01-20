using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vantus.Engine.Data;

public class FileEntity
{
    [Key]
    public int Id { get; set; }
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    public string Extension { get; set; } = "";
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime LastIndexed { get; set; }
    public string ContentHash { get; set; } = "";
    public string? TextContent { get; set; }
    
    public ICollection<EmbeddingEntity> Embeddings { get; set; } = new List<EmbeddingEntity>();
    public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
}

public class EmbeddingEntity
{
    [Key]
    public int Id { get; set; }
    public int FileId { get; set; }
    [ForeignKey("FileId")]
    public FileEntity File { get; set; } = null!;
    public string ModelName { get; set; } = "";
    public byte[] Vector { get; set; } = Array.Empty<byte>(); 
    public int ChunkIndex { get; set; }
    public string? ChunkText { get; set; }
}

public class TagEntity
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ICollection<FileEntity> Files { get; set; } = new List<FileEntity>();
}

public class IndexQueueItem
{
    [Key]
    public int Id { get; set; }
    public string Path { get; set; } = "";
    public string Action { get; set; } = "Index"; 
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}
