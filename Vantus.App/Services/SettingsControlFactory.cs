using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Vantus.Core.Models;
using Vantus.Core.Services;

namespace Vantus.App.Services;

public class SettingsControlFactory
{
    private readonly SettingsStore _settingsStore;
    private readonly PolicyEngine _policyEngine;
    private readonly ITelemetryService _telemetry;

    public SettingsControlFactory(SettingsStore settingsStore, PolicyEngine policyEngine, ITelemetryService? telemetry = null)
    {
        _settingsStore = settingsStore;
        _policyEngine = policyEngine;
        _telemetry = telemetry ?? new NullTelemetryService();
    }

    public FrameworkElement CreateControl(SettingDefinition definition, string scope = "global")
    {
        var lockState = _policyEngine.GetLockState(definition.Id);
        var currentValue = _settingsStore.GetValue<object>(definition.Id, scope);
        var effectiveValue = _policyEngine.GetEffectiveValue(definition.Id, currentValue);

        _telemetry.TrackEventAsync("ControlCreated", new Dictionary<string, string>
        {
            { "ControlType", definition.ControlType },
            { "SettingId", definition.Id },
            { "IsLocked", lockState.IsLocked.ToString() }
        });

        return definition.ControlType.ToLowerInvariant() switch
        {
            "toggle" => CreateToggleControl(definition, effectiveValue, lockState),
            "dropdown" => CreateDropdownControl(definition, effectiveValue, lockState),
            "slider" => CreateSliderControl(definition, effectiveValue, lockState),
            "multi_select" => CreateMultiSelectControl(definition, effectiveValue, lockState),
            "segmented" => CreateSegmentedControl(definition, effectiveValue, lockState),
            "button" => CreateButtonControl(definition, lockState),
            "list" => CreateListControl(definition, lockState),
            "token_list" => CreateTokenListControl(definition, lockState),
            "status" => CreateStatusControl(definition, effectiveValue),
            "read_only" => CreateReadOnlyControl(definition, effectiveValue),
            "link" => CreateLinkControl(definition),
            "time_range" => CreateTimeRangeControl(definition, effectiveValue, lockState),
            "editor" => CreateEditorControl(definition, lockState),
            "reorder_list" => CreateReorderListControl(definition, effectiveValue, lockState),
            _ => CreateUnknownControl(definition)
        };
    }

    private FrameworkElement CreateToggleControl(SettingDefinition definition, object? value, LockedSettingState lockState)
    {
        var toggle = new ToggleSwitch
        {
            IsOn = value is true,
            Header = definition.Label,
            IsEnabled = !lockState.IsLocked
        };

        if (lockState.IsLocked)
        {
            var lockIcon = new FontIcon
            {
                Glyph = "\uE72E",
                FontSize = 12,
                Margin = new Thickness(8, 0, 0, 0)
            };
            toggle.Header += " ";
        }

        toggle.Toggled += (s, e) =>
        {
            if (!lockState.IsLocked)
            {
                _settingsStore.SetValue(definition.Id, toggle.IsOn, definition.Scope);
            }
        };

        return WrapInContainer(toggle, definition, lockState);
    }

