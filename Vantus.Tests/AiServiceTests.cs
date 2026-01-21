using Vantus.Engine.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;
using System.Threading.Tasks;

namespace Vantus.Tests;

public class AiServiceTests
{
    private readonly Mock<ILogger<AiService>> _loggerMock;

    public AiServiceTests()
    {
        _loggerMock = new Mock<ILogger<AiService>>();
    }

    [Fact]
    public async Task ProcessFileAsync_Fallback_TagsCorrectly()
    {
        // Setup dependencies
        var dbLogger = new Mock<ILogger<DatabaseService>>();
        var dbPath = Path.Combine(Path.GetTempPath(), $"vantus_test_ai_{System.Guid.NewGuid()}.db");
        var db = new DatabaseService(dbLogger.Object, dbPath);
        await db.InitializeAsync();

        var tagLogger = new Mock<ILogger<TagService>>();
        var tagService = new TagService(db, tagLogger.Object);

        var aiService = new AiService(tagService, _loggerMock.Object);

        var content = "This is a confidential contract agreement.";
        await aiService.ProcessFileAsync("test.txt", content);

        using var conn = db.GetConnection();
        // Check if tags were created and applied
        // Wait, ProcessFileAsync calls _tagService.TagFileAsync.
        // Let's verify via DB.

        // Tags are created on demand?
        var tags = await Dapper.SqlMapper.QueryAsync<string>(conn, "SELECT name FROM tags");

        Assert.Contains("Sensitive", tags);
        Assert.Contains("Legal", tags);

        if(File.Exists(dbPath)) File.Delete(dbPath);
    }
}
