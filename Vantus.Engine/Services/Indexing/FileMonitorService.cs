namespace Vantus.Engine.Services.Indexing;

public class FileMonitorService
{
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly FileIndexerService _indexer;

    public FileMonitorService(FileIndexerService indexer)
    {
        _indexer = indexer;
    }

    public Task StartMonitoring(string path)
    {
        if (IsPathWatched(path)) return Task.CompletedTask;

        if (Directory.Exists(path))
        {
            var watcher = new FileSystemWatcher(path);
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;
            watcher.EnableRaisingEvents = true;
            _watchers.Add(watcher);
        }
        return Task.CompletedTask;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce logic should go here
        Console.WriteLine($"File {e.ChangeType}: {e.FullPath}");
        if (File.Exists(e.FullPath))
        {
             // Trigger reindex
             // _indexer.ReindexPathAsync(e.FullPath);
        }
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        Console.WriteLine($"Renamed: {e.OldFullPath} to {e.FullPath}");
    }

    public Task StopMonitoring(string path)
    {
        var watcher = _watchers.FirstOrDefault(w => w.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
        if (watcher != null)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            _watchers.Remove(watcher);
        }
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetWatchedPaths() => _watchers.Select(w => w.Path);
    public bool IsPathWatched(string path) => _watchers.Any(w => w.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
}
