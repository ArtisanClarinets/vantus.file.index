using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vantus.Core.Models;
using Vantus.Core.Services;
using Vantus.App.Services;
using Vantus.App.Pages;
using Vantus.App.Pages.General;

namespace Vantus.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly NavigationService _navigation;
    private readonly PolicyEngine _policyEngine;
    private readonly SettingsStore _settingsStore;
    private readonly ITelemetryService _telemetry;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _pageTitle = "Settings";

    [ObservableProperty]
    private bool _isManaged;

    [ObservableProperty]
    private int _pendingChangesCount;

    [ObservableProperty]
    private List<NavigationItem> _navigationItems = new();

    public MainWindowViewModel(
        NavigationService navigation,
        PolicyEngine policyEngine,
        SettingsStore settingsStore,
        ITelemetryService telemetry)
    {
        _navigation = navigation;
        _policyEngine = policyEngine;
        _settingsStore = settingsStore;
        _telemetry = telemetry;

        InitializeNavigation();
        SubscribeToEvents();
        LoadPolicyState();
    }

    [RelayCommand]
    private void NavigateToItem(NavigationItem item)
    {
        _telemetry.TrackEventAsync("Navigation", new Dictionary<string, string>
        {
            { "Category", item.Category ?? string.Empty },
            { "Page", item.Page ?? string.Empty }
        });

        _navigation.NavigateTo(typeof(SettingsPageBase), new NavigationParams
        {
            Category = item.Category,
            Page = item.Page,
            Schema = _settingsStore.GetSchema()
        });

        PageTitle = item.DisplayName;
    }

    [RelayCommand]
    private void PerformSearch()
    {
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            _telemetry.TrackEventAsync("SettingsSearch", new Dictionary<string, string>
            {
                { "Query", SearchQuery }
            });

            var results = SearchSettings(SearchQuery);
            if (results.Any())
            {
                var firstResult = results.First();
                NavigateToItem(new NavigationItem 
                { 
                    Category = firstResult.Category, 
                    Page = firstResult.Page, 
                    DisplayName = firstResult.Label 
                });
            }
        }
    }

    private List<SearchResult> SearchSettings(string query)
    {
        var results = new List<SearchResult>();
        var schema = _settingsStore.GetSchema();
        var lowerQuery = query.ToLowerInvariant();

        foreach (var category in schema.Categories)
        {
            if (category.Value.Visibility == "managed_only" && !_isManaged)
                continue;

            foreach (var page in category.Value.Pages)
            {
                foreach (var setting in page.Value.Settings)
                {
                    var matches = false;
                    var matchContext = string.Empty;

                    if (setting.Label.ToLowerInvariant().Contains(lowerQuery))
                    {
                        matches = true;
                        matchContext = "Label";
                    }
                    else if (setting.HelperText.ToLowerInvariant().Contains(lowerQuery))
                    {
                        matches = true;
                        matchContext = "Helper Text";
                    }
                    else if (setting.Id.ToLowerInvariant().Contains(lowerQuery))
                    {
                        matches = true;
                        matchContext = "ID";
                    }

                    if (matches)
                    {
                        results.Add(new SearchResult
                        {
                            SettingId = setting.Id,
                            Label = setting.Label,
                            Category = category.Key,
                            Page = page.Key,
                            Context = matchContext
                        });
                    }
                }
            }
        }

        return results;
    }

    private void InitializeNavigation()
    {
        var schema = _settingsStore.GetSchema();
        var items = new List<NavigationItem>();

        foreach (var category in schema.Categories)
        {
            if (category.Value.Visibility == "managed_only")
            {
                var policyState = _policyEngine.GetCurrentState();
                if (!policyState.IsManaged)
                {
                    continue;
                }
            }

            var categoryItem = new NavigationItem
            {
                DisplayName = category.Value.Name,
                Category = category.Key,
                IsExpanded = true,
                Children = new List<NavigationItem>()
            };

            foreach (var page in category.Value.Pages)
            {
                categoryItem.Children.Add(new NavigationItem
                {
                    DisplayName = page.Value.Name,
                    Category = category.Key,
                    Page = page.Key
                });
            }

            items.Add(categoryItem);
        }

        NavigationItems = items;
    }

    private void SubscribeToEvents()
    {
        _settingsStore.SettingChanged += (s, e) =>
        {
            if (e.SettingId != "*")
            {
                _telemetry.TrackEventAsync("SettingChanged", new Dictionary<string, string>
                {
                    { "SettingId", e.SettingId },
                    { "Scope", e.Scope }
                });
            }
        };

        _policyEngine.PolicyChanged += (s, e) =>
        {
            _telemetry.TrackEventAsync("PolicyChanged", new Dictionary<string, string>
            {
                { "ChangeType", e.ChangeType.ToString() }
            });

            LoadPolicyState();
            InitializeNavigation();
        };
    }

    private void LoadPolicyState()
    {
        var state = _policyEngine.GetCurrentState();
        IsManaged = state.IsManaged;
    }
}

public class NavigationItem
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Page { get; set; }
    public List<NavigationItem> Children { get; set; } = new();
    public bool IsExpanded { get; set; }
    public bool IsCategory => string.IsNullOrEmpty(Page);
}

public class SearchResult
{
    public string SettingId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}
