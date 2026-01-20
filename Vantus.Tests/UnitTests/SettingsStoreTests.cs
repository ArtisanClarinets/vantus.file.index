using Vantus.Core.Models;
using Vantus.Core.Services;
using Xunit;

namespace Vantus.Tests.UnitTests;

public class SettingsStoreTests
{
    private SettingsStore _settingsStore = null!;

    [Fact]
    public async Task SetValue_StoresGlobalSetting()
    {
        InitializeTest();

        _settingsStore.SetValue("test.setting", "test_value");

        var value = _settingsStore.GetValue<string>("test.setting");

        Assert.Equal("test_value", value);
    }

    [Fact]
    public async Task SetValue_StoresWorkspaceSetting()
    {
        InitializeTest();

        _settingsStore.SetValue("workspace.setting", "workspace_value", "workspace", "work-workspace");

        var value = _settingsStore.GetValue<string>("workspace.setting", "workspace", "work-workspace");

        Assert.Equal("workspace_value", value);
    }

    [Fact]
    public async Task SetValue_StoresLocationSetting()
    {
        InitializeTest();

        _settingsStore.SetValue("location.setting", "location_value", "location", "C:\\Data");

        var value = _settingsStore.GetValue<string>("location.setting", "location", "C:\\Data");

        Assert.Equal("location_value", value);
    }

    [Fact]
    public async Task GetValue_ReturnsNull_ForUnknownSetting()
    {
        InitializeTest();

        var value = _settingsStore.GetValue<string>("unknown.setting");

        Assert.Null(value);
    }

    [Fact]
    public async Task GetValueWithPolicy_ReturnsPolicyValue_WhenLocked()
    {
        InitializeTest();

        _settingsStore.SetValue("test.setting", "user_value");

        var policyState = new PolicyState
        {
            IsManaged = true,
            ActiveLocks = new Dictionary<string, PolicyLock>
            {
                ["test.setting"] = new PolicyLock
                {
                    SettingId = "test.setting",
                    LockedValue = "policy_value",
                    Reason = "Test policy",
                    Source = "Test"
                }
            }
        };

        var value = _settingsStore.GetValueWithPolicy("test.setting", policyState);

        Assert.Equal("policy_value", value);
    }

    [Fact]
    public async Task GetValueWithPolicy_ReturnsUserValue_WhenNotLocked()
    {
        InitializeTest();

        _settingsStore.SetValue("test.setting", "user_value");

        var policyState = new PolicyState();

        var value = _settingsStore.GetValueWithPolicy("test.setting", policyState);

        Assert.Equal("user_value", value);
    }

    [Fact]
    public async Task GetLockInfo_ReturnsLockedState()
    {
        InitializeTest();

        var policyState = new PolicyState
        {
            IsManaged = true,
            ActiveLocks = new Dictionary<string, PolicyLock>
            {
                ["test.setting"] = new PolicyLock
                {
                    SettingId = "test.setting",
                    LockedValue = true,
                    Reason = "Required by policy",
                    Source = "MDM"
                }
            }
        };

        var lockInfo = _settingsStore.GetLockInfo("test.setting", policyState);

        Assert.True(lockInfo.IsLocked);
        Assert.Equal("MDM", lockInfo.Source);
        Assert.Equal("Required by policy", lockInfo.Reason);
    }

    [Fact]
    public async Task GetLockInfo_ReturnsNotLocked_WhenNoPolicy()
    {
        InitializeTest();

        var policyState = new PolicyState();

        var lockInfo = _settingsStore.GetLockInfo("test.setting", policyState);

        Assert.False(lockInfo.IsLocked);
    }

    [Fact]
    public async Task ResetToDefaults_ClearsAllSettings()
    {
        InitializeTest();

        _settingsStore.SetValue("test.setting1", "value1");
        _settingsStore.SetValue("test.setting2", "value2");

        await _settingsStore.ResetToDefaultsAsync("personal");

        Assert.Null(_settingsStore.GetValue<string>("test.setting1"));
        Assert.Null(_settingsStore.GetValue<string>("test.setting2"));
    }

    [Fact]
    public async Task SettingChanged_EventFires_OnValueChange()
    {
        InitializeTest();

        var eventFired = false;
        SettingChangedEventArgs? args = null;

        _settingsStore.SettingChanged += (s, e) =>
        {
            eventFired = true;
            args = e;
        };

        _settingsStore.SetValue("test.setting", "new_value");

        Assert.True(eventFired);
        Assert.NotNull(args);
        Assert.Equal("test.setting", args.SettingId);
    }

    [Fact]
    public async Task GetKey_GeneratesCorrectKeys()
    {
        InitializeTest();

        var globalKey = _settingsStore.GetKey("setting", "global", null, null);
        var workspaceKey = _settingsStore.GetKey("setting", "workspace", "ws1", null);
        var locationKey = _settingsStore.GetKey("setting", "location", null, "C:\\Data");

        Assert.Equal("setting", globalKey);
        Assert.Equal("ws1:setting", workspaceKey);
        Assert.Equal("C:\\Data:setting", locationKey);
    }

    private void InitializeTest()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dataPath);

        _settingsStore = new SettingsStore(dataPath);
        _settingsStore.InitializeAsync().Wait();
    }
}
