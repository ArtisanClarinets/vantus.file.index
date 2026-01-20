using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Vantus.App.Services;
using Vantus.App.ViewModels;
using Vantus.Core.Models;
using Vantus.Core.Services;

namespace Vantus.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ITelemetryService>(sp =>
        {
            var dataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Vantus");
            Directory.CreateDirectory(dataPath);
            return new TelemetryService(dataPath);
        });

        services.AddSingleton<IFileLockProvider, FileLockProvider>();

        services.AddSingleton<IRetryPolicy>(sp =>
        {
            var telemetry = sp.GetRequiredService<ITelemetryService>();
            return new ExponentialBackoffRetryPolicy(
                maxRetries: 3,
                initialDelay: TimeSpan.FromMilliseconds(100),
                maxDelay: TimeSpan.FromSeconds(30),
                telemetry: telemetry);
        });

        services.AddSingleton<IValidationService, ValidationService>();

        services.AddSingleton<ISettingsMigrationService>(sp =>
        {
            var telemetry = sp.GetRequiredService<ITelemetryService>();
            return new SettingsMigrationService(telemetry);
        });

        services.AddSingleton(sp =>
        {
            var dataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Vantus");
            var telemetry = sp.GetRequiredService<ITelemetryService>();
            var fileLock = sp.GetRequiredService<IFileLockProvider>();
            var retryPolicy = sp.GetRequiredService<IRetryPolicy>();
            var validation = sp.GetRequiredService<IValidationService>();
            var migration = sp.GetRequiredService<ISettingsMigrationService>();
            return new SettingsStore(dataPath, fileLock, retryPolicy, validation, migration, telemetry);
        });

        services.AddSingleton(sp =>
        {
            var store = sp.GetRequiredService<SettingsStore>();
            return store.GetSchema();
        });

        services.AddSingleton(sp =>
        {
            var store = sp.GetRequiredService<SettingsStore>();
            var schema = sp.GetRequiredService<SettingsSchema>();
            var telemetry = sp.GetRequiredService<ITelemetryService>();
            return new PresetManager(store, schema, telemetry);
        });

        services.AddSingleton(sp =>
        {
            var dataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Vantus");
            var telemetry = sp.GetRequiredService<ITelemetryService>();
            var fileLock = sp.GetRequiredService<IFileLockProvider>();
            return new PolicyEngine(dataPath, telemetry, fileLock);
        });

        services.AddSingleton(sp =>
        {
            var store = sp.GetRequiredService<SettingsStore>();
            var schema = sp.GetRequiredService<SettingsSchema>();
            var telemetry = sp.GetRequiredService<ITelemetryService>();
            var retryPolicy = sp.GetRequiredService<IRetryPolicy>();
            return new ImportExportService(store, schema, telemetry, retryPolicy);
        });

        services.AddSingleton<IEngineClient, EngineClientStub>();

        services.AddSingleton<SettingsControlFactory>();
        services.AddSingleton<NavigationService>();

        Services = services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var telemetry = Services.GetRequiredService<ITelemetryService>();
        telemetry.TrackEventAsync("AppLaunched");

        try
        {
            var store = Services.GetRequiredService<SettingsStore>();
            store.InitializeAsync().Wait();

            var policyEngine = Services.GetRequiredService<PolicyEngine>();
            policyEngine.InitializeAsync().Wait();

            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
        catch (Exception ex)
        {
            telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
            {
                { "Operation", "OnLaunched" }
            });
            throw;
        }
    }
}
