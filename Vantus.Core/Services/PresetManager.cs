using Vantus.Core.Models;

namespace Vantus.Core.Services;

public class PresetManager
{
    private readonly SettingsStore _store;
    private readonly SettingsSchema _schema;
    
    public PresetManager(SettingsStore store, SettingsSchema schema)
    {
        _store = store;
        _schema = schema;
    }

    public async Task ApplyPresetAsync(string presetName)
    {
        var settings = _schema.GetAllSettings();
        foreach (var setting in settings)
        {
            var def = _schema.GetDefault(setting.Id, presetName);
            if (def != null)
            {
                _store.SetValue(setting.Id, def);
            }
        }
        
        await _store.SaveSettingsAsync();
    }
}
