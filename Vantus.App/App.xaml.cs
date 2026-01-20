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
                
                // Windows/Pages
                services.AddSingleton<MainWindow>();
            }).
            Build();
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        await Host.StartAsync();

        var errorHandler = GetService<ErrorHandlingService>();
        errorHandler.Initialize();

        // Start Engine
        var lifecycleManager = GetService<EngineLifecycleManager>();
        await lifecycleManager.StartEngineAsync();

        try
        {
            // Init Registry & Policy & Store
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
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        var lifecycleManager = GetService<EngineLifecycleManager>();
        lifecycleManager.StopEngine();

        await Host.StopAsync();
        base.OnExit(e);
    }
}
