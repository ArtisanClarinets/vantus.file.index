using System.Windows;
using Vantus.App.ViewModels;
using Vantus.App.Views;
using Wpf.Ui.Controls;
using Vantus.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Vantus.App;

public partial class MainWindow : FluentWindow
{
    private readonly SettingsSchema _schema;
    private readonly SettingsStore _store;

    public MainWindow(MainWindowViewModel viewModel, SettingsSchema schema, SettingsStore store)
    {
        InitializeComponent();
        DataContext = viewModel;
        _schema = schema;
        _store = store;
        
        Loaded += (s, e) => Navigate("dashboard");
    }

    private void RootNavigation_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is NavigationView nav && nav.SelectedItem is NavigationViewItem item)
        {
            if (item.Tag is string pageId)
            {
                Navigate(pageId);
            }
        }
    }
    
    private void Navigate(string pageId)
    {
        if (pageId == "dashboard")
        {
            ContentFrame.Navigate(App.Services.GetRequiredService<DashboardPage>());
            return;
        }

        if (pageId == "search")
        {
            ContentFrame.Navigate(App.Services.GetRequiredService<SearchPage>());
            return;
        }

        if (pageId == "rules")
        {
            ContentFrame.Navigate(App.Services.GetRequiredService<RulesEditor>());
            return;
        }

        var pageDef = _schema.GetPage(pageId);
        if (pageDef != null)
        {
            var settings = _schema.GetSettingsForPage(pageId);
            var vm = new SettingsPageViewModel(pageDef, settings, _store);
            ContentFrame.Navigate(new SettingsPage { DataContext = vm });
        }
    }
}
