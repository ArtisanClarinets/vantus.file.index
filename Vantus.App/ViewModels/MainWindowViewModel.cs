using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Vantus.Core.Models;
using Vantus.Core.Services;
using Wpf.Ui.Controls; 

namespace Vantus.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SettingsSchema _schema;

    [ObservableProperty]
    private string _applicationTitle = "Vantus File Indexer";

    public ObservableCollection<object> NavigationItems { get; } = new();

    public MainWindowViewModel(SettingsSchema schema)
    {
        _schema = schema;
        InitializeNavigation();
    }

    private void InitializeNavigation()
    {
        // Add Dashboard
        NavigationItems.Add(new NavigationViewItem
        {
            Content = "Dashboard",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
            Tag = "dashboard"
        });

        // Add Search
        NavigationItems.Add(new NavigationViewItem
        {
            Content = "Search",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Search24 },
            Tag = "search"
        });

        // Add Rules
        NavigationItems.Add(new NavigationViewItem
        {
            Content = "Automation Rules",
            Icon = new SymbolIcon { Symbol = SymbolRegular.ArrowRepeatAll24 },
            Tag = "rules"
        });

        foreach (var category in _schema.GetCategories())
        {
            var catItem = new NavigationViewItem
            {
                Content = category.Title,
                Icon = new SymbolIcon { Symbol = SymbolRegular.Folder24 },
                Tag = category.Id
            };
            
            foreach (var pageId in category.PageIds)
            {
                var page = _schema.GetPage(pageId);
                if (page != null)
                {
                    var pageItem = new NavigationViewItem
                    {
                        Content = page.Title,
                        Tag = page.Id,
                        Icon = new SymbolIcon { Symbol = SymbolRegular.Document24 }
                    };
                    catItem.MenuItems.Add(pageItem);
                }
            }
            
            NavigationItems.Add(catItem);
        }
    }
}
