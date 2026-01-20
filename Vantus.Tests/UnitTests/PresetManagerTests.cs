using Vantus.Core.Models;
using Vantus.Core.Services;
using Xunit;

namespace Vantus.Tests.UnitTests;

public class PresetManagerTests
{
    private SettingsStore _settingsStore = null!;
    private SettingsSchema _schema = null!;
    private PresetManager _presetManager = null!;

    [Fact]
    public void GetAvailablePresets_ReturnsAllFourPresets()
    {
        InitializeTest();

        var presets = _presetManager.GetAvailablePresets();

        Assert.Equal(4, presets.Count);
        Assert.Contains(presets, p => p.Id == "personal");
        Assert.Contains(presets, p => p.Id == "pro");
        Assert.Contains(presets, p => p.Id == "enterprise_private");
        Assert.Contains(presets, p => p.Id == "enterprise_automation");
    }

    [Fact]
    public void GetPreviewDiff_ReturnsChangesForPreset()
    {
        InitializeTest();

        _settingsStore.SetValue("general.theme", "Dark");

        var diff = _presetManager.GetPreviewDiff("personal");

        Assert.NotNull(diff);
        Assert.Equal("personal", diff.PresetId);
        Assert.NotEmpty(diff.Changes);
    }

    [Fact]
    public void GetPreviewDiff_NoChangesForCurrentPreset()
    {
        InitializeTest();

        _settingsStore.SetValue("general.theme", "System");

        var diff = _presetManager.GetPreviewDiff("personal");

        var themeChange = diff.Changes.FirstOrDefault(c => c.SettingId == "general.theme");
        Assert.Null(themeChange);
    }

    [Fact]
    public void GetDefaultsForPreset_ReturnsAllDefaults()
    {
        InitializeTest();

        var defaults = _presetManager.GetDefaultsForPreset("personal");

        Assert.NotEmpty(defaults);
        Assert.Contains(defaults, k => k.Key == "general.theme");
        Assert.Contains(defaults, k => k.Key == "general.reduce_motion");
    }

    private void InitializeTest()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dataPath);

        _settingsStore = new SettingsStore(dataPath);
        _settingsStore.InitializeAsync().Wait();

        _schema = new SettingsSchema
        {
            SchemaVersion = "1.0",
            Presets = new Dictionary<string, string>
            {
                { "personal", "Personal" },
                { "pro", "Pro" },
                { "enterprise_private", "Enterprise-Private" },
                { "enterprise_automation", "Enterprise-Automation" }
            },
            Categories = new Dictionary<string, SettingsCategory>
            {
                ["general"] = new SettingsCategory
                {
                    Name = "General",
                    Pages = new Dictionary<string, SettingsPage>
                    {
                        ["appearance_language"] = new SettingsPage
                        {
                            Name = "Appearance & Language",
                            Settings = new List<SettingDefinition>
                            {
                                new()
                                {
                                    Id = "general.theme",
                                    ControlType = "segmented",
                                    Label = "Theme",
                                    HelperText = "Choose theme",
                                    AllowedValues = new List<string> { "System", "Light", "Dark" },
                                    Scope = "global",
                                    RequiresRestart = false,
                                    PolicyLockable = true,
                                    Defaults = new Dictionary<string, object>
                                    {
                                        { "personal", "System" },
                                        { "pro", "System" },
                                        { "enterprise_private", "System" },
                                        { "enterprise_automation", "System" }
                                    }
                                },
                                new()
                                {
                                    Id = "general.reduce_motion",
                                    ControlType = "toggle",
                                    Label = "Reduce motion",
                                    HelperText = "Minimize animations",
                                    Scope = "global",
                                    RequiresRestart = false,
                                    PolicyLockable = true,
                                    Defaults = new Dictionary<string, object>
                                    {
                                        { "personal", false },
                                        { "pro", false },
                                        { "enterprise_private", true },
                                        { "enterprise_automation", true }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        _presetManager = new PresetManager(_settingsStore, _schema);
    }
}
