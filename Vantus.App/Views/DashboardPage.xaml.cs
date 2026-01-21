using System.Windows.Controls;
using Vantus.App.ViewModels;

namespace Vantus.App.Views;

public partial class DashboardPage : Page
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public DashboardPage() : this(App.GetService<DashboardViewModel>())
    {
    }
}
