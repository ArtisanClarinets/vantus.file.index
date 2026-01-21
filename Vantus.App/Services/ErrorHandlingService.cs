using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;
using System;

namespace Vantus.App.Services;

public class ErrorHandlingService
{
    public void Initialize()
    {
        if (Application.Current != null)
        {
            Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // In production, log this to a file
        System.Diagnostics.Debug.WriteLine($"[UI Error] {e.Exception}");
        MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true; 
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // In production, log this to a file
        System.Diagnostics.Debug.WriteLine($"[Critical Error] {e.ExceptionObject}");
        MessageBox.Show($"A critical error occurred: {(e.ExceptionObject as Exception)?.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        // In production, log this to a file
        System.Diagnostics.Debug.WriteLine($"[Task Error] {e.Exception}");
        e.SetObserved();
    }
}
