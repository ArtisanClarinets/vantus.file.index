using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vantus.Engine.Data.Entities;

[Table("FileTags")]
public class FileTag
{
    public long FileId { get; set; }
    public FileIndexItem File { get; set; } = null!;

    public long TagId { get; set; }
    public Tag Tag { get; set; } = null!;

    public double Confidence { get; set; } = 1.0;
}
