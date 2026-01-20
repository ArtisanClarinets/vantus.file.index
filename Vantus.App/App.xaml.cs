using System.Windows;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Vantus.App.ViewModels;
using Vantus.Core.Services;

namespace Vantus.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public new static Window? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton(sp =>
        {
            var path = Path.Combine(AppContext.BaseDirectory, "settings_definitions.json");
            var json = File.Exists(path) ? File.ReadAllText(path) : "{}";
            return new SettingsSchema(json);
        });
        
        services.AddSingleton(sp =>
        {
             var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vantus");
             return new PolicyEngine(dataPath);
        });

        services.AddSingleton(sp =>
        {
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vantus");
            Directory.CreateDirectory(dataPath);
            var schema = sp.GetRequiredService<SettingsSchema>();
            var policyEngine = sp.GetRequiredService<PolicyEngine>();
            return new SettingsStore(dataPath, schema, policyEngine);
        });

        services.AddSingleton(sp =>
        {
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vantus");
            return new RuleService(dataPath);
        });

        services.AddSingleton<IEngineClient, Vantus.App.Services.GrpcEngineClient>();

        services.AddTransient<SearchViewModel>();
        services.AddTransient<Vantus.App.Views.SearchPage>();
        
        services.AddTransient<RulesEditorViewModel>();
        services.AddTransient<Vantus.App.Views.RulesEditor>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<Vantus.App.Views.DashboardPage>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        Services = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var store = Services.GetRequiredService<SettingsStore>();
            store.InitializeAsync().Wait();

            var policyEngine = Services.GetRequiredService<PolicyEngine>();
            policyEngine.InitializeAsync().Wait();

            var ruleService = Services.GetRequiredService<RuleService>();
            ruleService.InitializeAsync().Wait();

            MainWindow = Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup error: {ex.Message}", "Vantus Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}
