using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Vantus.App.Services;

public class NavigationService
{
    private Frame? _frame;

    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public void GoBack()
    {
        _frame?.GoBack();
    }

    public void NavigateTo(Type pageType, object? parameter = null)
    {
        _frame?.Navigate(pageType, parameter);
    }
}
