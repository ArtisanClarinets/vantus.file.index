using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using Vantus.App.ViewModels;
using System.Text.Json;
using System.Windows.Data;

namespace Vantus.App.Controls;

public partial class SettingsPageRenderer : UserControl
{
    public List<SettingGroup> Groups
    {
        get => (List<SettingGroup>)GetValue(GroupsProperty);
        set => SetValue(GroupsProperty, value);
    }

    public static readonly DependencyProperty GroupsProperty =
        DependencyProperty.Register("Groups", typeof(List<SettingGroup>), typeof(SettingsPageRenderer), new PropertyMetadata(null, OnGroupsChanged));

    private static void OnGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SettingsPageRenderer)d).Render();
    }

    public SettingsPageRenderer()
    {
        InitializeComponent();
    }

    private void Render()
    {
        RootPanel.Children.Clear();
        if (Groups == null) return;

        foreach (var group in Groups)
        {
            if (!string.IsNullOrEmpty(group.Header))
            {
                var headerBlock = new System.Windows.Controls.TextBlock {
                    Text = group.Header,
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                RootPanel.Children.Add(headerBlock);
            }

            var groupPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            foreach(var setting in group.Settings)
            {
                 var control = CreateControl(setting);
                 if (control != null)
                 {
                     groupPanel.Children.Add(control);
                     // Add spacing
                     groupPanel.Children.Add(new Border { Height = 4 });
                 }
            }
            RootPanel.Children.Add(groupPanel);
        }
    }

    private FrameworkElement CreateControl(SettingViewModel vm)
    {
        var card = new CardControl();
        card.Header = vm.Definition.Label;
        card.Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 }; // Default icon
        
        // Helper text
        if (!string.IsNullOrEmpty(vm.Definition.HelperText))
        {
             // CardControl usually displays description if available? 
             // In Wpf.Ui 3.x, CardControl has Content, Header, Icon. 
             // Maybe we use ToolTip or put description in Content if logical.
             // But usually we want the control in Content.
             ToolTipService.SetToolTip(card, vm.Definition.HelperText);
        }

        if (vm.IsLocked)
        {
            card.Icon = new SymbolIcon { Symbol = SymbolRegular.LockClosed24 };
            ToolTipService.SetToolTip(card, $"Managed by your organization. {vm.LockReason}");
        }

        FrameworkElement? content = null;

        try {
            switch (vm.Definition.ControlType)
            {
                case "toggle":
                    var ts = new ToggleSwitch();
                    try { ts.IsChecked = Convert.ToBoolean(vm.Value); } catch {}
                    ts.Click += (s, e) => vm.Value = ts.IsChecked;
                    content = ts;
                    break;
                case "slider":
                    var sl = new Slider();
                    sl.Width = 200;
                    sl.TickFrequency = 1;
                    if (vm.Definition.AllowedValues is JsonElement je && je.ValueKind == JsonValueKind.Object)
                    {
                        if(je.TryGetProperty("min", out var min)) sl.Minimum = min.GetDouble();
                        if(je.TryGetProperty("max", out var max)) sl.Maximum = max.GetDouble();
                        if(je.TryGetProperty("step", out var step)) sl.TickFrequency = step.GetDouble();
                    }
                    try { sl.Value = Convert.ToDouble(vm.Value); } catch {}
                    sl.ValueChanged += (s, e) => vm.Value = sl.Value;
                    content = sl;
                    break;
                case "dropdown":
                    var cb = new System.Windows.Controls.ComboBox();
                    cb.Width = 200;
                    if (vm.Definition.AllowedValues is JsonElement jeArr && jeArr.ValueKind == JsonValueKind.Array)
                    {
                        var list = new List<string>();
                        foreach(var item in jeArr.EnumerateArray()) list.Add(item.ToString());
                        cb.ItemsSource = list;
                    }
                    else if (vm.Definition.AllowedValues is string[] arr)
                    {
                        cb.ItemsSource = arr;
                    }

                    try { cb.SelectedItem = vm.Value?.ToString(); } catch {}
                    cb.SelectionChanged += (s, e) => vm.Value = cb.SelectedItem;
                    content = cb;
                    break;
                case "button":
                    var btn = new Wpf.Ui.Controls.Button();
                    btn.Content = "Action";
                    content = btn;
                    break;
                case "status":
                    var tb = new System.Windows.Controls.TextBlock();
                    tb.Text = vm.Value?.ToString() ?? "";
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    content = tb;
                    break;
            }
        }
        catch { }

        if (content != null)
        {
            if (vm.IsLocked) content.IsEnabled = false;
            card.Content = content;
        }

        return card;
    }
}
