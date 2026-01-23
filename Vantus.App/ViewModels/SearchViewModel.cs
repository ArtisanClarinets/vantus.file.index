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
    private string _searchQuery = "";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<SearchResult> _results = new();

    public SearchViewModel(IEngineClient engineClient)
    {
        _engineClient = engineClient;
    }

    public SearchViewModel() : this(new StubEngineClient()) { }

    [RelayCommand]
    private async Task PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        IsLoading = true;
        try
        {
            var results = await _engineClient.SearchAsync(SearchQuery);
            Results.Clear();
            foreach (var r in results) Results.Add(r);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
