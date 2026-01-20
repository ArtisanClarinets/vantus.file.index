using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vantus.Engine.Data.Entities;

[Table("FileIndexItems")]
public class FileIndexItem
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(4096)] // Windows max path is roughly 32k, but 4k is a reasonable limit for index
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Extension { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public DateTime LastScannedAt { get; set; }

    [MaxLength(128)]
    public string? ContentHash { get; set; }

    /// <summary>
    /// Stores extracted metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }
    
    /// <summary>
    /// Extracted text content for search.
    /// </summary>
    public string? Content { get; set; }

    // Navigation properties for tags can be added later
    public ICollection<FileTag> FileTags { get; set; } = new List<FileTag>();
}
