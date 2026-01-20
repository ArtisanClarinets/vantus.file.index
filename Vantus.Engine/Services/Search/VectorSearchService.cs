using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Vantus.Engine.Data;
using Vantus.Engine.Data.Entities;
using Vantus.Engine.Services.AI;
using Vantus.Engine.Protos;

namespace Vantus.Engine.Services.Search;

public class VectorSearchService
{
    private readonly VantusDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<VectorSearchService> _logger;

    public VectorSearchService(
        VantusDbContext dbContext,
        IEmbeddingService embeddingService,
        ILogger<VectorSearchService> logger)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task IndexContentAsync(long fileId, string content, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            // Generate embedding
            var vector = await _embeddingService.GenerateEmbeddingAsync(content, ct);
            if (vector == null || vector.Length == 0) return;

            // Check if exists
            var existing = await _dbContext.FileEmbeddings
                .FirstOrDefaultAsync(e => e.FileId == fileId, ct);

            if (existing != null)
            {
                existing.Vector = vector;
                existing.CreatedAt = DateTime.UtcNow;
                _dbContext.FileEmbeddings.Update(existing);
            }
            else
            {
                var embedding = new FileEmbedding
                {
                    FileId = fileId,
                    Vector = vector,
                    ModelName = "all-MiniLM-L6-v2",
                    CreatedAt = DateTime.UtcNow
                };
                await _dbContext.FileEmbeddings.AddAsync(embedding, ct);
            }

            await _dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing vectors for file {FileId}", fileId);
        }
    }

    public async Task<List<SearchResultItem>> SearchAsync(string query, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            var queryVector = await _embeddingService.GenerateEmbeddingAsync(query, ct);
            if (queryVector == null || queryVector.Length == 0) return new List<SearchResultItem>();

            // Load all embeddings (naive in-memory approach for MVP)
            // For production with millions of files, use a Vector DB or SQLite-vec extension
            var allEmbeddings = await _dbContext.FileEmbeddings
                .Include(e => e.File)
                .AsNoTracking()
                .ToListAsync(ct);

            var results = allEmbeddings
                .Select(e => new
                {
                    Entity = e,
                    Score = CosineSimilarity(queryVector, e.Vector)
                })
                .OrderByDescending(x => x.Score)
                .Take(limit)
                .Select(x => new SearchResultItem
                {
                    Id = x.Entity.File.Id,
                    FilePath = x.Entity.File.FilePath,
                    FileName = x.Entity.File.FileName,
                    Score = (float)x.Score,
                    Snippet = x.Entity.File.Content?.Substring(0, Math.Min(200, x.Entity.File.Content?.Length ?? 0)) ?? ""
                })
                .ToList();

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing vector search");
            return new List<SearchResultItem>();
        }
    }

    private static float CosineSimilarity(float[] v1, float[] v2)
    {
        if (v1.Length != v2.Length) return 0f;

        float dot = 0f;
        float mag1 = 0f;
        float mag2 = 0f;

        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        if (mag1 == 0 || mag2 == 0) return 0f;

        return dot / (float)(Math.Sqrt(mag1) * Math.Sqrt(mag2));
    }
}
