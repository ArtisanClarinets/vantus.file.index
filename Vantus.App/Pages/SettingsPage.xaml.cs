using System.Windows.Controls;
using Vantus.App.ViewModels;

namespace Vantus.App.Pages;

public partial class SettingsPage : Page
{
    public SettingsPageViewModel ViewModel { get; }

    // Constructor used by DI
    public SettingsPage(SettingsPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
    
    // Default constructor for XAML / generic instantiation if DI fails
    public SettingsPage() : this(App.GetService<SettingsPageViewModel>()) {}
}
