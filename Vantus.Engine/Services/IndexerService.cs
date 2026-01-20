using Dapper;
using Microsoft.Extensions.Logging;
using Vantus.Engine.Parsers;

namespace Vantus.Engine.Services;

public class IndexerService
{
    private readonly DatabaseService _db;
    private readonly RulesEngineService _rules;
    private readonly ILogger<IndexerService> _logger;
    private readonly List<IFileParser> _parsers;

    public IndexerService(DatabaseService db, RulesEngineService rules, ILogger<IndexerService> logger)
    {
        _db = db;
        _rules = rules;
        _logger = logger;
        _parsers = new List<IFileParser>
        {
            new TextParser(),
            new PdfParser(),
            new ImageParser()
        };
    }

    public async Task IndexFileAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists) return;

        string content = "";
        var parser = _parsers.FirstOrDefault(p => p.CanParse(fileInfo.Extension));
        if (parser != null)
        {
            try
            {
                content = await parser.ParseAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse content for {Path}", filePath);
            }
        }

        using var conn = _db.GetConnection();
        var sql = @"
            INSERT INTO files (path, name, extension, size, last_modified, content)
            VALUES (@Path, @Name, @Extension, @Size, @LastModified, @Content)
            ON CONFLICT(path) DO UPDATE SET
                size = excluded.size,
                last_modified = excluded.last_modified,
                content = excluded.content;
        ";

        await conn.ExecuteAsync(sql, new
        {
            Path = filePath,
            Name = fileInfo.Name,
            Extension = fileInfo.Extension,
            Size = fileInfo.Length,
            LastModified = new DateTimeOffset(fileInfo.LastWriteTimeUtc).ToUnixTimeSeconds(),
            Content = content
        });

        // Trigger rules
        await _rules.ApplyRulesAsync(filePath);

        _logger.LogDebug("Indexed {Path}", filePath);
    }
}
