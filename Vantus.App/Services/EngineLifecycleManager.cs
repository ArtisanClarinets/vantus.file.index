using System.Diagnostics;
using System.IO;
using Vantus.Core.Services;

namespace Vantus.App.Services;

public class EngineLifecycleManager
{
    private Process? _engineProcess;
    private readonly string _enginePath;
    private readonly IEngineClient _client;

    public EngineLifecycleManager(IEngineClient client)
    {
        _client = client;
        // Assume Engine is in "Engine" subdirectory for production
        // Or check a few locations
        var productionPath = Path.Combine(AppContext.BaseDirectory, "Engine", "Vantus.Engine.exe");

        // Development fallback: try to find it in the build output of Engine project if we are in dev
        // but typically in dev we might run engine separately.
        // For "Production-Ready", we prioritize the bundled path.

        _enginePath = productionPath;
    }

    public void StartEngine()
    {
        if (!File.Exists(_enginePath))
        {
            // Log warning?
            return;
        }

        var existing = Process.GetProcessesByName("Vantus.Engine");
        if (existing.Length > 0)
        {
            // Already running, maybe attach to it or just ignore
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _enginePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(_enginePath)
        };

        try
        {
            _engineProcess = Process.Start(startInfo);
        }
        catch (Exception)
        {
            // In a real app, log this via ILogger
        }
    }

    public void StopEngine()
    {
        // Try graceful shutdown via RPC first
        try
        {
            // We use Wait() because this might be called from OnExit where async is tricky
            _client.ShutdownAsync().Wait(2000);
        }
        catch { }

        // Kill the process we started if it's still running
        if (_engineProcess != null && !_engineProcess.HasExited)
        {
            try
            {
                _engineProcess.Kill();
                _engineProcess.WaitForExit(1000);
            }
            catch { }
        }
    }
}
