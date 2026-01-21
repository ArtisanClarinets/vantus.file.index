using Dapper;
using Microsoft.Extensions.Logging;
using Vantus.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Vantus.Engine.Services;

public class SearchService
{
    private readonly DatabaseService _db;
    private readonly ILogger<SearchService> _logger;

    public SearchService(DatabaseService db, ILogger<SearchService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<SearchResult>> SearchAsync(string query)
    {
        using var conn = _db.GetConnection();

        // FTS5 query with snippet generation
        var sql = @"
            SELECT f.path, f.name, snippet(files_fts, 5, '<b>', '</b>', '...', 20) as snippet, fts.rank
            FROM files f
            JOIN files_fts fts ON f.id = fts.rowid
            WHERE files_fts MATCH @Query
            ORDER BY rank
            LIMIT 50;
        ";

        try
        {
            var results = await conn.QueryAsync<dynamic>(sql, new { Query = query });
            return results.Select(r => new SearchResult
            {
                Path = r.path,
                Title = r.name,
                Snippet = r.snippet ?? string.Empty,
                Score = (double)r.rank
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed for query {Query}", query);
            return Enumerable.Empty<SearchResult>();
        }
    }
}
