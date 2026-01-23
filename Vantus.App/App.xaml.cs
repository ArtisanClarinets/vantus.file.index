using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vantus.Core.Interfaces;
using Vantus.Core.Services;
using Vantus.Core.Engine;
using Vantus.App.ViewModels;
using Vantus.App.Services;

namespace Vantus.App;

public partial class App : Application
{
    public IHost Host { get; }
    private System.Diagnostics.Process? _engineProcess;

    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }
        return service;
    }

    public App()
    {
        Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                // Core Services
                services.AddSingleton<ISettingsRegistry, SettingsRegistry>();
                services.AddSingleton<ISettingsStore, SettingsStore>();
                services.AddSingleton<IPresetManager, PresetManager>();
                services.AddSingleton<IPolicyEngine, PolicyEngine>();
                services.AddSingleton<IEngineClient, NamedPipeEngineClient>();
                services.AddSingleton<IImportExportService, ImportExportService>();

                // App Services
                services.AddSingleton<ThemeService>();
                services.AddSingleton<LocalizationService>();
                services.AddSingleton<NotificationService>();
                services.AddSingleton<ErrorHandlingService>();
                services.AddSingleton<EngineLifecycleManager>();

                // ViewModels
                services.AddTransient<ShellViewModel>();
                services.AddTransient<SettingsPageViewModel>();
                services.AddTransient<ModesPageViewModel>();
                services.AddTransient<ImportExportPageViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<SearchViewModel>();
                services.AddTransient<RulesEditorViewModel>();
                services.AddTransient<TagsViewModel>();
                services.AddTransient<PartnersViewModel>();
                
                // Windows/Pages
                services.AddSingleton<MainWindow>();
            }).
            Build();
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        await Host.StartAsync();

        var lifecycleManager = GetService<EngineLifecycleManager>();
        await lifecycleManager.StartEngineAsync();

        // Start the Engine Process
        try 
        {
            var enginePath = Path.Combine(AppContext.BaseDirectory, "Engine", "Vantus.Engine.exe");
            if (!File.Exists(enginePath))
            {
                enginePath = Path.Combine(AppContext.BaseDirectory, "Vantus.Engine.exe");
            }

            if (File.Exists(enginePath))
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = enginePath,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                _engineProcess = System.Diagnostics.Process.Start(psi);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Engine not found at {enginePath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to start engine: {ex}");
        }

        var errorHandler = GetService<ErrorHandlingService>();
        errorHandler.Initialize();

        // Init Registry & Policy & Store
        try
        {
            var registry = GetService<ISettingsRegistry>();
            await registry.InitializeAsync();

            var policy = GetService<IPolicyEngine>();
            await policy.InitializeAsync();

            var store = GetService<ISettingsStore>();
            await store.LoadAsync();
            
            // Apply Theme
            var themeService = GetService<ThemeService>();
            themeService.Initialize();
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Initialization failed: {ex}");
        }

        var mainWindow = GetService<MainWindow>();
        
        // Set window icon from assets
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Resources", "app.ico");
        if (File.Exists(iconPath))
        {
            try {
                mainWindow.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath));
            } catch { /* Ignore icon error */ }
        }
        
        mainWindow.Show();
        
        mainWindow.Closed += (s, args) =>
        {
            StopEngine();
        };
    }


    protected override async void OnExit(ExitEventArgs e)
    {
        await Host.StopAsync();
        base.OnExit(e);
    }
    private void StopEngine()
    {
        try
        {
            if (_engineProcess != null && !_engineProcess.HasExited)
            {
                _engineProcess.Kill();
                _engineProcess.Dispose();
            }
        }
        catch { }
    }
}
