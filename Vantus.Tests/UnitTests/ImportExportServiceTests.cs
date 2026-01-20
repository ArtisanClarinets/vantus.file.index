using Vantus.Core.Models;
using Vantus.Core.Services;
using Xunit;

namespace Vantus.Tests.UnitTests;

public class ImportExportServiceTests
{
    private SettingsStore _settingsStore = null!;
    private SettingsSchema _schema = null!;
    private ImportExportService _service = null!;

    [Fact]
    public async Task ExportSettings_CreatesValidJson()
    {
        InitializeTest();

        _settingsStore.SetValue("general.theme", "Dark");
        _settingsStore.SetValue("general.reduce_motion", true);

        var result = await _service.ExportSettingsAsync();

        Assert.True(result.Success);
        Assert.NotNull(result.Content);
        Assert.Contains("vantus-settings", result.FileName);
        Assert.Contains("Dark", result.Content);
    }

    [Fact]
    public async Task PreviewImport_ShowsValidChanges()
    {
        InitializeTest();

        _settingsStore.SetValue("general.theme", "Light");

        var exportContent = CreateTestExportJson("Dark");

        var preview = _service.PreviewImport(exportContent);

        Assert.True(preview.IsValid);
        Assert.NotEmpty(preview.Changes);
    }

    [Fact]
    public async Task PreviewImport_DetectsNoChanges()
    {
        InitializeTest();

        _settingsStore.SetValue("general.theme", "Dark");

        var exportContent = CreateTestExportJson("Dark");

        var preview = _service.PreviewImport(exportContent);

        Assert.True(preview.IsValid);
        Assert.Empty(preview.Changes);
    }

    [Fact]
    public async Task PreviewImport_ReportsInvalidJson()
    {
        InitializeTest();

        var preview = _service.PreviewImport("invalid json {");

        Assert.False(preview.IsValid);
        Assert.NotNull(preview.ErrorMessage);
    }

    [Fact]
    public async Task ImportSettings_AppliesChanges()
    {
        InitializeTest();

        _settingsStore.SetValue("general.theme", "Light");
        var exportContent = CreateTestExportJson("Dark");

        var result = await _service.ImportSettingsAsync(exportContent, apply: true);

        Assert.True(result.Success);
        Assert.Equal("Dark", _settingsStore.GetValue<string>("general.theme"));
    }

    [Fact]
    public async Task ImportSettings_PreviewOnly_DoesNotApplyChanges()
    {
        InitializeTest();

        _settingsStore.SetValue("general.theme", "Light");
        var exportContent = CreateTestExportJson("Dark");

        var result = await _service.ImportSettingsAsync(exportContent, apply: false);

        Assert.True(result.Success);
        Assert.Equal("Light", _settingsStore.GetValue<string>("general.theme"));
    }

    [Fact]
    public async Task ExportSettings_IncludesActivePreset()
    {
        InitializeTest();

        _settingsStore.SetValue("general.preset", "pro");

        var result = await _service.ExportSettingsAsync();

        Assert.True(result.Success);
        Assert.Contains("pro", result.Content);
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
            Presets = new Dictionary<string, string>(),
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
                                        { "pro", "System" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        _service = new ImportExportService(_settingsStore, _schema);
    }

    private string CreateTestExportJson(string themeValue)
    {
        var export = new SettingsExport
        {
            Version = "1.0",
            SchemaVersion = "1.0",
            ExportedAt = DateTime.UtcNow,
            ActivePreset = "personal",
            GlobalSettings = new Dictionary<string, object>
            {
                { "general.theme", themeValue }
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(export, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
