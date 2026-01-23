using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Vantus.Core.Models;
using System.Text.Json;

namespace Vantus.Core.Engine;

public class NamedPipeEngineClient : IEngineClient
{
    private const string PipeName = "VantusEnginePipe";
    private readonly ILogger<NamedPipeEngineClient> _logger;

    public NamedPipeEngineClient(ILogger<NamedPipeEngineClient> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetIndexStatusAsync()
    {
        return await SendCommandAsync("STATUS");
    }

    public async Task<IEnumerable<SearchResult>> SearchAsync(string query)
    {
        var response = await SendCommandAsync($"SEARCH {query}");
        if (IsError(response)) return Enumerable.Empty<SearchResult>();

        try
        {
            return JsonSerializer.Deserialize<List<SearchResult>>(response) ?? Enumerable.Empty<SearchResult>();
        }
        catch { return Enumerable.Empty<SearchResult>(); }
    }

    public Task PauseIndexingAsync() => SendCommandAsync("PAUSE");
    public Task ResumeIndexingAsync() => SendCommandAsync("RESUME");
    public Task RequestRebuildIndexAsync() => SendCommandAsync("REBUILD");

    // NEW METHODS

    public async Task<IndexStats> GetStatsAsync()
    {
        var response = await SendCommandAsync("GET_STATS");
        if (IsError(response)) return new IndexStats { Status = "Disconnected" };
        try
        {
            return JsonSerializer.Deserialize<IndexStats>(response) ?? new IndexStats();
        }
        catch { return new IndexStats { Status = "Error" }; }
    }

    public async Task<IEnumerable<Tag>> GetTagsAsync()
    {
        var response = await SendCommandAsync("GET_TAGS");
        if (IsError(response)) return Enumerable.Empty<Tag>();
        try
        {
            return JsonSerializer.Deserialize<List<Tag>>(response) ?? Enumerable.Empty<Tag>();
        }
        catch { return Enumerable.Empty<Tag>(); }
    }

    public Task AddTagAsync(Tag tag) => SendCommandAsync($"ADD_TAG {JsonSerializer.Serialize(tag)}");
    public Task DeleteTagAsync(string name) => SendCommandAsync($"DELETE_TAG {name}");

    public async Task<IEnumerable<Partner>> GetPartnersAsync()
    {
        var response = await SendCommandAsync("GET_PARTNERS");
        if (IsError(response)) return Enumerable.Empty<Partner>();
        try
        {
             return JsonSerializer.Deserialize<List<Partner>>(response) ?? Enumerable.Empty<Partner>();
        }
        catch { return Enumerable.Empty<Partner>(); }
    }

    public Task AddPartnerAsync(Partner partner) => SendCommandAsync($"ADD_PARTNER {JsonSerializer.Serialize(partner)}");

    public async Task<IEnumerable<Rule>> GetRulesAsync()
    {
        var response = await SendCommandAsync("GET_RULES");
        if (IsError(response)) return Enumerable.Empty<Rule>();
        try
        {
             return JsonSerializer.Deserialize<List<Rule>>(response) ?? Enumerable.Empty<Rule>();
        }
        catch { return Enumerable.Empty<Rule>(); }
    }

    public Task AddRuleAsync(Rule rule) => SendCommandAsync($"ADD_RULE {JsonSerializer.Serialize(rule)}");
    public Task DeleteRuleAsync(long id) => SendCommandAsync($"DELETE_RULE {id}");

    private bool IsError(string response)
    {
        return string.IsNullOrEmpty(response) || response == "Unknown" || response == "Disconnected";
    }

    private async Task<string> SendCommandAsync(string command)
    {
        const int MaxRetries = 3;
        const int BaseDelayMs = 200;

        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
                await client.ConnectAsync(500);

                using var writer = new StreamWriter(client) { AutoFlush = true };
                using var reader = new StreamReader(client);

                await writer.WriteLineAsync(command);
                return await reader.ReadLineAsync() ?? "Unknown";
            }
            catch (Exception ex)
            {
                if (i == MaxRetries - 1)
                {
                    _logger.LogWarning(ex, "Failed to connect to engine IPC after {Retries} attempts", MaxRetries);
                    return "Disconnected";
                }
                await Task.Delay(BaseDelayMs * (i + 1));
            }
        }
        return "Disconnected";
    }
}
