using System.Windows;
using Wpf.Ui.Controls;
using Microsoft.Extensions.DependencyInjection;
using Vantus.App.Views;
using Vantus.App.Pages;

namespace Vantus.App;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (s, e) => NavigateToDashboard();
    }

    private void NavigateToDashboard()
    {
        // Navigate to the first item
        if (RootNavigation.MenuItems.Count > 0 && RootNavigation.MenuItems[0] is NavigationViewItem item)
        {
             if (item.TargetPageType != null)
             {
                 NavigateToType(item.TargetPageType);
             }
        }
    }

    private void OnNavigationSelectionChanged(NavigationView sender, RoutedEventArgs args)
    {
        if (sender.SelectedItem is NavigationViewItem item && item.TargetPageType != null)
        {
            NavigateToType(item.TargetPageType);
        }
    }

    private void NavigateToType(Type pageType)
    {
        // Resolve page from DI
        var page = ((App)App.Current).Host.Services.GetService(pageType) ?? Activator.CreateInstance(pageType);
        ContentFrame.Navigate(page);
    }
}
