
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vantus.Engine.Services;

namespace Vantus.Engine;

public class EngineWorker : BackgroundService
{
    private readonly DatabaseService _db;
    private readonly FileCrawlerService _crawler;
    private readonly IpcServer _ipc;
    private readonly ILogger<EngineWorker> _logger;

    public EngineWorker(DatabaseService db, FileCrawlerService crawler, IpcServer ipc, ILogger<EngineWorker> logger)
    {
        _db = db;
        _crawler = crawler;
        _ipc = ipc;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Vantus Engine starting...");
        await _db.InitializeAsync();

        // Start IPC
        _ = Task.Run(() => _ipc.StartAsync(stoppingToken), stoppingToken);

        // Initial crawl
        _ = Task.Run(() => _crawler.StartCrawlingAsync(stoppingToken), stoppingToken);

        await Task.Delay(-1, stoppingToken);
    }
}
