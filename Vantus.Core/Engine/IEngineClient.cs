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

    Task<IndexStats> GetStatsAsync();

    Task<IEnumerable<Tag>> GetTagsAsync();
    Task AddTagAsync(Tag tag);
    Task DeleteTagAsync(string name);

    Task<IEnumerable<Partner>> GetPartnersAsync();
    Task AddPartnerAsync(Partner partner);

    Task<IEnumerable<Rule>> GetRulesAsync();
    Task AddRuleAsync(Rule rule);
    Task DeleteRuleAsync(long id);
}

public class StubEngineClient : IEngineClient
{
    public Task<string> GetIndexStatusAsync() => Task.FromResult("Idle");
    public Task<IEnumerable<SearchResult>> SearchAsync(string query) => Task.FromResult(Enumerable.Empty<SearchResult>());
    public Task PauseIndexingAsync() => Task.CompletedTask;
    public Task ResumeIndexingAsync() => Task.CompletedTask;
    public Task RequestRebuildIndexAsync() => Task.CompletedTask;

    public Task<IndexStats> GetStatsAsync() => Task.FromResult(new IndexStats { FilesIndexed = 123, TotalTags = 10, TotalPartners = 5, Status = "Idle" });

    public Task<IEnumerable<Tag>> GetTagsAsync() => Task.FromResult<IEnumerable<Tag>>(new List<Tag>{ new Tag { Name="Test", Type="user"} });
    public Task AddTagAsync(Tag tag) => Task.CompletedTask;
    public Task DeleteTagAsync(string name) => Task.CompletedTask;

    public Task<IEnumerable<Partner>> GetPartnersAsync() => Task.FromResult<IEnumerable<Partner>>(new List<Partner>{ new Partner { Name="Acme" }});
    public Task AddPartnerAsync(Partner partner) => Task.CompletedTask;

    public Task<IEnumerable<Rule>> GetRulesAsync() => Task.FromResult<IEnumerable<Rule>>(new List<Rule>{ new Rule { Name="Test Rule" }});
    public Task AddRuleAsync(Rule rule) => Task.CompletedTask;
    public Task DeleteRuleAsync(long id) => Task.CompletedTask;
}
