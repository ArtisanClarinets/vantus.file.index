using System.Windows.Controls;
using Vantus.App.ViewModels;

namespace Vantus.App.Views;

public partial class RulesEditor : Page
{
    public RulesEditor(RulesEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
