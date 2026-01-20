using Vantus.Core.Models;

namespace Vantus.Core.Services;

public class PresetManager
{
    private readonly SettingsStore _settingsStore;
    private readonly SettingsSchema _schema;
    private readonly ITelemetryService _telemetry;
    private readonly Dictionary<string, Preset> _presets = new();

    public PresetManager(SettingsStore settingsStore, SettingsSchema schema, ITelemetryService? telemetry = null)
    {
        _settingsStore = settingsStore;
        _schema = schema;
        _telemetry = telemetry ?? new NullTelemetryService();
        InitializePresets();
    }

    private void InitializePresets()
    {
        foreach (var preset in _schema.Presets)
        {
            _presets[preset.Key] = new Preset
            {
                Id = preset.Key,
                Name = preset.Value,
                Description = GetPresetDescription(preset.Key),
                Intent = GetPresetIntent(preset.Key),
                Settings = new Dictionary<string, object>()
            };
        }
    }

    public List<Preset> GetAvailablePresets() => _presets.Values.ToList();

    public Preset? GetPreset(string presetId)
    {
        return _presets.TryGetValue(presetId, out var preset) ? preset : null;
    }

    public PresetDiff GetPreviewDiff(string presetId)
    {
        var preset = GetPreset(presetId);
        if (preset == null)
        {
            return new PresetDiff { PresetId = presetId };
        }

        var diff = new PresetDiff
        {
            PresetId = presetId,
            PresetName = preset.Name
        };

        foreach (var category in _schema.Categories)
        {
            foreach (var page in category.Value.Pages)
            {
                foreach (var setting in page.Value.Settings)
                {
                    var currentValue = _settingsStore.GetValue<object>(setting.Id);
                    var presetValue = GetPresetValue(setting, presetId);

                    if (!ValuesEqual(currentValue, presetValue))
                    {
                        diff.Changes.Add(new PresetChange
                        {
                            SettingId = setting.Id,
                            SettingLabel = setting.Label,
                            CurrentValue = currentValue,
                            NewValue = presetValue,
                            Category = category.Key,
                            Page = page.Key
                        });
                    }
                }
            }
        }

        return diff;
    }

    public async Task ApplyPresetAsync(string presetId)
    {
        var preset = GetPreset(presetId);
        if (preset == null) return;

        var startTime = DateTime.UtcNow;
        try
        {
            foreach (var category in _schema.Categories)
            {
                foreach (var page in category.Value.Pages)
                {
                    foreach (var setting in page.Value.Settings)
                    {
                        var presetValue = GetPresetValue(setting, presetId);
                        if (presetValue != null)
                        {
                            _settingsStore.SetValue(setting.Id, presetValue, setting.Scope);
                        }
                    }
                }
            }

            _settingsStore.SetValue("general.preset", presetId);
            await _settingsStore.SaveSettingsAsync();

            await _telemetry.TrackEventAsync("PresetApplied", new Dictionary<string, string>
            {
                { "PresetId", presetId },
                { "PresetName", preset.Name }
            });
        }
        catch (Exception ex)
        {
            await _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
            {
                { "Operation", "ApplyPresetAsync" },
                { "PresetId", presetId }
            });
            throw;
        }
    }

    public Dictionary<string, object> GetDefaultsForPreset(string presetId)
    {
        var defaults = new Dictionary<string, object>();

        foreach (var category in _schema.Categories)
        {
            foreach (var page in category.Value.Pages)
            {
                foreach (var setting in page.Value.Settings)
                {
                    var value = GetPresetValue(setting, presetId);
                    if (value != null)
                    {
                        defaults[setting.Id] = value;
                    }
                }
            }
        }

        return defaults;
    }

    private object? GetPresetValue(SettingDefinition setting, string presetId)
    {
        if (setting.Defaults.TryGetValue(presetId, out var value))
        {
            return value;
        }
        return null;
    }

    private bool ValuesEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        if (a is List<object> listA && b is List<object> listB)
        {
            if (listA.Count != listB.Count) return false;
            return listA.SequenceEqual(listB);
        }

        return a.Equals(b);
    }

    private string GetPresetDescription(string presetId)
    {
        return presetId switch
        {
            "personal" => "Best experience with helpful previews and suggestions over automation.",
            "pro" => "Power user mode with more aggressive extraction and approval-based organizing.",
            "enterprise_private" => "Privacy-first mode with minimized content retention and strong controls.",
            "enterprise_automation" => "Managed environment where trusted rules can auto-run with heavy auditing.",
            _ => string.Empty
        };
    }

    private string GetPresetIntent(string presetId)
    {
        return presetId switch
        {
            "personal" => "Best experience",
            "pro" => "Power user",
            "enterprise_private" => "Privacy-first",
            "enterprise_automation" => "Automation & auditing",
            _ => string.Empty
        };
    }
}
