using Microsoft.Extensions.Logging;

namespace Vantus.Tests.Engine;

public class TestLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger();
    }

    public void Dispose()
    {
    }

    private class TestLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
            if (exception != null)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
