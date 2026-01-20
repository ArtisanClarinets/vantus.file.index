using System.Windows.Controls;
using Vantus.App.ViewModels;

namespace Vantus.App.Pages;

public partial class ImportExportPage : Page
{
    public ImportExportPageViewModel ViewModel { get; }

    public ImportExportPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<ImportExportPageViewModel>();
        DataContext = this;
    }
}
