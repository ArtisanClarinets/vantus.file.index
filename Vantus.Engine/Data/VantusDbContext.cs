using Microsoft.EntityFrameworkCore;
using Vantus.Engine.Data.Entities;

namespace Vantus.Engine.Data;

public class VantusDbContext : DbContext
{
    public DbSet<FileIndexItem> Files { get; set; }
    public DbSet<FileEmbedding> FileEmbeddings { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<FileTag> FileTags { get; set; }

    public VantusDbContext(DbContextOptions<VantusDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure FileIndexItem
        modelBuilder.Entity<FileIndexItem>()
            .HasIndex(f => f.FilePath)
            .IsUnique();

        // Configure FileEmbedding
        modelBuilder.Entity<FileEmbedding>()
            .HasOne(e => e.File)
            .WithMany() // Assuming one-to-many or one-to-one, keeping it simple for now
            .HasForeignKey(e => e.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure FileTag many-to-many relationship
        modelBuilder.Entity<FileTag>()
            .HasKey(ft => new { ft.FileId, ft.TagId });

        modelBuilder.Entity<FileTag>()
            .HasOne(ft => ft.File)
            .WithMany(f => f.FileTags)
            .HasForeignKey(ft => ft.FileId);

        modelBuilder.Entity<FileTag>()
            .HasOne(ft => ft.Tag)
            .WithMany(t => t.FileTags)
            .HasForeignKey(ft => ft.TagId);
            
        // Configure Tag
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name); // Ideally unique per source, or just unique name
    }
}
