using System.Text.Json;
using Vantus.Core.Models;

namespace Vantus.Core.Services;

public class SettingsSchema
{
    private readonly SettingsRegistry _registry;

    public SettingsSchema(string jsonContent)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        _registry = JsonSerializer.Deserialize<SettingsRegistry>(jsonContent, options) 
                   ?? new SettingsRegistry();
    }

    public string SchemaVersion => _registry.SchemaVersion;
    public Dictionary<string, string> Presets => _registry.Presets;
    public List<SettingsCategory> Categories => _registry.Categories;

    public List<SettingsCategory> GetCategories() => _registry.Categories;
    
    public SettingsPageDefinition? GetPage(string pageId) => 
        _registry.Pages.FirstOrDefault(p => p.Id == pageId);
    
    public List<SettingDefinition> GetSettingsForPage(string pageId) => 
        _registry.Settings.Where(s => s.PageId == pageId).ToList();
    
    public SettingDefinition? GetSetting(string settingId) => 
        _registry.Settings.FirstOrDefault(s => s.Id == settingId);

    public object? GetDefault(string settingId, string preset)
    {
        var setting = GetSetting(settingId);
        if (setting == null) return null;
        
        // JSON deserialization of "object" results in JsonElement.
        // We might need to unwrap it if we want raw types, but JsonElement is fine for now.
        
        if (setting.Defaults.TryGetValue(preset, out var val)) return val;
        if (setting.Defaults.TryGetValue("personal", out var def)) return def;
        return setting.Defaults.Values.FirstOrDefault();
    }
    
    public List<SettingDefinition> GetAllSettings() => _registry.Settings;
}
