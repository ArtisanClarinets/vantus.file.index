using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Vantus.Core.Services;

namespace Vantus.App.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private readonly IEngineClient _engineClient;

    [ObservableProperty]
    private string _searchQuery = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    [NotifyPropertyChangedFor(nameof(NoResultsFound))]
    private bool _isSearching;

    [ObservableProperty]
    private string _statusMessage = "";

    public ObservableCollection<SearchResultItem> Results { get; } = new();

    public bool HasResults => Results.Count > 0;
    public bool NoResultsFound => !IsSearching && !HasResults && !string.IsNullOrEmpty(StatusMessage);

    public SearchViewModel(IEngineClient engineClient)
    {
        _engineClient = engineClient;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        IsSearching = true;
        StatusMessage = "Searching...";
        Results.Clear();
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(NoResultsFound));

        try
        {
            var response = await _engineClient.SearchAsync(SearchQuery);
            
            foreach (var item in response.Results)
            {
                Results.Add(item);
            }

            StatusMessage = $"Found {response.TotalCount} results.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(NoResultsFound));
        }
    }
}
