using Vantus.Engine;
using Vantus.Engine.Services;
using Vantus.Engine.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using System.IO;

using Vantus.Engine.Services.Indexing;
using Vantus.Engine.Services.Extraction;
using Vantus.Engine.Services.AI;

var builder = WebApplication.CreateBuilder(args);

// Configure DB Path
var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var dbPath = Path.Combine(localAppData, "Vantus", "vantus.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContext<VantusDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Register Indexing Services
builder.Services.AddSingleton<IndexingChannel>();
builder.Services.AddSingleton<FileMonitorService>();
builder.Services.AddHostedService<FileIndexerService>();

// Register Extractors
builder.Services.AddSingleton<IContentExtractor, PdfExtractor>();
builder.Services.AddSingleton<IContentExtractor, OfficeExtractor>();
builder.Services.AddSingleton<CompositeContentExtractor>();

// Register AI Services
builder.Services.AddSingleton<IEmbeddingService, OnnxEmbeddingService>();
builder.Services.AddScoped<Vantus.Engine.Services.Search.VectorSearchService>();

// Configure Kestrel for gRPC
builder.WebHost.ConfigureKestrel(options =>
{
    // Setup a HTTP/2 endpoint without TLS for local IPC
    options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddGrpc();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapGrpcService<EngineService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VantusDbContext>();
    db.Database.Migrate();
}

app.Run();
