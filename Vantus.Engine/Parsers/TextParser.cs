using System;
using System.IO;
using System.Threading.Tasks;

namespace Vantus.Engine.Parsers;

public class TextParser : IFileParser
{
    public bool CanParse(string extension) =>
        extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".cs", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".log", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ParseAsync(string filePath)
    {
        try
        {
            return await File.ReadAllTextAsync(filePath);
        }
        catch
        {
            return string.Empty;
        }
    }
}
