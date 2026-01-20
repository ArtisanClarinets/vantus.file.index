using CommunityToolkit.Mvvm.ComponentModel;
using Vantus.Core.Models;
using Vantus.Core.Services;

namespace Vantus.App.ViewModels;

public partial class SettingsShellViewModel : ObservableObject
{
    private readonly SettingsStore _settingsStore;
    private readonly PolicyEngine _policyEngine;
    private readonly PresetManager _presetManager;

    [ObservableProperty]
    private string _currentPreset = "personal";

    [ObservableProperty]
    private bool _isManaged;

    [ObservableProperty]
    private int _pendingChanges;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public SettingsShellViewModel(
        SettingsStore settingsStore,
        PolicyEngine policyEngine,
        PresetManager presetManager)
    {
        _settingsStore = settingsStore;
        _policyEngine = policyEngine;
        _presetManager = presetManager;

        LoadState();
    }

    private void LoadState()
    {
        CurrentPreset = _settingsStore.GetValue<string>("general.preset") ?? "personal";

        var policyState = _policyEngine.GetCurrentState();
        IsManaged = policyState.IsManaged;
    }

    public void RefreshState()
    {
        LoadState();
    }

    public List<Preset> GetPresets()
    {
        return _presetManager.GetAvailablePresets();
    }

    public async Task ApplyPresetAsync(string presetId)
    {
        await _presetManager.ApplyPresetAsync(presetId);
        CurrentPreset = presetId;
    }

    public PresetDiff GetPresetPreview(string presetId)
    {
        return _presetManager.GetPreviewDiff(presetId);
    }

    public PolicyState GetPolicyState()
    {
        return _policyEngine.GetCurrentState();
    }

    public List<PolicyLock> GetPolicyLocks()
    {
        return _policyEngine.GetAllLocks();
    }
}
