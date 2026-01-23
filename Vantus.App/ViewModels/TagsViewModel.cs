using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Vantus.Core.Engine;
using Vantus.Core.Models;

namespace Vantus.App.ViewModels;

public partial class TagsViewModel : ObservableObject
{
    private readonly IEngineClient _engineClient;

    [ObservableProperty]
    private ObservableCollection<Tag> _tags = new();

    [ObservableProperty]
    private string _newTagName = "";

    public TagsViewModel(IEngineClient engineClient)
    {
        _engineClient = engineClient;
        _ = LoadTagsAsync();
    }

    public TagsViewModel() : this(new StubEngineClient()) { }

    [RelayCommand]
    private async Task LoadTagsAsync()
    {
        var tags = await _engineClient.GetTagsAsync();
        Tags.Clear();
        foreach (var t in tags) Tags.Add(t);
    }

    [RelayCommand]
    private async Task AddTagAsync()
    {
         if (string.IsNullOrWhiteSpace(NewTagName)) return;
         await _engineClient.AddTagAsync(new Tag { Name = NewTagName, Type = "user" });
         NewTagName = "";
         await LoadTagsAsync();
    }

    [RelayCommand]
    private async Task DeleteTagAsync(Tag tag)
    {
        if (tag == null) return;
        await _engineClient.DeleteTagAsync(tag.Name);
        await LoadTagsAsync();
    }
}
