using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vantus.Core.Models;

namespace Vantus.Core.Services;

public interface ITelemetryService
{
    Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null);
    Task TrackExceptionAsync(Exception exception, Dictionary<string, string>? properties = null);
    Task TrackMetricAsync(string metricName, double value, Dictionary<string, string>? properties = null);
    Task TrackDependencyAsync(string dependencyName, string type, string data, bool success, TimeSpan duration);
}

public interface IValidationService
{
    ValidationResult ValidateSettingValue(SettingDefinition definition, object? value);
    ValidationResult ValidateSettingsSnapshot(SettingsSnapshot snapshot);
}

public record ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public static ValidationResult Valid() => new() { IsValid = true };
    public static ValidationResult Invalid(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
}

public interface ISettingsMigrationService
{
    bool NeedsMigration(string currentVersion);
    Task<SettingsSnapshot> MigrateAsync(SettingsSnapshot snapshot, string targetVersion);
}

public interface IFileLockProvider
{
    IDisposable AcquireLock(string filePath, TimeSpan timeout);
    bool IsFileLocked(string filePath);
}

public interface IRetryPolicy
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
    Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}

public class NullTelemetryService : ITelemetryService
{
    public Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null) => Task.CompletedTask;
    public Task TrackExceptionAsync(Exception exception, Dictionary<string, string>? properties = null) => Task.CompletedTask;
    public Task TrackMetricAsync(string metricName, double value, Dictionary<string, string>? properties = null) => Task.CompletedTask;
    public Task TrackDependencyAsync(string dependencyName, string type, string data, bool success, TimeSpan duration) => Task.CompletedTask;
}

public class TelemetryService : ITelemetryService
{
    private readonly string _logPath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public TelemetryService(string logPath)
    {
        _logPath = logPath;
        Directory.CreateDirectory(_logPath);
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null)
    {
        var evt = new TelemetryEvent
        {
            Timestamp = DateTime.UtcNow,
            EventName = eventName,
            Properties = properties ?? new Dictionary<string, string>()
        };

        await WriteToLogAsync("event", evt);
    }

    public async Task TrackExceptionAsync(Exception exception, Dictionary<string, string>? properties = null)
    {
        var ex = new ExceptionTelemetry
        {
            Timestamp = DateTime.UtcNow,
            ExceptionType = exception.GetType().Name,
            Message = exception.Message,
            StackTrace = exception.StackTrace,
            InnerException = exception.InnerException?.Message,
            Properties = properties ?? new Dictionary<string, string>()
        };

        await WriteToLogAsync("exception", ex);
    }

    public async Task TrackMetricAsync(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        var metric = new MetricTelemetry
        {
            Timestamp = DateTime.UtcNow,
            MetricName = metricName,
            Value = value,
            Properties = properties ?? new Dictionary<string, string>()
        };

        await WriteToLogAsync("metric", metric);
    }

    public async Task TrackDependencyAsync(string dependencyName, string type, string data, bool success, TimeSpan duration)
    {
        var dep = new DependencyTelemetry
        {
            Timestamp = DateTime.UtcNow,
            DependencyName = dependencyName,
            Type = type,
            Data = data,
            Success = success,
            DurationMs = duration.TotalMilliseconds
        };

        await WriteToLogAsync("dependency", dep);
    }

