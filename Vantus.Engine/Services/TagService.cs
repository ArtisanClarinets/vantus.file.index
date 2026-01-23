using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vantus.Core.Models;

namespace Vantus.Engine.Services;

public class TagService
{
    private readonly DatabaseService _db;
    private readonly ILogger<TagService> _logger;

    public TagService(DatabaseService db, ILogger<TagService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        using var conn = _db.GetConnection();
        return await conn.QueryAsync<Tag>("SELECT id, name, type FROM tags");
    }

    public async Task AddTagAsync(string tagName, string type = "user")
    {
        using var conn = _db.GetConnection();
        await conn.ExecuteAsync("INSERT OR IGNORE INTO tags (name, type) VALUES (@Name, @Type)", new { Name = tagName, Type = type });
    }

    public async Task DeleteTagAsync(string tagName)
    {
        using var conn = _db.GetConnection();
        await conn.ExecuteAsync("PRAGMA foreign_keys = ON;");
        await conn.ExecuteAsync("DELETE FROM tags WHERE name = @Name", new { Name = tagName });
    }

    public async Task TagFileAsync(string filePath, string tagName, double confidence = 1.0)
    {
        await AddTagAsync(tagName);
        using var conn = _db.GetConnection();

        var fileId = await conn.ExecuteScalarAsync<long?>("SELECT id FROM files WHERE path = @Path", new { Path = filePath });
        var tagId = await conn.ExecuteScalarAsync<long?>("SELECT id FROM tags WHERE name = @Name", new { Name = tagName });

        if (fileId.HasValue && tagId.HasValue)
        {
            await conn.ExecuteAsync(
                "INSERT OR REPLACE INTO file_tags (file_id, tag_id, confidence) VALUES (@FileId, @TagId, @Confidence)",
                new { FileId = fileId.Value, TagId = tagId.Value, Confidence = confidence });
        }
    }
}
