using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vantus.Core.Engine;

namespace Vantus.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IEngineClient _engineClient;

    [ObservableProperty]
    private string _indexStatus = "Checking...";

    public DashboardViewModel(IEngineClient engineClient)
    {
        _engineClient = engineClient;
        RefreshStatus();
    }

    [RelayCommand]
    public async Task RefreshStatus()
    {
        try
        {
            IndexStatus = await _engineClient.GetIndexStatusAsync();
        }
        catch
        {
            IndexStatus = "Unavailable";
        }
    }

    [RelayCommand]
    public async Task PauseIndexing()
    {
        await _engineClient.PauseIndexingAsync();
        await RefreshStatus();
    }

    [RelayCommand]
    public async Task ResumeIndexing()
    {
        await _engineClient.ResumeIndexingAsync();
        await RefreshStatus();
    }
}
