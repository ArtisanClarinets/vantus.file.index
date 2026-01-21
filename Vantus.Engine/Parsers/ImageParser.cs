using System;
using System.IO;
using System.Threading.Tasks;
using Tesseract;

namespace Vantus.Engine.Parsers;

public class ImageParser : IFileParser
{
    public bool CanParse(string extension) =>
        extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".tif", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".tiff", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase);

    public Task<string> ParseAsync(string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                // Note: Tesseract requires 'tessdata' folder with language files in the app directory.
                // For this environment, we assume it's present or fail gracefully.
                var tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
                if (!Directory.Exists(tessDataPath))
                {
                    // Fallback: If no OCR, return basic file info as text
                    return $"[OCR Unavailable: Missing tessdata] Filename: {Path.GetFileName(filePath)}";
                }

                using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(filePath);
                using var page = engine.Process(img);
                return page.GetText();
            }
            catch (Exception)
            {
                // Return empty string on error so indexing doesn't crash
                return string.Empty;
            }
        });
    }
}
