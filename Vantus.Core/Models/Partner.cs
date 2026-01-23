namespace Vantus.Core.Models;

public class Partner
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Domains { get; set; }
    public string? Keywords { get; set; }
}
