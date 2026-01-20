using Vantus.Engine.Services.Indexing;

namespace Vantus.Engine.Services;

public class FileMonitorService : IDisposable
{
    private readonly IndexingChannel _channel;
    private readonly ILogger<FileMonitorService> _logger;
    private readonly List<FileSystemWatcher> _watchers = new();

    public FileMonitorService(IndexingChannel channel, ILogger<FileMonitorService> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    public void StartMonitoring(string path)
    {
        if (!Directory.Exists(path))
        {
            _logger.LogWarning("Cannot monitor non-existent path: {Path}", path);
            return;
        }

        if (_watchers.Any(w => w.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogInformation("Already monitoring path: {Path}", path);
            return;
        }

        try
        {
            var watcher = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | 
                               NotifyFilters.LastWrite | NotifyFilters.Size | 
                               NotifyFilters.CreationTime
            };

            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            watcher.EnableRaisingEvents = true;
            _watchers.Add(watcher);

            _logger.LogInformation("Started monitoring: {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start monitoring for path: {Path}", path);
        }
    }

    public void StopMonitoring(string path)
    {
        var watcher = _watchers.FirstOrDefault(w => w.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
        if (watcher != null)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            _watchers.Remove(watcher);
            _logger.LogInformation("Stopped monitoring: {Path}", path);
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        _channel.Write(new FileChangeEvent { FilePath = e.FullPath, ChangeType = ChangeType.Created });
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        // FSW can fire multiple events for a single change.
        // The IndexerService should handle debouncing or simple re-indexing (idempotent).
        _channel.Write(new FileChangeEvent { FilePath = e.FullPath, ChangeType = ChangeType.Modified });
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        _channel.Write(new FileChangeEvent { FilePath = e.FullPath, ChangeType = ChangeType.Deleted });
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _channel.Write(new FileChangeEvent 
        { 
            FilePath = e.FullPath, 
            OldFilePath = e.OldFullPath, 
            ChangeType = ChangeType.Renamed 
        });
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        var ex = e.GetException();
        _logger.LogError(ex, "FileSystemWatcher error");
        // TODO: Implement recovery logic (e.g., restart watcher, full scan)
    }

    public void Dispose()
    {
        foreach (var watcher in _watchers)
        {
            watcher.Dispose();
        }
        _watchers.Clear();
    }
}
