using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT;

namespace Vantus.App;

public static class WindowHelpers
{
    public static void SetTitleBar(Window window, UIElement titleBar)
    {
        var appWindow = GetAppWindow(window);
        if (appWindow != null)
        {
            // Note: Actual implementation for drag rectangles usually needs scaling and element position
            // For now, using a simplified version
            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        }
    }

    public static AppWindow GetAppWindow(Window window)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        return AppWindow.GetFromWindowId(windowId);
    }
}
