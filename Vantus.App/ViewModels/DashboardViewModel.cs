using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using Vantus.Core.Engine;

namespace Vantus.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IEngineClient _engineClient;
    private readonly CancellationTokenSource _cts = new();

    [ObservableProperty]
    private string _status = "Unknown";

    public DashboardViewModel(IEngineClient engineClient)
    {
        _engineClient = engineClient;
        _ = PollStatusAsync();
    }

    public DashboardViewModel() : this(new StubEngineClient()) { }

    private async Task PollStatusAsync()
    {
        var timer = new PeriodicTimer(System.TimeSpan.FromSeconds(2));
        while (await timer.WaitForNextTickAsync(_cts.Token))
        {
            try
            {
                Status = await _engineClient.GetIndexStatusAsync();
            }
            catch
            {
                Status = "Connection Failed";
            }
        }
    }
}
