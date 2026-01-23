using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Vantus.Core.Engine;
using Vantus.Core.Models;

namespace Vantus.App.ViewModels;

public partial class PartnersViewModel : ObservableObject
{
    private readonly IEngineClient _engineClient;

    [ObservableProperty]
    private ObservableCollection<Partner> _partners = new();

    [ObservableProperty]
    private string _newPartnerName = "";

    [ObservableProperty]
    private string _newPartnerDomains = "";

    [ObservableProperty]
    private string _newPartnerKeywords = "";

    public PartnersViewModel(IEngineClient engineClient)
    {
        _engineClient = engineClient;
        _ = LoadPartnersAsync();
    }

    public PartnersViewModel() : this(new StubEngineClient()) { }

    [RelayCommand]
    private async Task LoadPartnersAsync()
    {
        var partners = await _engineClient.GetPartnersAsync();
        Partners.Clear();
        foreach (var p in partners) Partners.Add(p);
    }

    [RelayCommand]
    private async Task AddPartnerAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPartnerName)) return;
        var p = new Partner
        {
            Name = NewPartnerName,
            Domains = NewPartnerDomains,
            Keywords = NewPartnerKeywords
        };
        await _engineClient.AddPartnerAsync(p);
        NewPartnerName = "";
        NewPartnerDomains = "";
        NewPartnerKeywords = "";
        await LoadPartnersAsync();
    }
}
