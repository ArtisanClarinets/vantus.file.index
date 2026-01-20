using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vantus.Core.Interfaces;
using Vantus.Core.Services;
using Vantus.Engine;
using Vantus.Engine.Data;
using Vantus.Engine.Services;
using Vantus.Engine.Services.Indexing;
using Vantus.Engine.Services.AI;
using Vantus.Engine.Services.Search;

var builder = Host.CreateApplicationBuilder(args);

// Core Services
builder.Services.AddSingleton<ISettingsStore, SettingsStore>();
builder.Services.AddSingleton<ISettingsRegistry, SettingsRegistry>();

// Data
builder.Services.AddDbContext<VantusDbContext>();

// Engine Services
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<FileIndexerService>();
builder.Services.AddSingleton<FileMonitorService>();
builder.Services.AddSingleton<OnnxEmbeddingService>();
builder.Services.AddSingleton<VectorSearchService>();

builder.Services.AddSingleton<TagService>();
builder.Services.AddSingleton<PartnerService>();
builder.Services.AddSingleton<AiService>();
builder.Services.AddSingleton<RulesEngineService>();
builder.Services.AddSingleton<ActionLogService>();
builder.Services.AddSingleton<FileCrawlerService>();
builder.Services.AddSingleton<IpcServer>();
builder.Services.AddHostedService<EngineWorker>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var host = builder.Build();

// Ensure Settings are loaded
var store = host.Services.GetRequiredService<ISettingsStore>();
await store.LoadAsync();

// Ensure DB created
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VantusDbContext>();
    db.Database.EnsureCreated();
}

await host.RunAsync();
