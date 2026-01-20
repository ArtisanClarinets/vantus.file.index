using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vantus.Engine.Data.Entities;

[Table("Tags")]
public class Tag
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The source of the tag (e.g., "User", "AI", "Rule").
    /// </summary>
    [MaxLength(50)]
    public string Source { get; set; } = "User";

    public ICollection<FileTag> FileTags { get; set; } = new List<FileTag>();
}
