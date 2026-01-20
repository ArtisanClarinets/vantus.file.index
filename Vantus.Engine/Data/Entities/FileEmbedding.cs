using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Vantus.Engine.Data.Entities;

[Table("FileEmbeddings")]
public class FileEmbedding
{
    [Key]
    public long Id { get; set; }

    public long FileId { get; set; }
    
    [ForeignKey("FileId")]
    public FileIndexItem File { get; set; } = null!;

    /// <summary>
    /// Serialized vector data (e.g., float array as JSON or bytes).
    /// </summary>
    [Required]
    public string VectorJson { get; set; } = "[]";

    [MaxLength(50)]
    public string ModelName { get; set; } = "all-MiniLM-L6-v2";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Helper to get/set vector as float array.
    /// </summary>
    [NotMapped]
    public float[] Vector
    {
        get => JsonSerializer.Deserialize<float[]>(VectorJson) ?? Array.Empty<float>();
        set => VectorJson = JsonSerializer.Serialize(value);
    }
}
