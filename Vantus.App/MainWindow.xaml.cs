using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Vantus.App.Services;
using Vantus.App.Pages;
using Vantus.App.Pages.General;
using Vantus.App.ViewModels;
using Vantus.Core.Models;
using Vantus.Core.Services;
using Windows.System;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace Vantus.App;

public sealed partial class MainWindow : Window
{
    private readonly NavigationService _navigationService;
    private readonly PolicyEngine _policyEngine;
    private readonly SettingsSchema _schema;
    private readonly ITelemetryService _telemetry;
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        this.InitializeComponent();

        _telemetry = App.Services.GetRequiredService<ITelemetryService>();
        _navigationService = App.Services.GetRequiredService<NavigationService>();
        _policyEngine = App.Services.GetRequiredService<PolicyEngine>();
        _schema = App.Services.GetRequiredService<SettingsSchema>();

        _viewModel = new MainWindowViewModel(_navigationService, _policyEngine, 
            App.Services.GetRequiredService<SettingsStore>(), _telemetry);
        
        if (this.Content is FrameworkElement root)
        {
            root.DataContext = _viewModel;
        }

        SetupTitleBar();
        SetupNavigation();
        SetupKeyboardAccelerators();
        SetupErrorHandling();
        SubscribeToEvents();
        LoadInitialPage();

