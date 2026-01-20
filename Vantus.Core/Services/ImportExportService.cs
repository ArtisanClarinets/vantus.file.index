using System.Text.Json;
using Vantus.Core.Models;

namespace Vantus.Core.Services;

public class ImportExportService
{
    private readonly SettingsStore _store;
    private readonly SettingsSchema _schema;
    
    public ImportExportService(SettingsStore store, SettingsSchema schema)
    {
        _store = store;
        _schema = schema;
    }

    public async Task ExportAsync(string filePath)
    {
        var snapshot = _store.GetSnapshot();
        var exportData = new Dictionary<string, object>
        {
            { "schema_version", _schema.SchemaVersion },
            { "exported_at", DateTime.UtcNow },
            { "settings", snapshot }
        };
        
        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task ImportAsync(string filePath)
    {
        if (!File.Exists(filePath)) return;
        
        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        
        if (data != null && data.TryGetValue("settings", out var settingsObj) && settingsObj is JsonElement je)
        {
            var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(je.GetRawText());
            if (settings != null)
            {
                _store.ApplySnapshot(settings);
            }
        }
    }
}
