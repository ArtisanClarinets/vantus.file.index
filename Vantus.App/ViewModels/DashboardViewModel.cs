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

    [ObservableProperty]
    private long _filesIndexed;

    [ObservableProperty]
    private long _totalTags;

    [ObservableProperty]
    private long _totalPartners;

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
                var stats = await _engineClient.GetStatsAsync();
                Status = stats.Status;
                FilesIndexed = stats.FilesIndexed;
                TotalTags = stats.TotalTags;
                TotalPartners = stats.TotalPartners;
            }
            catch
            {
                Status = "Connection Failed";
            }
        }
    }
}
