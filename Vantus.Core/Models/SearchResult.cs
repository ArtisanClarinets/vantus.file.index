using System.Collections.Generic;

namespace Vantus.Core.Models;

public class SearchResult
{
    public string Path { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
