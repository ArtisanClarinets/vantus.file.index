using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Vantus.Engine.Data;

public class VantusDbContext : DbContext
{
    public DbSet<FileEntity> Files { get; set; }
    public DbSet<EmbeddingEntity> Embeddings { get; set; }
    public DbSet<IndexQueueItem> IndexQueue { get; set; }
    public DbSet<TagEntity> Tags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vantus");
            Directory.CreateDirectory(folder);
            var dbPath = Path.Combine(folder, "index.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileEntity>()
            .HasIndex(f => f.Path)
            .IsUnique();

        modelBuilder.Entity<TagEntity>()
            .HasIndex(t => t.Name)
            .IsUnique();
    }
}