        WindowHelpers.SetTitleBar(this, AppTitleBar);
    }

    private void SetupTitleBar()
    {
        var store = App.Services.GetRequiredService<SettingsStore>();
        store.SettingsError += OnSettingsError;
    }

    private void SetupNavigation()
    {
        NavView.Loaded += NavView_Loaded;
        NavView.BackRequested += NavView_BackRequested;
        NavView.ItemInvoked += NavView_ItemInvoked;
        NavView.SizeChanged += NavView_SizeChanged;

        foreach (var category in _schema.Categories)
        {
            if (category.Value.Visibility == "managed_only")
            {
                var state = _policyEngine.GetCurrentState();
                if (!state.IsManaged)
                {
                    continue;
                }
            }

            var categoryItem = new NavigationViewItem
            {
                Content = category.Value.Name,
                Name = $"Category_{category.Key}",
                Tag = $"category:{category.Key}",
                Icon = GetIconForCategory(category.Key)
            };

            NavView.MenuItems.Add(categoryItem);

            foreach (var page in category.Value.Pages)
            {
                var pageItem = new NavigationViewItem
                {
                    Content = page.Value.Name,
                    Name = $"Page_{page.Key}",
                    Tag = $"page:{category.Key}:{page.Key}"
                };

                categoryItem.MenuItems.Add(pageItem);
            }
        }
    }

    private IconElement GetIconForCategory(string categoryKey)
    {
        return categoryKey switch
        {
            "general" => new SymbolIcon(Symbol.Setting),
            "indexing" => new SymbolIcon(Symbol.Find),
            "search" => new SymbolIcon(Symbol.Find),
            "appearance" => new SymbolIcon(Symbol.FontColor),
            "performance" => new SymbolIcon(Symbol.Directions),
            "privacy" => new SymbolIcon(Symbol.Admin),
            "advanced" => new SymbolIcon(Symbol.AllApps),
            "about" => new SymbolIcon(Symbol.Help),
            _ => new SymbolIcon(Symbol.Setting)
        };
    }

    private void SetupKeyboardAccelerators()
    {
        if (this.Content is not UIElement root) return;

        KeyboardAccelerator ctrlF = new()
        {
            Key = VirtualKey.F,
            Modifiers = VirtualKeyModifiers.Control,
            ScopeOwner = root
        };
        ctrlF.Invoked += CtrlF_Invoked;
        root.KeyboardAccelerators.Add(ctrlF);

        KeyboardAccelerator escape = new()
        {
            Key = VirtualKey.Escape,
            ScopeOwner = root
        };
        escape.Invoked += Escape_Invoked;
        root.KeyboardAccelerators.Add(escape);

        KeyboardAccelerator ctrlHome = new()
        {
            Key = VirtualKey.Home,
            Modifiers = VirtualKeyModifiers.Control,
            ScopeOwner = root
        };
        ctrlHome.Invoked += CtrlHome_Invoked;
        root.KeyboardAccelerators.Add(ctrlHome);

        KeyboardAccelerator ctrlR = new()
        {
            Key = VirtualKey.R,
            Modifiers = VirtualKeyModifiers.Control,
            ScopeOwner = root
        };
        ctrlR.Invoked += CtrlR_Invoked;
        root.KeyboardAccelerators.Add(ctrlR);

        KeyboardAccelerator f1 = new()
        {
            Key = VirtualKey.F1,
            ScopeOwner = root
        };
        f1.Invoked += F1_Invoked;
        root.KeyboardAccelerators.Add(f1);
    }

    private void SetupErrorHandling()
    {
        Application.Current.UnhandledException += OnUnhandledException;
    }

    private void SubscribeToEvents()
    {
        _policyEngine.PolicyChanged += OnPolicyChanged;
    }

    private void LoadInitialPage()
    {
        var initialCategory = _schema.Categories.First().Key;
        var initialPage = _schema.Categories.First().Value.Pages.First().Key;
        NavigateToPage(initialCategory, initialPage);
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateNavigationViewDisplayMode();
        ContentFrame.Navigate(typeof(SettingsPageBase), new NavigationParams
        {
            Category = "general",
            Page = "appearance_language",
            Schema = _schema
        });
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag is string tag && tag.StartsWith("page:"))
        {
            var parts = tag.Split(':');
            if (parts.Length >= 3)
            {
                NavigateToPage(parts[1], parts[2]);
            }
        }
    }

    private void NavView_SizeChanged(object sender, SizeChangedEventArgs args)
    {
        UpdateNavigationViewDisplayMode();
    }

    private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        UpdateNavigationViewDisplayMode();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
    }

    private async void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Reset Settings",
            Content = "Are you sure you want to reset all settings to their defaults? This cannot be undone.",
            PrimaryButtonText = "Reset",
            CloseButtonText = "Cancel",
            XamlRoot = Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            try
            {
                _viewModel.IsLoading = true;
                var store = App.Services.GetRequiredService<SettingsStore>();
                await store.ResetToDefaultsAsync();
                ShowSuccess("Settings have been reset to defaults");
                _telemetry.TrackEventAsync("SettingsReset");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to reset settings: {ex.Message}");
                _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
                {
                    { "Operation", "ResetSettings" }
                });
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }
    }

    private void UpdateNavigationViewDisplayMode()
    {
        if (this.Content is not FrameworkElement root) return;
        var width = root.ActualWidth;
        
        if (width >= 1008)
        {
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
        }
        else if (width >= 640)
        {
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
        }
        else
        {
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        }
    }

    private void NavigateToPage(string category, string page)
    {
        var state = _policyEngine.GetCurrentState();

        if (_schema.Categories.TryGetValue(category, out var categoryData))
        {
            if (categoryData.Visibility == "managed_only" && !state.IsManaged)
            {
                return;
            }
        }

        _viewModel.IsLoading = true;

        try
        {
            ContentFrame.Navigate(typeof(SettingsPageBase), new NavigationParams
            {
                Category = category,
                Page = page,
                Schema = _schema
            }, new SuppressNavigationTransitionInfo());

            NavView.Header = GetPageTitle(category, page);

            _telemetry.TrackEventAsync("PageNavigate", new Dictionary<string, string>
            {
                { "Category", category },
                { "Page", page }
            });
        }
        catch (Exception ex)
        {
            ShowError($"Failed to navigate to page: {ex.Message}");
            _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
            {
                { "Operation", "NavigateToPage" },
                { "Category", category },
                { "Page", page }
            });
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }

    private string GetPageTitle(string category, string page)
    {
        if (_schema.Categories.TryGetValue(category, out var cat))
        {
            if (cat.Pages.TryGetValue(page, out var pageData))
            {
                return pageData.Name;
            }
        }
        return "Settings";
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
    }

    private void OnPolicyChanged(object? sender, PolicyChangedEventArgs e)
    {
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            _telemetry.TrackEventAsync("PolicyChanged", new Dictionary<string, string>
            {
                { "ChangeType", e.ChangeType.ToString() }
            });
        });
    }

    private void OnSettingsError(object? sender, SettingsErrorEventArgs e)
    {
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            ShowError(e.Message);
        });
    }

    private async void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        _telemetry.TrackExceptionAsync(e.Exception, new Dictionary<string, string>
        {
            { "Source", "UnhandledException" }
        });
        await ShowErrorDialogAsync($"An unexpected error occurred:\n{e.Exception.Message}");
    }

    private void CtrlF_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        SearchBox.Focus(FocusState.Keyboard);
    }

    private void Escape_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (!string.IsNullOrEmpty(_viewModel.SearchQuery))
        {
            _viewModel.SearchQuery = string.Empty;
            SearchBox.Text = string.Empty;
        }
    }

    private void CtrlHome_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ContentFrame.Content is SettingsPageBase page)
        {
            page.ScrollToTop();
        }
    }

    private async void CtrlR_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        await RefreshSettingsAsync();
    }

    private void F1_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ShowHelpDialog();
    }

    private async Task RefreshSettingsAsync()
    {
        _viewModel.IsLoading = true;
        try
        {
            var store = App.Services.GetRequiredService<SettingsStore>();
            await store.RefreshAsync();
            ShowSuccess("Settings refreshed successfully");
            _telemetry.TrackEventAsync("SettingsRefreshed");
        }
        catch (Exception ex)
        {
            ShowError($"Failed to refresh settings: {ex.Message}");
            _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
            {
                { "Operation", "RefreshSettings" }
            });
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }

    private void ShowHelpDialog()
    {
        var dialog = new ContentDialog
        {
            Title = "Keyboard Shortcuts",
            Content = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        CreateShortcutRow("Ctrl + F", "Focus search box"),
                        CreateShortcutRow("Escape", "Clear search"),
                        CreateShortcutRow("Ctrl + Home", "Scroll to top of page"),
                        CreateShortcutRow("Ctrl + R", "Refresh settings"),
                        CreateShortcutRow("F1", "Show this help"),
                        CreateShortcutRow("Alt + Left", "Go back"),
                        CreateShortcutRow("Alt + Right", "Go forward")
                    }
                },
                VerticalScrollMode = ScrollMode.Enabled
            },
            CloseButtonText = "Close",
            XamlRoot = Content.XamlRoot
        };
        _ = dialog.ShowAsync();
        _telemetry.TrackEventAsync("HelpDialogOpened");
    }

    private StackPanel CreateShortcutRow(string shortcut, string description)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 4, 0, 4),
            Children =
            {
                new TextBlock
                {
                    Text = shortcut,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    MinWidth = 120
                },
                new TextBlock
                {
                    Text = description
                }
            }
        };
    }

    private async Task ShowErrorDialogAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = Content.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private void ShowError(string message)
    {
        FooterInfoBar.Message = message;
        FooterInfoBar.Severity = InfoBarSeverity.Error;
        FooterInfoBar.IsOpen = true;
        FooterInfoBar.Visibility = Visibility.Visible;
    }

    private async void ShowSuccess(string message)
    {
        FooterInfoBar.Message = message;
        FooterInfoBar.Severity = InfoBarSeverity.Success;
        FooterInfoBar.IsOpen = true;

        await Task.Delay(3000);
        FooterInfoBar.IsOpen = false;
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (!string.IsNullOrWhiteSpace(sender.Text))
        {
            _viewModel.SearchQuery = sender.Text;
            PerformSearch(sender.Text);
        }
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var results = GetSearchSuggestions(sender.Text);
            sender.ItemsSource = results.Take(5).ToList();
        }
    }

    private List<string> GetSearchSuggestions(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var suggestions = new HashSet<string>();
        var lowerText = text.ToLowerInvariant();
        var schema = _schema;

        foreach (var category in schema.Categories)
        {
            if (category.Value.Visibility == "managed_only" && !_policyEngine.GetCurrentState().IsManaged)
                continue;

            foreach (var page in category.Value.Pages)
            {
                foreach (var setting in page.Value.Settings)
                {
                    if (setting.Label.ToLowerInvariant().Contains(lowerText) ||
                        setting.Id.ToLowerInvariant().Contains(lowerText))
                    {
                        suggestions.Add(setting.Label);
                    }
                }
            }
        }

        return suggestions.ToList();
    }

    private void PerformSearch(string query)
    {
        var results = SearchSettings(query);
        if (results.Any())
        {
            var firstResult = results.First();
            NavigateToPage(firstResult.Category, firstResult.Page);
            ShowSuccess($"Found setting: {firstResult.Label}");
        }
        else
        {
            ShowError("No settings found matching your search");
        }
    }

    private List<SearchResult> SearchSettings(string query)
    {
        var results = new List<SearchResult>();
        var schema = _schema;
        var lowerQuery = query.ToLowerInvariant();

        foreach (var category in schema.Categories)
        {
            if (category.Value.Visibility == "managed_only" && !_policyEngine.GetCurrentState().IsManaged)
                continue;

            foreach (var page in category.Value.Pages)
            {
                foreach (var setting in page.Value.Settings)
                {
                    var matches = false;

                    if (setting.Label.ToLowerInvariant().Contains(lowerQuery) ||
                        setting.Id.ToLowerInvariant().Contains(lowerQuery))
                    {
                        matches = true;
                    }

                    if (matches)
                    {
                        results.Add(new SearchResult
                        {
                            SettingId = setting.Id,
                            Label = setting.Label,
                            Category = category.Key,
                            Page = page.Key
                        });
                    }
                }
            }
        }

        return results;
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        _ = RefreshSettingsAsync();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        ShowHelpDialog();
    }

    private void InfoBar_CloseButtonClick(object sender, RoutedEventArgs e)
    {
        FooterInfoBar.IsOpen = false;
    }
}

public class NavigationParams
{
    public string Category { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public SettingsSchema Schema { get; set; } = new();
}

public class SearchResult
{
    public string SettingId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
}
