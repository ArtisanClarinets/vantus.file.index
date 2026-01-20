using System.Net.Http;
using Grpc.Net.Client;
using Vantus.Core.Services;
using Vantus.Engine.Protos;

namespace Vantus.App.Services;

public class GrpcEngineClient : IEngineClient
{
    private readonly Engine.Protos.Engine.EngineClient _client;

    public GrpcEngineClient()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:5000", new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            }
        });
        _client = new Engine.Protos.Engine.EngineClient(channel);
    }

    public async Task<Core.Services.IndexStatus> GetIndexStatusAsync()
    {
        try
        {
            var response = await _client.GetIndexStatusAsync(new Empty());
            return new Core.Services.IndexStatus
            {
                IsIndexing = response.IsIndexing,
                FilesIndexed = response.FilesIndexed,
                FilesRemaining = response.FilesRemaining,
                ProgressPercent = response.ProgressPercent,
                CurrentLocation = response.CurrentLocation,
                ActiveDevice = (Core.Services.ComputeDevice)response.ActiveDevice,
                LastIndexTime = DateTime.TryParse(response.LastIndexTime, out var dt) ? dt : null
            };
        }
        catch
        {
            // Fallback if engine is not running
            return new Core.Services.IndexStatus();
        }
    }

    public async Task PauseIndexingAsync()
    {
        await _client.PauseIndexingAsync(new Empty());
    }

    public async Task ResumeIndexingAsync()
    {
        await _client.ResumeIndexingAsync(new Empty());
    }

    public async Task SetComputePreferenceAsync(Core.Services.ComputeDevice device)
    {
        await _client.SetComputePreferenceAsync(new ComputePreferenceRequest
        {
            Device = (Engine.Protos.ComputeDevice)device
        });
    }

    public async Task ReindexLocationAsync(string path)
    {
        await _client.ReindexLocationAsync(new ReindexLocationRequest { Path = path });
    }

    public async Task RebuildIndexAsync()
    {
        await _client.RebuildIndexAsync(new Empty());
    }

    public async Task<Core.Services.TestExtractionResult> TestExtractionAsync(string filePath)
    {
        var response = await _client.TestExtractionAsync(new TestExtractionRequest { FilePath = filePath });
        return new Core.Services.TestExtractionResult
        {
            Success = response.Success,
            FilePath = response.FilePath,
            FileType = response.FileType,
            ExtractedText = response.ExtractedText,
            Metadata = response.Metadata.ToList(),
            Tags = response.Tags.ToList(),
            Error = response.Error,
            ProcessingTimeMs = response.ProcessingTimeMs
        };
    }

    public async Task<Core.Services.SearchResponse> SearchAsync(string query, int limit = 50, int offset = 0)
    {
        var request = new Vantus.Engine.Protos.SearchRequest
        {
            Query = query,
            Limit = limit,
            Offset = offset
        };

        var response = await _client.SearchAsync(request);

        return new Core.Services.SearchResponse
        {
            TotalCount = response.TotalCount,
            Results = response.Results.Select(r => new Core.Services.SearchResultItem
            {
                Id = r.Id,
                FilePath = r.FilePath,
                FileName = r.FileName,
                Snippet = r.Snippet,
                Score = r.Score,
                Tags = r.Tags.ToList(),
                LastModified = r.LastModified
            }).ToList()
        };
    }
}
