using System.Windows.Controls;
using Vantus.App.ViewModels;

namespace Vantus.App.Views;

public partial class SearchPage : Page
{
    public SearchPage(SearchViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public SearchPage() : this(App.GetService<SearchViewModel>())
    {
    }
}
