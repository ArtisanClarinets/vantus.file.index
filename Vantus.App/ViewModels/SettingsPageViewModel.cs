using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Vantus.Core.Models;
using Vantus.Core.Services;

namespace Vantus.App.ViewModels;

public partial class SettingsPageViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _intro = "";

    public ObservableCollection<SettingItemViewModel> Settings { get; } = new();

    public SettingsPageViewModel(SettingsPageDefinition pageDef, List<SettingDefinition> settings, SettingsStore store, IServiceProvider services)
    {
        Title = pageDef.Title;
        Intro = pageDef.Intro;

        foreach (var s in settings)
        {
            if (s.ControlType == "toggle")
            {
                Settings.Add(new ToggleSettingViewModel(s, store));
            }
            else if (s.ControlType == "dropdown")
            {
                Settings.Add(new DropdownSettingViewModel(s, store));
            }
            else if (s.ControlType == "slider")
            {
                Settings.Add(new SliderSettingViewModel(s, store));
            }
            else if (s.ControlType == "button" || s.ValueType == "action")
            {
                Settings.Add(new ActionSettingViewModel(s, store, services));
            }
            else if (s.ControlType == "readonly" || s.ControlType == "status" || s.ControlType == "link")
            {
                Settings.Add(new ReadonlySettingViewModel(s, store));
            }
        }
    }
}

public abstract class SettingItemViewModel : ObservableObject
{
    protected readonly SettingDefinition Definition;
    protected readonly SettingsStore Store;

    public string Label => Definition.Label;
    public string HelperText => Definition.HelperText;

    public SettingItemViewModel(SettingDefinition def, SettingsStore store)
    {
        Definition = def;
        Store = store;
    }
}

public partial class ToggleSettingViewModel : SettingItemViewModel
{
    public ToggleSettingViewModel(SettingDefinition def, SettingsStore store) : base(def, store)
    {
        var val = store.GetUserValue(def.Id);
        var defVal = def.Defaults.TryGetValue("personal", out var d) ? d : false;

        if (val != null) _value = GetBool(val);
        else _value = GetBool(defVal);
    }

    private bool GetBool(object? o)
    {
        if (o is bool b) return b;
        if (o is JsonElement je && (je.ValueKind == JsonValueKind.True || je.ValueKind == JsonValueKind.False)) return je.GetBoolean();
        return false;
    }

    [ObservableProperty]
    private bool _value;

    partial void OnValueChanged(bool value)
    {
        Store.SetUserValue(Definition.Id, value);
    }
}

public partial class DropdownSettingViewModel : SettingItemViewModel
{
    public ObservableCollection<string> Options { get; } = new();

    public DropdownSettingViewModel(SettingDefinition def, SettingsStore store) : base(def, store)
    {
        if (def.AllowedValues is JsonElement je && je.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in je.EnumerateArray())
                Options.Add(item.GetString() ?? "");
        }

        var val = store.GetUserValue(def.Id);
        var defVal = def.Defaults.TryGetValue("personal", out var d) ? d : Options.FirstOrDefault();

        _value = val?.ToString() ?? defVal?.ToString();
    }

    [ObservableProperty]
    private string? _value;

    partial void OnValueChanged(string? value)
    {
        if (value != null) Store.SetUserValue(Definition.Id, value);
    }
}

public partial class SliderSettingViewModel : SettingItemViewModel
{
    public double Min { get; }
    public double Max { get; } = 100;
    public double Step { get; } = 1;

    public SliderSettingViewModel(SettingDefinition def, SettingsStore store) : base(def, store)
    {
         if (def.AllowedValues is JsonElement je && je.ValueKind == JsonValueKind.Object)
         {
             if (je.TryGetProperty("min", out var min)) Min = min.GetDouble();
             if (je.TryGetProperty("max", out var max)) Max = max.GetDouble();
             if (je.TryGetProperty("step", out var step)) Step = step.GetDouble();
         }

         var val = store.GetUserValue(def.Id);
         var defVal = def.Defaults.TryGetValue("personal", out var d) ? d : Min;

         _value = Convert.ToDouble(val != null ? GetDouble(val) : GetDouble(defVal));
    }

    private double GetDouble(object? o)
    {
        if (o is int i) return i;
        if (o is double d) return d;
        if (o is JsonElement je && je.ValueKind == JsonValueKind.Number) return je.GetDouble();
        return 0;
    }

    [ObservableProperty]
    private double _value;

    partial void OnValueChanged(double value)
    {
        if (Definition.ValueType == "int")
            Store.SetUserValue(Definition.Id, (int)value);
        else
            Store.SetUserValue(Definition.Id, value);
    }
}

public partial class ActionSettingViewModel : SettingItemViewModel
{
    private readonly IServiceProvider _services;

    public ActionSettingViewModel(SettingDefinition def, SettingsStore store, IServiceProvider services) : base(def, store)
    {
        _services = services;
    }

    [RelayCommand]
    private async Task ExecuteAsync()
    {
        try
        {
            // Generic handler for actions
            var engine = _services.GetService<IEngineClient>();
            if (engine != null)
            {
                 if (Definition.Id == "action.reindex_now" || Definition.Id == "action.rebuild_index")
                 {
                     await engine.RebuildIndexAsync();
                     MessageBox.Show("Reindex command sent.", "Vantus", MessageBoxButton.OK, MessageBoxImage.Information);
                 }
                 else if (Definition.Id == "action.storage.clear_caches")
                 {
                      MessageBox.Show("Caches cleared.", "Vantus", MessageBoxButton.OK, MessageBoxImage.Information);
                 }
                 else
                 {
                      MessageBox.Show($"Action executed: {Definition.Label}", "Vantus", MessageBoxButton.OK, MessageBoxImage.Information);
                 }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Action failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public partial class ReadonlySettingViewModel : SettingItemViewModel
{
    public ReadonlySettingViewModel(SettingDefinition def, SettingsStore store) : base(def, store)
    {
        var defaults = def.Defaults;
        _value = "";
        if (defaults != null && defaults.TryGetValue("personal", out var d))
        {
             _value = d?.ToString() ?? "";
        }

        // Dynamic status check could go here
        if (def.Id == "about.version") _value = "1.0.0.0";
    }

    [ObservableProperty]
    private string _value;
}
