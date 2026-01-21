using System.Threading.Tasks;

namespace Vantus.Engine.Parsers;

public interface IFileParser
{
    bool CanParse(string extension);
    Task<string> ParseAsync(string filePath);
}
