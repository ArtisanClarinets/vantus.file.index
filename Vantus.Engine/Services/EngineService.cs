using Grpc.Core;
using Vantus.Core.Services;
using Vantus.Engine.Protos;
using Vantus.Engine.Data;
using Microsoft.EntityFrameworkCore;
using Vantus.Engine.Services.Search;

namespace Vantus.Engine.Services;

public class EngineService : Engine.Protos.Engine.EngineBase
{
    private readonly ILogger<EngineService> _logger;
    private readonly FileMonitorService _fileMonitor;
    private readonly VantusDbContext _db;
    private readonly VectorSearchService _vectorSearch;

    public EngineService(
        ILogger<EngineService> logger, 
        FileMonitorService fileMonitor, 
        VantusDbContext db,
        VectorSearchService vectorSearch)
    {
        _logger = logger;
        _fileMonitor = fileMonitor;
        _db = db;
        _vectorSearch = vectorSearch;
    }

    public override async Task<Vantus.Engine.Protos.SearchResponse> Search(SearchRequest request, ServerCallContext context)
    {
        var response = new Vantus.Engine.Protos.SearchResponse();
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                // Return generic latest files if query is empty
                var latest = await _db.Files
                    .OrderByDescending(f => f.ModifiedAt)
                    .Take(request.Limit > 0 ? request.Limit : 20)
                    .Select(f => new Vantus.Engine.Protos.SearchResultItem
                    {
                        Id = f.Id,
                        FilePath = f.FilePath,
                        FileName = f.FileName,
                        Snippet = "",
                        Score = 1.0,
                        LastModified = f.ModifiedAt.ToString("O")
                    })
                    .ToListAsync();
                response.Results.AddRange(latest);
                response.TotalCount = latest.Count;
            }
            else
            {
                // Vector Search
                var results = await _vectorSearch.SearchAsync(request.Query, request.Limit > 0 ? request.Limit : 20, context.CancellationToken);
                response.Results.AddRange(results);
                response.TotalCount = results.Count;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed for query: {Query}", request.Query);
        }
        return response;
    }
    public override Task<IndexStatusResponse> GetIndexStatus(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new IndexStatusResponse
        {
            IsIndexing = false,
            FilesIndexed = 0,
            FilesRemaining = 0,
            ProgressPercent = 0,
            ActiveDevice = Vantus.Engine.Protos.ComputeDevice.Auto,
            CurrentLocation = "",
            LastIndexTime = DateTime.UtcNow.ToString("O")
        });
    }

    public override Task<Empty> PauseIndexing(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("Pause Indexing requested");
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> ResumeIndexing(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("Resume Indexing requested");
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> SetComputePreference(ComputePreferenceRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Set Compute Preference: {Device}", request.Device);
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> ReindexLocation(ReindexLocationRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Reindex Location: {Path}", request.Path);
        _fileMonitor.StartMonitoring(request.Path);
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> RebuildIndex(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("Rebuild Index requested");
        return Task.FromResult(new Empty());
    }

    public override Task<Vantus.Engine.Protos.TestExtractionResult> TestExtraction(TestExtractionRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Test Extraction: {FilePath}", request.FilePath);
        
        // Mock result
        return Task.FromResult(new Vantus.Engine.Protos.TestExtractionResult
        {
            Success = true,
            FilePath = request.FilePath,
            FileType = "Mock",
            ExtractedText = $"Mock extraction content for {Path.GetFileName(request.FilePath)}",
            ProcessingTimeMs = 100
        });
    }
}
