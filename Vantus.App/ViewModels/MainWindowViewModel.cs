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
                Icon = new SymbolIcon { Symbol = GetCategoryIcon(category.Id) },
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

    private SymbolRegular GetCategoryIcon(string categoryId)
    {
        return categoryId switch
        {
            "general" => SymbolRegular.Settings24,
            "workspaces" => SymbolRegular.Briefcase24,
            "locations" => SymbolRegular.Folder24,
            "indexing" => SymbolRegular.ArrowSync24,
            "ai_models" => SymbolRegular.BrainCircuit24,
            "extraction" => SymbolRegular.DocumentText24,
            "organize_and_automations" => SymbolRegular.Flowchart24,
            "tags_and_taxonomy" => SymbolRegular.Tag24,
            "partners" => SymbolRegular.People24,
            "search" => SymbolRegular.Search24,
            "windows_integration" => SymbolRegular.Window24,
            "privacy_and_security" => SymbolRegular.Shield24,
            "compliance_and_audit" => SymbolRegular.Notepad24,
            "notifications" => SymbolRegular.Alert24,
            "storage_and_maintenance" => SymbolRegular.Storage24,
            "diagnostics" => SymbolRegular.Pulse24,
            "admin_managed" => SymbolRegular.LockClosed24,
            "about" => SymbolRegular.Info24,
            _ => SymbolRegular.Folder24
        };
    }
}
