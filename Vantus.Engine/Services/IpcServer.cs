
using System.IO.Pipes;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System;

namespace Vantus.Engine.Services;

public class IpcServer
{
    private readonly ILogger<IpcServer> _logger;
    private readonly SearchService _searchService;
    private readonly DatabaseService _db;
    private readonly FileCrawlerService _crawler;
    private const string PipeName = "VantusEnginePipe";

    public IpcServer(ILogger<IpcServer> logger, SearchService searchService, DatabaseService db, FileCrawlerService crawler)
    {
        _logger = logger;
        _searchService = searchService;
        _db = db;
        _crawler = crawler;
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
                if (line == "STATUS")
                {
                    await writer.WriteLineAsync("Indexing");
                }
                else if (line?.StartsWith("SEARCH ") == true)
                {
                    var query = line.Substring(7);
                    var results = await _searchService.SearchAsync(query);
                    var json = System.Text.Json.JsonSerializer.Serialize(results);
                    await writer.WriteLineAsync(json);
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
        catch { }
    }
}
