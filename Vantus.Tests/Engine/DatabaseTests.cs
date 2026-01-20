using Microsoft.EntityFrameworkCore;
using Vantus.Engine.Data;
using Vantus.Engine.Data.Entities;
using Xunit;

namespace Vantus.Tests.Engine;

public class DatabaseTests
{
    private VantusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<VantusDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new VantusDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void Can_Save_And_Retrieve_FileIndexItem()
    {
        using var context = CreateContext();

        var item = new FileIndexItem
        {
            FilePath = @"C:\Test\doc.txt",
            FileName = "doc.txt",
            Extension = ".txt",
            SizeBytes = 1024,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            LastScannedAt = DateTime.UtcNow
        };

        context.Files.Add(item);
        context.SaveChanges();

        var saved = context.Files.FirstOrDefault(f => f.FileName == "doc.txt");
        Assert.NotNull(saved);
        Assert.Equal(@"C:\Test\doc.txt", saved!.FilePath);
        Assert.True(saved.Id > 0);
    }

    [Fact]
    public void Can_Save_And_Retrieve_Tags()
    {
        using var context = CreateContext();

        var tag = new Tag { Name = "Important", Source = "User" };
        context.Tags.Add(tag);
        context.SaveChanges();

        var saved = context.Tags.FirstOrDefault(t => t.Name == "Important");
        Assert.NotNull(saved);
        Assert.Equal("User", saved!.Source);
    }

    [Fact]
    public void Can_Link_File_And_Tag()
    {
        using var context = CreateContext();

        var file = new FileIndexItem
        {
            FilePath = @"C:\Test\tagged.txt",
            FileName = "tagged.txt"
        };
        var tag = new Tag { Name = "Work" };

        context.Files.Add(file);
        context.Tags.Add(tag);
        context.SaveChanges();

        context.FileTags.Add(new FileTag
        {
            FileId = file.Id,
            TagId = tag.Id,
            Confidence = 0.9
        });
        context.SaveChanges();

        var fileWithTags = context.Files
            .Include(f => f.FileTags)
            .ThenInclude(ft => ft.Tag)
            .FirstOrDefault(f => f.Id == file.Id);

        Assert.NotNull(fileWithTags);
        Assert.Single(fileWithTags!.FileTags);
        Assert.Equal("Work", fileWithTags.FileTags.First().Tag.Name);
    }
}
