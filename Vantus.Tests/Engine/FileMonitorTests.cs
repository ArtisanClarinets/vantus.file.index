using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Vantus.Engine.Data;
using Vantus.Engine.Services;
using Vantus.Engine.Services.Indexing;
using Vantus.Engine.Services.Search;
using Vantus.Engine.Services.AI;
using Xunit;

using Vantus.Engine.Services.Extraction;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Path = System.IO.Path;

namespace Vantus.Tests.Engine;

public class FileMonitorTests
{
    [Fact]
    public async Task Monitor_Detects_Creation_And_Indexer_Saves_To_Db()
    {
        // Setup
        var channel = new IndexingChannel();
        var monitor = new FileMonitorService(channel, NullLogger<FileMonitorService>.Instance);

        // DbContext with File Sqlite (better concurrency for integration test)
        var tempDb = Path.GetTempFileName();
        var connectionString = $"Data Source={tempDb}";
        
        var options = new DbContextOptionsBuilder<VantusDbContext>()
            .UseSqlite(connectionString)
            .Options;
        
        // Ensure DB created
        using (var ctx = new VantusDbContext(options))
        {
            ctx.Database.EnsureCreated();
        }

        // Setup DI Container for ScopeFactory
        var services = new ServiceCollection();
        
        // Register DbContext
        services.AddDbContext<VantusDbContext>(opts => opts.UseSqlite(connectionString));
        
        // Register Mock EmbeddingService
        var embeddingMock = new Mock<IEmbeddingService>();
        embeddingMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[384]);
        services.AddSingleton(embeddingMock.Object);

        // Register VectorSearchService
        services.AddScoped<VectorSearchService>();
        
        // Register Logging
        services.AddLogging(builder => builder.AddProvider(new TestLoggerProvider()));

        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var extractors = new List<IContentExtractor>();
        var compositeExtractor = new CompositeContentExtractor(extractors, NullLogger<CompositeContentExtractor>.Instance);
        
        var indexer = new FileIndexerService(channel, scopeFactory, serviceProvider.GetRequiredService<ILogger<FileIndexerService>>(), compositeExtractor);

        // Start Indexer
        var cts = new CancellationTokenSource();
        var indexerTask = indexer.StartAsync(cts.Token);

        // Start Monitor
        var tempDir = Path.Combine(Path.GetTempPath(), "VantusTest_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            monitor.StartMonitoring(tempDir);

            // Action
            var filePath = Path.Combine(tempDir, "test.txt");
            await File.WriteAllTextAsync(filePath, "content");

            // Wait for processing (polling)
            int retries = 20; // Increased retries
            bool found = false;
            while (retries-- > 0)
            {
                await Task.Delay(200);
                using var verifyContext = new VantusDbContext(options);
                if (await verifyContext.Files.AnyAsync(f => f.FilePath == filePath))
                {
                    found = true;
                    break;
                }
            }

            Assert.True(found, "File was not indexed within timeout");
        }
        finally
        {
            cts.Cancel();
            await Task.WhenAny(indexerTask, Task.Delay(1000)); // Wait for indexer to stop
            monitor.Dispose();
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
            // Try to delete db file, might be locked if context not disposed
            try { File.Delete(tempDb); } catch { }
        }
    }
}
