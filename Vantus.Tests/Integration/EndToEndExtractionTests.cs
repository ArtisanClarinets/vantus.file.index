using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Vantus.Engine.Data;
using Vantus.Engine.Data.Entities;
using Vantus.Engine.Services;
using Vantus.Engine.Services.Indexing;
using Vantus.Engine.Services.Extraction;
using Vantus.Engine.Services.Search;
using Vantus.Engine.Services.AI;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using Moq;
using Xunit;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Path = System.IO.Path;

namespace Vantus.Tests.Integration;

public class EndToEndExtractionTests : IAsyncLifetime
{
    private readonly string _testDir;
    private readonly string _dbPath;
    private IHost _host;
    // private VantusDbContext? _db; // Removed unused field

    public EndToEndExtractionTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "VantusTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);

        _dbPath = Path.Combine(_testDir, "test.db");

        var builder = Host.CreateApplicationBuilder();

        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

        // Setup DB
        builder.Services.AddDbContext<VantusDbContext>(options =>
            options.UseSqlite($"Data Source={_dbPath}"));

        // Register Services
        builder.Services.AddSingleton<IndexingChannel>();
        
        builder.Services.AddSingleton<FileMonitorService>();
        builder.Services.AddHostedService<FileIndexerService>();
        
        builder.Services.AddSingleton<IContentExtractor, PdfExtractor>();
        builder.Services.AddSingleton<IContentExtractor, OfficeExtractor>();
        builder.Services.AddSingleton<CompositeContentExtractor>();

        // Mock Embedding Service to avoid model dependency
        var mockEmbedding = new Mock<IEmbeddingService>();
        mockEmbedding.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[384]); // Mock 384-dim vector
        builder.Services.AddSingleton(mockEmbedding.Object);
        
        // Use real VectorSearchService (with mocked embedding)
        builder.Services.AddScoped<VectorSearchService>();

        _host = builder.Build();
    }

    public async Task InitializeAsync()
    {
        // Initialize DB
        using var scope = _host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VantusDbContext>();
        await db.Database.EnsureCreatedAsync();

        // Start Host
        await _host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
        try { Directory.Delete(_testDir, true); } catch { }
    }

    [Fact]
    public async Task Dropping_Pdf_Should_Extract_Content_And_Index()
    {
        // 1. Create PDF file
        var pdfPath = Path.Combine(_testDir, "test.pdf");
        var pdfContent = "This is a test PDF document for Vantus Indexer.";
        
        CreatePdf(pdfPath, pdfContent);

        // 2. Trigger Indexing
        var channel = _host.Services.GetRequiredService<IndexingChannel>();
        channel.Write(new FileChangeEvent
        {
            ChangeType = ChangeType.Created,
            FilePath = pdfPath
        });

        // 3. Wait for DB update
        // Poll DB for up to 10 seconds
        FileIndexItem? indexedFile = null;
        for (int i = 0; i < 100; i++)
        {
            // Create new scope to avoid stale cache
            using var scope = _host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<VantusDbContext>();
            indexedFile = await db.Files.Include(f => f.FileTags).FirstOrDefaultAsync(f => f.FilePath == pdfPath);
            
            if (indexedFile != null && !string.IsNullOrEmpty(indexedFile.Content))
            {
                break;
            }
            await Task.Delay(100);
        }

        // 4. Assertions
        Assert.NotNull(indexedFile);
        Assert.Equal("test.pdf", indexedFile.FileName);
        Assert.Contains("Vantus Indexer", indexedFile.Content);
        
        // Check Vector (Poll for it as it happens after file indexing)
        FileEmbedding? embedding = null;
        for (int i = 0; i < 100; i++)
        {
            using var scope2 = _host.Services.CreateScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<VantusDbContext>();
            embedding = await db2.FileEmbeddings.FirstOrDefaultAsync(e => e.FileId == indexedFile.Id);
            if (embedding != null) break;
            await Task.Delay(100);
        }

        Assert.NotNull(embedding);
        Assert.Equal(384, embedding.Vector.Length);
    }

    private void CreatePdf(string path, string text)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(UglyToad.PdfPig.Content.PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        page.AddText(text, 12, new UglyToad.PdfPig.Core.PdfPoint(25, 700), font);
        File.WriteAllBytes(path, builder.Build());
    }

    public void Dispose()
    {
    }
}
