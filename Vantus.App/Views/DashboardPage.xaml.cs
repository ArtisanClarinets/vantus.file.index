using System.Windows.Controls;
using Vantus.App.ViewModels;

namespace Vantus.App.Views;

public partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage(DashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public DashboardPage() : this(App.GetService<DashboardViewModel>()) { }
}