    private FrameworkElement CreateDropdownControl(SettingDefinition definition, object? value, LockedSettingState lockState)
    {
        var comboBox = new ComboBox
        {
            Header = definition.Label,
            IsEnabled = !lockState.IsLocked,
            Width = 300,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        if (definition.AllowedValues != null)
        {
            foreach (var val in definition.AllowedValues)
            {
                comboBox.Items.Add(val);
            }

            comboBox.SelectedItem = value?.ToString() ?? definition.AllowedValues.FirstOrDefault();
        }

        comboBox.SelectionChanged += (s, e) =>
        {
            if (!lockState.IsLocked)
            {
                _settingsStore.SetValue(definition.Id, comboBox.SelectedItem?.ToString(), definition.Scope);
            }
        };

        return WrapInContainer(comboBox, definition, lockState);
    }

    private FrameworkElement CreateSliderControl(SettingDefinition definition, object? value, LockedSettingState lockState)
    {
        var slider = new Slider
        {
            Header = definition.Label,
            IsEnabled = !lockState.IsLocked,
            Minimum = definition.MinValue ?? 0,
            Maximum = definition.MaxValue ?? 100,
            StepFrequency = definition.Step ?? 1,
            Value = Convert.ToDouble(value ?? definition.MinValue ?? 0)
        };

        slider.ValueChanged += (s, e) =>
        {
            if (!lockState.IsLocked)
            {
                _settingsStore.SetValue(definition.Id, slider.Value, definition.Scope);
            }
        };

        return WrapInContainer(slider, definition, lockState);
    }

    private FrameworkElement CreateMultiSelectControl(SettingDefinition definition, object? value, LockedSettingState lockState)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var header = new TextBlock
        {
            Text = definition.Label,
            Style = (Style)Application.Current.Resources["SettingsItemHeaderStyle"]
        };
        panel.Children.Add(header);

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 4, 0, 8)
        };
        panel.Children.Add(description);

        if (definition.AllowedValues != null)
        {
            var currentValues = value is List<object> list
                ? list.Select(v => v.ToString()).ToHashSet()
                : new HashSet<string>();

            foreach (var val in definition.AllowedValues)
            {
                var checkBox = new CheckBox
                {
                    Content = val,
                    IsChecked = currentValues.Contains(val),
                    IsEnabled = !lockState.IsLocked,
                    Margin = new Thickness(0, 4, 0, 4)
                };

                checkBox.Checked += (s, e) => UpdateMultiSelect(definition, checkBox.Content?.ToString(), true, currentValues);
                checkBox.Unchecked += (s, e) => UpdateMultiSelect(definition, checkBox.Content?.ToString(), false, currentValues);

                panel.Children.Add(checkBox);
            }
        }

        return panel;
    }

    private void UpdateMultiSelect(SettingDefinition definition, string? value, bool added, HashSet<string> currentValues)
    {
        if (value == null) return;

        if (added)
            currentValues.Add(value);
        else
            currentValues.Remove(value);

        _settingsStore.SetValue(definition.Id, currentValues.ToList(), definition.Scope);
    }

    private FrameworkElement CreateSegmentedControl(SettingDefinition definition, object? value, LockedSettingState lockState)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var header = new TextBlock
        {
            Text = definition.Label,
            Style = (Style)Application.Current.Resources["SettingsItemHeaderStyle"]
        };
        panel.Children.Add(header);

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 4, 0, 8)
        };
        panel.Children.Add(description);

        var radioButtons = new RadioButtons
        {
            IsEnabled = !lockState.IsLocked
        };

        if (definition.AllowedValues != null)
        {
            foreach (var val in definition.AllowedValues)
            {
                radioButtons.Items.Add(val);
            }

            radioButtons.SelectedItem = value?.ToString();
        }

        radioButtons.SelectionChanged += (s, e) =>
        {
            if (!lockState.IsLocked && radioButtons.SelectedItem != null)
            {
                _settingsStore.SetValue(definition.Id, radioButtons.SelectedItem?.ToString(), definition.Scope);
            }
        };

        panel.Children.Add(radioButtons);
        return panel;
    }

    private FrameworkElement CreateButtonControl(SettingDefinition definition, LockedSettingState lockState)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var button = new Button
        {
            Content = definition.ButtonText ?? definition.Label,
            IsEnabled = !lockState.IsLocked
        };

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 8, 0, 0)
        };

        panel.Children.Add(button);
        panel.Children.Add(description);

        return panel;
    }

    private FrameworkElement CreateListControl(SettingDefinition definition, LockedSettingState lockState)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var header = new TextBlock
        {
            Text = definition.Label,
            Style = (Style)Application.Current.Resources["SettingsItemHeaderStyle"]
        };
        panel.Children.Add(header);

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 4, 0, 8)
        };
        panel.Children.Add(description);

        var listView = new ListView
        {
            IsEnabled = !lockState.IsLocked,
            Height = 150
        };

        panel.Children.Add(listView);

        return panel;
    }

    private FrameworkElement CreateTokenListControl(SettingDefinition definition, LockedSettingState lockState)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var header = new TextBlock
        {
            Text = definition.Label,
            Style = (Style)Application.Current.Resources["SettingsItemHeaderStyle"]
        };
        panel.Children.Add(header);

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 4, 0, 8)
        };
        panel.Children.Add(description);

        var textBox = new TextBox
        {
            IsEnabled = !lockState.IsLocked,
            PlaceholderText = "Enter patterns (comma-separated)"
        };

        panel.Children.Add(textBox);

        return panel;
    }

    private FrameworkElement CreateStatusControl(SettingDefinition definition, object? value)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var header = new TextBlock
        {
            Text = definition.Label,
            Style = (Style)Application.Current.Resources["SettingsItemHeaderStyle"]
        };
        panel.Children.Add(header);

        var status = new InfoBar
        {
            Title = value?.ToString() ?? "Active",
            Severity = InfoBarSeverity.Success,
            IsOpen = true
        };

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 4, 0, 0)
        };
        panel.Children.Add(description);
        panel.Children.Add(status);

        return panel;
    }

    private FrameworkElement CreateReadOnlyControl(SettingDefinition definition, object? value)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var header = new TextBlock
        {
            Text = definition.Label,
            Style = (Style)Application.Current.Resources["SettingsItemHeaderStyle"]
        };
        panel.Children.Add(header);

        var valueText = new TextBlock
        {
            Text = value?.ToString() ?? string.Empty,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 4, 0, 0)
        };
        panel.Children.Add(valueText);

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 4, 0, 0)
        };
        panel.Children.Add(description);

        return panel;
    }

    private FrameworkElement CreateLinkControl(SettingDefinition definition)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var link = new HyperlinkButton
        {
            Content = definition.Label
        };

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 8, 0, 0)
        };
        panel.Children.Add(link);
        panel.Children.Add(description);

        return panel;
    }

    private FrameworkElement CreateTimeRangeControl(SettingDefinition definition, object? value, LockedSettingState lockState)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var header = new TextBlock
        {
            Text = definition.Label,
            Style = (Style)Application.Current.Resources["SettingsItemHeaderStyle"]
        };
        panel.Children.Add(header);

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 4, 0, 8)
        };
        panel.Children.Add(description);

        return panel;
    }

    private FrameworkElement CreateEditorControl(SettingDefinition definition, LockedSettingState lockState)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var header = new TextBlock
        {
            Text = definition.Label,
            Style = (Style)Application.Current.Resources["SettingsItemHeaderStyle"]
        };
        panel.Children.Add(header);

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 4, 0, 8)
        };
        panel.Children.Add(description);

        var editor = new TextBox
        {
            IsEnabled = !lockState.IsLocked,
            AcceptsReturn = true,
            MinHeight = 100
        };

        panel.Children.Add(editor);

        return panel;
    }

    private FrameworkElement CreateReorderListControl(SettingDefinition definition, object? value, LockedSettingState lockState)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 8, 0, 8)
        };

        var header = new TextBlock
        {
            Text = definition.Label,
            Style = (Style)Application.Current.Resources["SettingsItemHeaderStyle"]
        };
        panel.Children.Add(header);

        var description = new TextBlock
        {
            Text = definition.HelperText,
            Style = (Style)Application.Current.Resources["SettingsItemDescriptionStyle"],
            Margin = new Thickness(0, 4, 0, 8)
        };
        panel.Children.Add(description);

        var listView = new ListView
        {
            IsEnabled = !lockState.IsLocked
        };

        panel.Children.Add(listView);

        return panel;
    }

    private FrameworkElement CreateUnknownControl(SettingDefinition definition)
    {
        return new TextBlock
        {
            Text = $"Unknown control type: {definition.ControlType}",
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red)
        };
    }

    private FrameworkElement WrapInContainer(FrameworkElement control, SettingDefinition definition, LockedSettingState lockState)
    {
        var container = new Grid
        {
            Margin = new Thickness(0, 8, 0, 8)
        };
        container.Children.Add(control);

        if (lockState.IsLocked)
        {
            var toolTip = new ToolTip
            {
                Content = $"Managed by your organization\n{lockState.Reason}"
            };
            ToolTipService.SetToolTip(control, toolTip);

            var icon = new FontIcon
            {
                Glyph = "\uE72E",
                FontSize = 14,
                Margin = new Thickness(8, 0, 0, 0)
            };
        }

        return container;
    }
}
