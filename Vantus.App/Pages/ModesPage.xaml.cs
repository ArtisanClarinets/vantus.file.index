using System.Windows.Controls;
using Vantus.App.ViewModels;

namespace Vantus.App.Pages;

public partial class ModesPage : Page
{
    public ModesPageViewModel ViewModel { get; }

    public ModesPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<ModesPageViewModel>();
        DataContext = this;
    }
}
