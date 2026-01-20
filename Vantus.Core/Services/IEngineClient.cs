namespace Vantus.Core.Services;

public interface IEngineClient
{
    Task<IndexStatus> GetIndexStatusAsync();
    Task PauseIndexingAsync();
    Task ResumeIndexingAsync();
    Task SetComputePreferenceAsync(ComputeDevice device);
    Task ReindexLocationAsync(string path);
    Task RebuildIndexAsync();
    Task ShutdownAsync();
    Task<TestExtractionResult> TestExtractionAsync(string filePath);
    Task<SearchResponse> SearchAsync(string query, int limit = 50, int offset = 0);
}

public class SearchResponse
{
    public List<SearchResultItem> Results { get; set; } = new();
    public int TotalCount { get; set; }
}

public class SearchResultItem
{
    public long Id { get; set; }
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string? Snippet { get; set; }
    public double Score { get; set; }
    public List<string> Tags { get; set; } = new();
    public string LastModified { get; set; } = "";
}

public enum ComputeDevice
{
    Auto,
    NPU,
    iGPU,
    CPU
}

public class IndexStatus
{
    public bool IsIndexing { get; set; }
    public int FilesIndexed { get; set; }
    public int FilesRemaining { get; set; }
    public double ProgressPercent { get; set; }
    public string? CurrentLocation { get; set; }
    public ComputeDevice ActiveDevice { get; set; }
    public DateTime? LastIndexTime { get; set; }
}

public class TestExtractionResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public string? ExtractedText { get; set; }
    public List<string> Metadata { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string? Error { get; set; }
    public long ProcessingTimeMs { get; set; }
}

public class EngineClientStub : IEngineClient
{
    public Task<IndexStatus> GetIndexStatusAsync()
    {
        return Task.FromResult(new IndexStatus
        {
            IsIndexing = false,
            FilesIndexed = 0,
            FilesRemaining = 0,
            ProgressPercent = 100,
            ActiveDevice = ComputeDevice.Auto
        });
    }

    public Task PauseIndexingAsync()
    {
        return Task.CompletedTask;
    }

    public Task ResumeIndexingAsync()
    {
        return Task.CompletedTask;
    }

    public Task SetComputePreferenceAsync(ComputeDevice device)
    {
        return Task.CompletedTask;
    }

    public Task ReindexLocationAsync(string path)
    {
        return Task.CompletedTask;
    }

    public Task RebuildIndexAsync()
    {
        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        return Task.CompletedTask;
    }

    public Task<TestExtractionResult> TestExtractionAsync(string filePath)
    {
        return Task.FromResult(new TestExtractionResult
        {
            Success = true,
            FilePath = filePath,
            FileType = "Test",
            ExtractedText = "Sample extracted text for testing purposes.",
            Metadata = new List<string> { "Author: Test", "Created: 2026-01-01" },
            Tags = new List<string> { "test", "sample" },
            ProcessingTimeMs = 50
        });
    }

    public Task<SearchResponse> SearchAsync(string query, int limit = 50, int offset = 0)
    {
        return Task.FromResult(new SearchResponse
        {
            TotalCount = 1,
            Results = new List<SearchResultItem>
            {
                new SearchResultItem
                {
                    Id = 1,
                    FilePath = @"C:\Mock\Result.txt",
                    FileName = "Result.txt",
                    Snippet = "Found this mock result for " + query,
                    Score = 1.0,
                    Tags = new List<string> { "mock" },
                    LastModified = DateTime.UtcNow.ToString("O")
                }
            }
        });
    }
}
