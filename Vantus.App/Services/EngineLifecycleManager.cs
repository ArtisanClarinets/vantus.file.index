using System.Diagnostics;
using System.IO;

namespace Vantus.App.Services;

public class EngineLifecycleManager
{
    private Process? _engineProcess;

    public async Task StartEngineAsync()
    {
        var appDir = AppContext.BaseDirectory;
        var engineExe = "Vantus.Engine.exe";
        var enginePath = Path.Combine(appDir, engineExe);

        // Dev environment fallback
        if (!File.Exists(enginePath))
        {
            // Try looking in relative path for dev
             var devPath = Path.GetFullPath(Path.Combine(appDir, "..", "..", "..", "..", "Vantus.Engine", "bin", "Debug", "net8.0", engineExe));
             if (File.Exists(devPath))
             {
                 enginePath = devPath;
             }
        }

        if (File.Exists(enginePath))
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = enginePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(enginePath)
                };

                _engineProcess = Process.Start(psi);

                // Give it a moment to start up
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start engine: {ex.Message}");
            }
        }
        else
        {
            Debug.WriteLine("Engine executable not found.");
        }
    }

    public void StopEngine()
    {
        try
        {
            if (_engineProcess != null && !_engineProcess.HasExited)
            {
                _engineProcess.Kill();
                _engineProcess.Dispose();
                _engineProcess = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to stop engine: {ex.Message}");
        }
    }
}
