using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Vantus.Engine.Parsers;

public class OfficeParser : IFileParser
{
    public bool CanParse(string extension) =>
        extension.Equals(".docx", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".pptx", StringComparison.OrdinalIgnoreCase);

    public Task<string> ParseAsync(string filePath)
    {
        return Task.Run(() =>
        {
            var extension = System.IO.Path.GetExtension(filePath);
            try
            {
                if (extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    using var doc = WordprocessingDocument.Open(filePath, false);
                    var body = doc.MainDocumentPart?.Document.Body;
                    return body?.InnerText ?? string.Empty;
                }
                else if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    using var doc = SpreadsheetDocument.Open(filePath, false);
                    var workbookPart = doc.WorkbookPart;
                    var sharedStringTable = workbookPart?.SharedStringTablePart?.SharedStringTable;
                    var sb = new StringBuilder();

                    if (workbookPart != null)
                    {
                        foreach (var worksheetPart in workbookPart.WorksheetParts)
                        {
                            var reader = DocumentFormat.OpenXml.OpenXmlReader.Create(worksheetPart);
                            while (reader.Read())
                            {
                                if (reader.ElementType == typeof(Cell))
                                {
                                    var cell = (Cell)reader.LoadCurrentElement();
                                    if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                                    {
                                        if (int.TryParse(cell.CellValue?.Text, out int id))
                                        {
                                            sb.Append(sharedStringTable?.ElementAt(id).InnerText + " ");
                                        }
                                    }
                                    else if (cell.CellValue != null)
                                    {
                                        sb.Append(cell.CellValue.Text + " ");
                                    }
                                }
                            }
                        }
                    }
                    return sb.ToString();
                }
                else if (extension.Equals(".pptx", StringComparison.OrdinalIgnoreCase))
                {
                    using var doc = PresentationDocument.Open(filePath, false);
                    var sb = new StringBuilder();
                    var presentationPart = doc.PresentationPart;
                    if (presentationPart != null)
                    {
                        foreach (var slidePart in presentationPart.SlideParts)
                        {
                            if (slidePart.Slide != null)
                            {
                                // Simple extraction of all text in the slide
                                sb.Append(slidePart.Slide.InnerText + " ");
                            }
                        }
                    }
                    return sb.ToString();
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        });
    }
}
