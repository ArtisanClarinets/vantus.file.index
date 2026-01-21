using System;
using System.Linq;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Vantus.Engine.Parsers;

public class PdfParser : IFileParser
{
    public bool CanParse(string extension) => extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public Task<string> ParseAsync(string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                using var document = PdfDocument.Open(filePath);
                var pages = document.GetPages().Select(p => p.Text);
                return string.Join(Environment.NewLine, pages);
            }
            catch
            {
                return string.Empty;
            }
        });
    }
}
