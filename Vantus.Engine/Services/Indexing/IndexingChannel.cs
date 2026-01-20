using System.Threading.Channels;

namespace Vantus.Engine.Services.Indexing;

public class IndexingChannel
{
    private readonly Channel<FileChangeEvent> _channel;

    public IndexingChannel()
    {
        // Unbounded for now, but in production we might want bounded to apply backpressure
        _channel = Channel.CreateUnbounded<FileChangeEvent>();
    }

    public bool Write(FileChangeEvent fileEvent)
    {
        return _channel.Writer.TryWrite(fileEvent);
    }

    public IAsyncEnumerable<FileChangeEvent> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}
