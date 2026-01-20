using Microsoft.EntityFrameworkCore;
using Vantus.Engine.Data;
using Vantus.Engine.Data.Entities;
using Vantus.Engine.Services.Extraction;
using Vantus.Engine.Services.Search;

namespace Vantus.Engine.Services.Indexing;

public class FileIndexerService : BackgroundService
{
    private readonly IndexingChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FileIndexerService> _logger;
    private readonly CompositeContentExtractor _extractor;

    public FileIndexerService(
        IndexingChannel channel,
        IServiceScopeFactory scopeFactory,
        ILogger<FileIndexerService> logger,
        CompositeContentExtractor extractor)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _extractor = extractor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FileIndexerService starting.");
        Console.WriteLine("DEBUG: FileIndexerService starting.");

        await foreach (var evt in _channel.ReadAllAsync(stoppingToken))
        {
            Console.WriteLine($"DEBUG: Processing event for {evt.FilePath}");
            try
            {
                await ProcessEventAsync(evt, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file event for {Path}", evt.FilePath);
            }
        }
    }

    private async Task ProcessEventAsync(FileChangeEvent evt, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VantusDbContext>();
        var vectorService = scope.ServiceProvider.GetRequiredService<VectorSearchService>();

        switch (evt.ChangeType)
        {
            case ChangeType.Created:
            case ChangeType.Modified:
                await IndexFileAsync(db, vectorService, evt.FilePath, ct);
                break;
            case ChangeType.Deleted:
                await RemoveFileAsync(db, evt.FilePath, ct);
                break;
            case ChangeType.Renamed:
                if (evt.OldFilePath != null)
                {
                    await RenameFileAsync(db, vectorService, evt.OldFilePath, evt.FilePath, ct);
                }
                break;
        }
    }

    private async Task IndexFileAsync(VantusDbContext db, VectorSearchService vectorService, string path, CancellationToken ct)
    {
        if (!File.Exists(path)) return;

        try
        {
            var fileInfo = new FileInfo(path);
            var entity = await db.Files.FirstOrDefaultAsync(f => f.FilePath == path, ct);

            if (entity == null)
            {
                entity = new FileIndexItem
                {
                    FilePath = path,
                    FileName = fileInfo.Name,
                    Extension = fileInfo.Extension,
                    CreatedAt = fileInfo.CreationTimeUtc,
                };
                db.Files.Add(entity);
            }

            // Update properties
            entity.SizeBytes = fileInfo.Length;
            entity.ModifiedAt = fileInfo.LastWriteTimeUtc;
            entity.LastScannedAt = DateTime.UtcNow;
            
            // Extract Content
            try 
            {
                var content = await _extractor.ExtractAsync(path, ct);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    entity.Content = content;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Content extraction failed for {Path}", path);
            }
            
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Indexed: {Path}", path);

            // Vector Indexing
            if (!string.IsNullOrWhiteSpace(entity.Content))
            {
                await vectorService.IndexContentAsync(entity.Id, entity.Content, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to index file: {Path}", path);
        }
    }

    private async Task RemoveFileAsync(VantusDbContext db, string path, CancellationToken ct)
    {
        var entity = await db.Files.FirstOrDefaultAsync(f => f.FilePath == path, ct);
        if (entity != null)
        {
            db.Files.Remove(entity);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Removed from index: {Path}", path);
        }
    }

    private async Task RenameFileAsync(VantusDbContext db, VectorSearchService vectorService, string oldPath, string newPath, CancellationToken ct)
    {
        var entity = await db.Files.FirstOrDefaultAsync(f => f.FilePath == oldPath, ct);
        if (entity != null)
        {
            entity.FilePath = newPath;
            entity.FileName = Path.GetFileName(newPath);
            entity.Extension = Path.GetExtension(newPath);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Renamed in index: {Old} -> {New}", oldPath, newPath);
        }
        else
        {
            // Treat as new file if old not found
            await IndexFileAsync(db, vectorService, newPath, ct);
        }
    }
}
