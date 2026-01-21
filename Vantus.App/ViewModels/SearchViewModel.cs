using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Vantus.Core.Engine;
using Vantus.Core.Models;
using System.Collections.Generic;

namespace Vantus.App.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private readonly IEngineClient _engineClient;

    [ObservableProperty]
    private string _query = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SearchResult> _results = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private SearchResult? _selectedResult;

    public SearchViewModel(IEngineClient engineClient)
    {
        _engineClient = engineClient;
    }

    // Default constructor for design time (or if service provider fails)
    public SearchViewModel() : this(new StubEngineClient()) { }

    [RelayCommand]
    public async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query)) return;

        IsLoading = true;
        try
        {
            var results = await _engineClient.SearchAsync(Query);
            Results = new ObservableCollection<SearchResult>(results);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
