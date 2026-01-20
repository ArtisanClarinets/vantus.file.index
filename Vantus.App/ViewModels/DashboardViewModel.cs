using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Threading;
using Vantus.Core.Services;

namespace Vantus.App.ViewModels;

public partial class DashboardViewModel : ObservableObject, IDisposable
{
    private readonly IEngineClient _engineClient;
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    private IndexStatus _status = new();

    [ObservableProperty]
    private string _connectionStatus = "Connecting...";

    [ObservableProperty]
    private bool _isConnected;

    public DashboardViewModel(IEngineClient engineClient)
    {
        _engineClient = engineClient;
        
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _timer.Tick += async (s, e) => await RefreshStatus();
        _timer.Start();

        // Initial refresh
        Task.Run(RefreshStatus);
    }

    private async Task RefreshStatus()
    {
        try
        {
            var status = await _engineClient.GetIndexStatusAsync();
            Status = status;
            ConnectionStatus = "Connected to Engine";
            IsConnected = true;
        }
        catch
        {
            ConnectionStatus = "Engine Offline";
            IsConnected = false;
        }
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
