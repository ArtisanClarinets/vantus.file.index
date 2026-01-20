using Vantus.Engine.Data;
using Microsoft.EntityFrameworkCore;

namespace Vantus.Engine.Services.Indexing;

public class IndexingProgressEventArgs : EventArgs { public string Message { get; set; } = ""; public int Percent { get; set; } }
public class FileIndexedEventArgs : EventArgs { public string Path { get; set; } = ""; }

public class FileIndexerService
{
    private readonly VantusDbContext _db;
    private bool _isIndexing;

    public event EventHandler<IndexingProgressEventArgs>? ProgressChanged;
    public event EventHandler<FileIndexedEventArgs>? FileIndexed;

    public bool IsIndexing => _isIndexing;
    public int FilesIndexed { get; private set; }
    public int FilesRemaining { get; private set; }

    public FileIndexerService(VantusDbContext db)
    {
        _db = db;
        _db.Database.EnsureCreated();
    }

    public async Task StartIndexingAsync(IEnumerable<string> paths)
    {
        if (_isIndexing) return;
        _isIndexing = true;
        
        await Task.Run(async () => {
            try {
                foreach(var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        await IndexDirectoryAsync(path);
                    }
                }
            }
            finally {
                _isIndexing = false;
            }
        });
    }

    private async Task IndexDirectoryAsync(string path)
    {
        // Simple recursive scan
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        FilesRemaining = files.Length;
        FilesIndexed = 0;

        foreach(var file in files)
        {
            if (!_isIndexing) break;

            try {
                // Check if already indexed and modified
                var fileInfo = new FileInfo(file);
                var existing = await _db.Files.FirstOrDefaultAsync(f => f.Path == file);
                
                if (existing == null)
                {
                    // New file
                    var entity = new FileEntity {
                        Path = file,
                        Name = fileInfo.Name,
                        Extension = fileInfo.Extension,
                        Size = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                        LastIndexed = DateTime.UtcNow
                    };
                    _db.Files.Add(entity);
                    await _db.SaveChangesAsync();
                    FileIndexed?.Invoke(this, new FileIndexedEventArgs { Path = file });
                }
                else if (existing.LastModified != fileInfo.LastWriteTime)
                {
                    // Modified
                    existing.LastModified = fileInfo.LastWriteTime;
                    existing.LastIndexed = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                    FileIndexed?.Invoke(this, new FileIndexedEventArgs { Path = file });
                }

                FilesIndexed++;
                FilesRemaining--;
                ProgressChanged?.Invoke(this, new IndexingProgressEventArgs { Message = $"Indexed {fileInfo.Name}", Percent = (int)((double)FilesIndexed / files.Length * 100) });
            }
            catch (Exception ex) {
                // Log error
                Console.WriteLine($"Error indexing {file}: {ex.Message}");
            }
        }
    }

    public Task StopIndexingAsync()
    {
        _isIndexing = false;
        return Task.CompletedTask;
    }
    
    public Task ReindexPathAsync(string path) => StartIndexingAsync(new[] { path });
    
    public async Task RebuildIndexAsync()
    {
        await StopIndexingAsync();
        _db.Files.RemoveRange(_db.Files);
        await _db.SaveChangesAsync();
    }
}
