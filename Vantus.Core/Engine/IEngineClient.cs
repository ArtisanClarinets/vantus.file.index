using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vantus.Core.Models;

namespace Vantus.Core.Engine;

public interface IEngineClient
{
    Task<string> GetIndexStatusAsync();
    Task<IEnumerable<SearchResult>> SearchAsync(string query);
    Task PauseIndexingAsync();
    Task ResumeIndexingAsync();
    Task RequestRebuildIndexAsync();
}

public class StubEngineClient : IEngineClient
{
    public Task<string> GetIndexStatusAsync() => Task.FromResult("Idle");
    public Task<IEnumerable<SearchResult>> SearchAsync(string query) => Task.FromResult(Enumerable.Empty<SearchResult>());
    public Task PauseIndexingAsync() => Task.CompletedTask;
    public Task ResumeIndexingAsync() => Task.CompletedTask;
    public Task RequestRebuildIndexAsync() => Task.CompletedTask;
}
