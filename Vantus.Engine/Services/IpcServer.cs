using System.IO.Pipes;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System;
using System.Text.Json;
using Vantus.Core.Models;
using Dapper;

namespace Vantus.Engine.Services;

public class IpcServer
{
    private readonly ILogger<IpcServer> _logger;
    private readonly SearchService _searchService;
    private readonly DatabaseService _db;
    private readonly FileCrawlerService _crawler;
    private readonly TagService _tagService;
    private readonly PartnerService _partnerService;
    private readonly RulesEngineService _rulesService;
    private const string PipeName = "VantusEnginePipe";

    public IpcServer(ILogger<IpcServer> logger, SearchService searchService, DatabaseService db, FileCrawlerService crawler, TagService tagService, PartnerService partnerService, RulesEngineService rulesService)
    {
        _logger = logger;
        _searchService = searchService;
        _db = db;
        _crawler = crawler;
        _tagService = tagService;
        _partnerService = partnerService;
        _rulesService = rulesService;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        // Monitor for orphaned process state (if parent dies without killing us)
        _ = Task.Run(async () =>
        {
            while(!ct.IsCancellationRequested)
            {
                await Task.Delay(5000, ct);
            }
        }, ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync(ct);
                _ = HandleConnectionAsync(server, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "IPC Server Error");
            }
        }
    }

    private async Task HandleConnectionAsync(NamedPipeServerStream server, CancellationToken ct)
    {
        try
        {
            using (server)
            using (var reader = new StreamReader(server))
            using (var writer = new StreamWriter(server) { AutoFlush = true })
            {
                var line = await reader.ReadLineAsync(ct);
                if (string.IsNullOrEmpty(line)) return;

                if (line == "STATUS")
                {
                    await writer.WriteLineAsync("Indexing");
                }
                else if (line == "GET_STATS")
                {
                     using var conn = _db.GetConnection();
                     var files = await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM files");
                     var tags = await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM tags");
                     var partners = await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM partners");
                     var stats = new IndexStats { FilesIndexed = files, TotalTags = tags, TotalPartners = partners, Status = "Active" };
                     await writer.WriteLineAsync(JsonSerializer.Serialize(stats));
                }
                else if (line.StartsWith("SEARCH "))
                {
                    var query = line.Substring(7);
                    var results = await _searchService.SearchAsync(query);
                    await writer.WriteLineAsync(JsonSerializer.Serialize(results));
                }
                else if (line == "GET_TAGS")
                {
                    var results = await _tagService.GetAllTagsAsync();
                    await writer.WriteLineAsync(JsonSerializer.Serialize(results));
                }
                else if (line.StartsWith("ADD_TAG "))
                {
                    var json = line.Substring(8);
                    var tag = JsonSerializer.Deserialize<Tag>(json);
                    if (tag != null) await _tagService.AddTagAsync(tag.Name, tag.Type);
                    await writer.WriteLineAsync("OK");
                }
                else if (line.StartsWith("DELETE_TAG "))
                {
                    var name = line.Substring(11);
                    await _tagService.DeleteTagAsync(name);
                    await writer.WriteLineAsync("OK");
                }
                else if (line == "GET_PARTNERS")
                {
                    var results = await _partnerService.GetAllPartnersAsync();
                    await writer.WriteLineAsync(JsonSerializer.Serialize(results));
                }
                else if (line.StartsWith("ADD_PARTNER "))
                {
                    var json = line.Substring(12);
                    var p = JsonSerializer.Deserialize<Partner>(json);
                    if (p != null) await _partnerService.AddPartnerAsync(p);
                    await writer.WriteLineAsync("OK");
                }
                 else if (line == "GET_RULES")
                {
                    var results = await _rulesService.GetAllRulesAsync();
                    await writer.WriteLineAsync(JsonSerializer.Serialize(results));
                }
                else if (line.StartsWith("ADD_RULE "))
                {
                    var json = line.Substring(9);
                    var r = JsonSerializer.Deserialize<Rule>(json);
                    if (r != null) await _rulesService.AddRuleAsync(r);
                    await writer.WriteLineAsync("OK");
                }
                else if (line.StartsWith("DELETE_RULE "))
                {
                    if (long.TryParse(line.Substring(12), out long id))
                    {
                        await _rulesService.DeleteRuleAsync(id);
                    }
                    await writer.WriteLineAsync("OK");
                }
                else if (line == "REBUILD")
                {
                    await _db.RebuildAsync();
                    await _crawler.UpdateLocationsAsync(ct);
                    await writer.WriteLineAsync("OK");
                }
                else
                {
                    await writer.WriteLineAsync("OK");
                }
            }
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "IPC Handler Error");
        }
    }
}
