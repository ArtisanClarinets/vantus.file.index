
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

        // Watch for settings changes
        WatchSettings(stoppingToken);

        await Task.Delay(-1, stoppingToken);
    }

    private void WatchSettings(CancellationToken stoppingToken)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(localAppData, "Vantus");
        Directory.CreateDirectory(folder);

        var watcher = new FileSystemWatcher(folder, "settings.json");
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += async (s, e) =>
        {
            // Debounce settings reload
            await Task.Delay(1000, stoppingToken);
            _logger.LogInformation("Settings changed, reloading...");
            await _crawler.UpdateLocationsAsync(stoppingToken);
        };
        watcher.EnableRaisingEvents = true;
    }
}
