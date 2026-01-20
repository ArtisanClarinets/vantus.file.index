
using Vantus.Engine.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Data.Sqlite;
using Dapper;

namespace Vantus.Tests;

public class EngineTests : IDisposable
{
    private readonly Mock<ILogger<DatabaseService>> _dbLoggerMock;
    private readonly Mock<ILogger<IndexerService>> _indexerLoggerMock;
    private readonly Mock<ILogger<TagService>> _tagLoggerMock;
    private readonly Mock<ILogger<RulesEngineService>> _rulesLoggerMock;
    private readonly string _testDbPath;

    public EngineTests()
    {
        _dbLoggerMock = new Mock<ILogger<DatabaseService>>();
        _indexerLoggerMock = new Mock<ILogger<IndexerService>>();
        _tagLoggerMock = new Mock<ILogger<TagService>>();
        _rulesLoggerMock = new Mock<ILogger<RulesEngineService>>();
        _testDbPath = Path.Combine(Path.GetTempPath(), $"vantus_test_{Guid.NewGuid()}.db");
    }

    public void Dispose()
    {
         if (File.Exists(_testDbPath)) File.Delete(_testDbPath);
    }

    [Fact]
    public async Task DatabaseService_Initialize_CreatesTables()
    {
        var db = new DatabaseService(_dbLoggerMock.Object, _testDbPath);
        await db.InitializeAsync();

        using var conn = db.GetConnection();
        var tableCount = await conn.ExecuteScalarAsync<int>("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='files'");
        Assert.Equal(1, tableCount);

        var tagCount = await conn.ExecuteScalarAsync<int>("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='tags'");
        Assert.Equal(1, tagCount);
    }

    [Fact]
    public async Task IndexerService_IndexFile_InsertsData_And_AppliesRules()
    {
        var db = new DatabaseService(_dbLoggerMock.Object, _testDbPath);
        await db.InitializeAsync();

        var tagService = new TagService(db, _tagLoggerMock.Object);
        var rulesService = new RulesEngineService(tagService, db, _rulesLoggerMock.Object);
        var indexer = new IndexerService(db, rulesService, _indexerLoggerMock.Object);

        // Create dummy pdf
        var dummyFile = Path.Combine(Path.GetTempPath(), "test_doc.pdf");
        await File.WriteAllTextAsync(dummyFile, "PDF Placeholder");

        try
        {
            await indexer.IndexFileAsync(dummyFile);

            using var conn = db.GetConnection();
            var row = await conn.QuerySingleAsync("SELECT * FROM files WHERE path = @Path", new { Path = dummyFile });
            Assert.Equal("test_doc.pdf", row.name);

            // Verify Rule applied (PDF -> Document tag)
            var tagCount = await conn.ExecuteScalarAsync<int>(
                "SELECT count(*) FROM file_tags ft JOIN tags t ON ft.tag_id = t.id WHERE t.name = 'Document' AND ft.file_id = @FileId",
                new { FileId = row.id });
            Assert.Equal(1, tagCount);
        }
        finally
        {
            if (File.Exists(dummyFile)) File.Delete(dummyFile);
        }
    }
}
