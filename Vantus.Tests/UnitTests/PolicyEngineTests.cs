using Vantus.Core.Models;
using Vantus.Core.Services;
using Xunit;

namespace Vantus.Tests.UnitTests;

public class PolicyEngineTests
{
    private PolicyEngine _policyEngine = null!;

    [Fact]
    public void IsSettingLocked_ReturnsFalse_WhenNoPolicyLoaded()
    {
        InitializeTest();

        var result = _policyEngine.IsSettingLocked("test.setting");

        Assert.False(result);
    }

    [Fact]
    public void GetLockState_ReturnsNotLocked_WhenNoPolicy()
    {
        InitializeTest();

        var state = _policyEngine.GetLockState("test.setting");

        Assert.False(state.IsLocked);
        Assert.Equal("test.setting", state.SettingId);
    }

    [Fact]
    public void LoadPolicyFromPath_UpdatesLockState()
    {
        InitializeTest();

        var policyPath = CreateTestPolicyFile();
        _policyEngine.LoadPolicyFromPathAsync(policyPath).Wait();

        var state = _policyEngine.GetLockState("privacy.encrypt_index_db");

        Assert.True(state.IsLocked);
        Assert.Equal(true, state.LockedValue);
        Assert.Equal("MDM", state.Source);
    }

    [Fact]
    public void GetEffectiveValue_ReturnsLockedValue_WhenLocked()
    {
        InitializeTest();

        var policyPath = CreateTestPolicyFile();
        _policyEngine.LoadPolicyFromPathAsync(policyPath).Wait();

        var effectiveValue = _policyEngine.GetEffectiveValue("privacy.encrypt_index_db", false);

        Assert.Equal(true, effectiveValue);
    }

    [Fact]
    public void GetEffectiveValue_ReturnsUserValue_WhenNotLocked()
    {
        InitializeTest();

        var policyPath = CreateTestPolicyFile();
        _policyEngine.LoadPolicyFromPathAsync(policyPath).Wait();

        var effectiveValue = _policyEngine.GetEffectiveValue("some.other.setting", "user_value");

        Assert.Equal("user_value", effectiveValue);
    }

    [Fact]
    public void CanUserChange_ReturnsFalse_WhenLocked()
    {
        InitializeTest();

        var policyPath = CreateTestPolicyFile();
        _policyEngine.LoadPolicyFromPathAsync(policyPath).Wait();

        var canChange = _policyEngine.CanUserChange("privacy.encrypt_index_db");

        Assert.False(canChange);
    }

    [Fact]
    public void CanUserChange_ReturnsTrue_WhenNotLocked()
    {
        InitializeTest();

        var policyPath = CreateTestPolicyFile();
        _policyEngine.LoadPolicyFromPathAsync(policyPath).Wait();

        var canChange = _policyEngine.CanUserChange("some.other.setting");

        Assert.True(canChange);
    }

    [Fact]
    public void GetCurrentState_ReturnsManagedFalse_WhenNoPolicy()
    {
        InitializeTest();

        var state = _policyEngine.GetCurrentState();

        Assert.False(state.IsManaged);
    }

    [Fact]
    public void ExportPolicy_CreatesValidJson()
    {
        InitializeTest();

        var policyPath = CreateTestPolicyFile();
        _policyEngine.LoadPolicyFromPathAsync(policyPath).Wait();

        var exportPath = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}.json");
        _policyEngine.ExportPolicyAsync(exportPath).Wait();

        Assert.True(File.Exists(exportPath));

        var content = File.ReadAllText(exportPath);
        Assert.Contains("managed", content);
        Assert.Contains("locks", content);

        File.Delete(exportPath);
    }

    private void InitializeTest()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dataPath);

        _policyEngine = new PolicyEngine(dataPath);
    }

    private string CreateTestPolicyFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"policy_{Guid.NewGuid()}.json");
        var policy = new PolicyFile
        {
            Managed = true,
            Locks = new List<PolicyLock>
            {
                new()
                {
                    SettingId = "privacy.encrypt_index_db",
                    LockedValue = true,
                    Reason = "Encryption is required.",
                    Source = "MDM"
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(policy, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
        return path;
    }
}