    private async Task WriteToLogAsync<T>(string type, T data)
    {
        await _semaphore.WaitAsync();
        try
        {
            var fileName = $"telemetry_{DateTime.UtcNow:yyyyMMdd}.log";
            var filePath = Path.Combine(_logPath, fileName);

            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var line = $"{DateTime.UtcNow:HH:mm:ss.fff}\t{type}\t{json}";

            await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
        }
        catch
        {
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

internal class TelemetryEvent
{
    public DateTime Timestamp { get; set; }
    public string EventName { get; set; } = string.Empty;
    public Dictionary<string, string> Properties { get; set; } = new();
}

internal class ExceptionTelemetry
{
    public DateTime Timestamp { get; set; }
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
}

internal class MetricTelemetry
{
    public DateTime Timestamp { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
}

internal class DependencyTelemetry
{
    public DateTime Timestamp { get; set; }
    public string DependencyName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public bool Success { get; set; }
    public double DurationMs { get; set; }
}

public class SettingsMigrationService : ISettingsMigrationService
{
    private readonly ITelemetryService _telemetry;

    public SettingsMigrationService(ITelemetryService telemetry)
    {
        _telemetry = telemetry;
    }

    public bool NeedsMigration(string currentVersion)
    {
        if (string.IsNullOrEmpty(currentVersion))
            return true;

        var current = ParseVersion(currentVersion);
        var target = ParseVersion("1.0");

        return current < target;
    }

    public async Task<SettingsSnapshot> MigrateAsync(SettingsSnapshot snapshot, string targetVersion)
    {
        var originalVersion = snapshot.Version ?? "0.0";

        try
        {
            await _telemetry.TrackEventAsync("SettingsMigrationStarted", new Dictionary<string, string>
            {
                { "FromVersion", originalVersion },
                { "ToVersion", targetVersion }
            });

            var migrated = await ApplyMigrations(snapshot, originalVersion, targetVersion);

            migrated.Version = targetVersion;
            migrated.SchemaVersion = targetVersion;

            await _telemetry.TrackEventAsync("SettingsMigrationCompleted", new Dictionary<string, string>
            {
                { "FromVersion", originalVersion },
                { "ToVersion", targetVersion }
            });

            return migrated;
        }
        catch (Exception ex)
        {
            await _telemetry.TrackExceptionAsync(ex, new Dictionary<string, string>
            {
                { "MigrationStep", "MigrateAsync" },
                { "FromVersion", originalVersion },
                { "ToVersion", targetVersion }
            });
            throw;
        }
    }

    private static int ParseVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            return 0;

        var parts = version.Split('.');
        if (parts.Length < 2)
            return 0;

        if (int.TryParse(parts[0], out var major) && int.TryParse(parts[1], out var minor))
        {
            return major * 1000 + minor;
        }

        return 0;
    }

    private static async Task<SettingsSnapshot> ApplyMigrations(SettingsSnapshot snapshot, string fromVersion, string toVersion)
    {
        if (string.IsNullOrEmpty(fromVersion) || fromVersion == "0.0")
        {
            await MigrateFrom0To1(snapshot);
        }

        return snapshot;
    }

    private static Task MigrateFrom0To1(SettingsSnapshot snapshot)
    {
        snapshot.ActivePreset ??= "personal";
        snapshot.CreatedAt = snapshot.CreatedAt == default ? DateTime.UtcNow : snapshot.CreatedAt;
        snapshot.SchemaVersion ??= "1.0";

        if (snapshot.GlobalSettings == null)
            snapshot.GlobalSettings = new Dictionary<string, object>();

        if (snapshot.WorkspaceSettings == null)
            snapshot.WorkspaceSettings = new Dictionary<string, Dictionary<string, object>>();

        if (snapshot.LocationSettings == null)
            snapshot.LocationSettings = new Dictionary<string, Dictionary<string, object>>();

        return Task.CompletedTask;
    }
}

public class FileLockProvider : IFileLockProvider
{
    private readonly Dictionary<string, FileStream> _locks = new();
    private readonly object _lockObject = new();

    public IDisposable AcquireLock(string filePath, TimeSpan timeout)
    {
        var startTime = DateTime.UtcNow;

        lock (_lockObject)
        {
            while ((DateTime.UtcNow - startTime) < timeout)
            {
                if (!_locks.ContainsKey(filePath))
                {
                    try
                    {
                        var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                        _locks[filePath] = stream;
                        return new FileLockReleaser(this, filePath);
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(50);
                    }
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }

        throw new TimeoutException($"Could not acquire lock for file {filePath} within {timeout.TotalMilliseconds}ms");
    }

    public bool IsFileLocked(string filePath)
    {
        lock (_lockObject)
        {
            return _locks.ContainsKey(filePath);
        }
    }

    private void ReleaseLock(string filePath)
    {
        lock (_lockObject)
        {
            if (_locks.TryGetValue(filePath, out var stream))
            {
                stream.Dispose();
                _locks.Remove(filePath);
            }
        }
    }

    private class FileLockReleaser : IDisposable
    {
        private readonly FileLockProvider _provider;
        private readonly string _filePath;
        private bool _disposed;

        public FileLockReleaser(FileLockProvider provider, string filePath)
        {
            _provider = provider;
            _filePath = filePath;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _provider.ReleaseLock(_filePath);
                _disposed = true;
            }
        }
    }
}

public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly TimeSpan _maxDelay;
    private readonly ITelemetryService _telemetry;

    public ExponentialBackoffRetryPolicy(
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        TimeSpan? maxDelay = null,
        ITelemetryService? telemetry = null)
    {
        _maxRetries = maxRetries;
        _initialDelay = initialDelay ?? TimeSpan.FromMilliseconds(100);
        _maxDelay = maxDelay ?? TimeSpan.FromSeconds(30);
        _telemetry = telemetry ?? new NullTelemetryService();
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        var delay = _initialDelay;

        while (attempt <= _maxRetries)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsRetryable(ex) && attempt < _maxRetries)
            {
                attempt++;
                await _telemetry.TrackEventAsync("RetryAttempt", new Dictionary<string, string>
                {
                    { "Attempt", attempt.ToString() },
                    { "Delay", delay.TotalMilliseconds.ToString() },
                    { "ExceptionType", ex.GetType().Name }
                });

                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, _maxDelay.TotalMilliseconds));
            }
        }

        return await operation();
    }

    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async () =>
        {
            await operation();
            return true;
        }, cancellationToken);
    }

    private static bool IsRetryable(Exception ex)
    {
        return ex is IOException ||
               ex is TimeoutException ||
               (ex is JsonException && ex.Message.Contains("could not be read"));
    }
}
