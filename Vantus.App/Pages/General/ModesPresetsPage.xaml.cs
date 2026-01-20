using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vantus.Core.Models;
using Vantus.Core.Services;

namespace Vantus.App.Pages.General;

public sealed partial class ModesPresetsPage : SettingsPageBase
{
    public ModesPresetsPage()
    {
        this.InitializeComponent();
    }

    protected override void LoadPage()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var contentGrid = new Grid
        {
            MaxWidth = 1000
        };

        var headerPanel = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, 24)
        };

        var titleText = new TextBlock
        {
            Text = "Modes & Presets",
            Style = (Style)Application.Current.Resources["TitleTextBlockStyle"],
            Margin = new Thickness(0, 0, 0, 8)
        };
        headerPanel.Children.Add(titleText);

        var introText = new TextBlock
        {
            Text = "Apply recommended defaults or customize settings to match your workflow. Presets provide a starting point you can refine.",
            Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondary"],
            TextWrapping = TextWrapping.Wrap
        };
        headerPanel.Children.Add(introText);

        contentGrid.Children.Add(headerPanel);

        var settingsPanel = new StackPanel
        {
            Name = "SettingsPanel",
            Margin = new Thickness(0, 0, 0, 24)
        };

        var presetSection = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, 24)
        };

        var sectionHeader = new TextBlock
        {
            Text = "Preset Selection",
            Style = (Style)Application.Current.Resources["SectionHeaderTextBlockStyle"],
            Margin = new Thickness(0, 0, 0, 8)
        };
        presetSection.Children.Add(sectionHeader);

        var presetCombo = new ComboBox
        {
            Header = "Preset",
            Width = 300,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var presets = PresetManager.GetAvailablePresets();
        foreach (var preset in presets)
        {
            presetCombo.Items.Add(new ComboBoxItem
            {
                Content = preset.Name,
                Tag = preset.Id
            });
        }

        var currentPreset = SettingsStore.GetValue<string>("general.preset") ?? "personal";
        var currentIndex = presets.FindIndex(p => p.Id == currentPreset);
        if (currentIndex >= 0)
        {
            presetCombo.SelectedIndex = currentIndex;
        }

        presetCombo.SelectionChanged += (s, e) =>
        {
            if (presetCombo.SelectedItem is ComboBoxItem item && item.Tag is string presetId)
            {
                ShowPreviewDiff(presetId);
            }
        };

        presetSection.Children.Add(presetCombo);

        var previewButton = new Button
        {
            Content = "Preview Changes",
            Margin = new Thickness(0, 8, 0, 8)
        };
        previewButton.Click += PreviewButton_Click;
        presetSection.Children.Add(previewButton);

        var diffPanel = new StackPanel
        {
            Name = "DiffPanel",
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(0, 16, 0, 0),
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardBackgroundFillColorDefault"],
            Padding = new Thickness(16),
            CornerRadius = new CornerRadius(8)
        };

        var diffHeader = new TextBlock
        {
            Text = "Changes that will be applied:",
            Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"],
            Margin = new Thickness(0, 0, 0, 12)
        };
        diffPanel.Children.Add(diffHeader);

        var diffList = new ItemsControl
        {
            Name = "DiffList"
        };
        diffPanel.Children.Add(diffList);

        var applyButton = new Button
        {
            Content = "Apply Preset",
            Margin = new Thickness(0, 16, 0, 0),
            Style = (Style)Application.Current.Resources["AccentButtonStyle"]
        };
        applyButton.Click += ApplyButton_Click;
        diffPanel.Children.Add(applyButton);

        presetSection.Children.Add(diffPanel);

        var revertButton = new Button
        {
            Content = "Revert to Preset Defaults",
            Margin = new Thickness(0, 16, 0, 0)
        };
        revertButton.Click += RevertButton_Click;
        presetSection.Children.Add(revertButton);

        settingsPanel.Children.Add(presetSection);

        var performanceSection = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, 24)
        };

        var perfHeader = new TextBlock
        {
            Text = "Performance Profile",
            Style = (Style)Application.Current.Resources["SectionHeaderTextBlockStyle"],
            Margin = new Thickness(0, 0, 0, 8)
        };
        performanceSection.Children.Add(perfHeader);

        var performanceCombo = new ComboBox
        {
            Header = "Performance profile",
            Description = "Controls CPU/disk usage and how aggressively Vantus runs.",
            Width = 300,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        performanceCombo.Items.Add(new ComboBoxItem { Content = "Quiet" });
        performanceCombo.Items.Add(new ComboBoxItem { Content = "Balanced" });
        performanceCombo.Items.Add(new ComboBoxItem { Content = "Performance" });

        var currentPerf = SettingsStore.GetValue<string>("general.performance_profile") ?? "Balanced";
        var perfIndex = currentPerf switch
        {
            "Quiet" => 0,
            "Performance" => 2,
            _ => 1
        };
        performanceCombo.SelectedIndex = perfIndex;

        performanceCombo.SelectionChanged += (s, e) =>
        {
            var selected = performanceCombo.SelectedIndex switch
            {
                0 => "Quiet",
                2 => "Performance",
                _ => "Balanced"
            };
            SettingsStore.SetValue("general.performance_profile", selected);
        };

        performanceSection.Children.Add(performanceCombo);
        settingsPanel.Children.Add(performanceSection);

        var privateSection = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, 24)
        };

        var privateHeader = new TextBlock
        {
            Text = "Privacy Mode",
            Style = (Style)Application.Current.Resources["SectionHeaderTextBlockStyle"],
            Margin = new Thickness(0, 0, 0, 8)
        };
        privateSection.Children.Add(privateHeader);

        var privateToggle = new ToggleSwitch
        {
            Header = "Private mode",
            IsOn = SettingsStore.GetValue<bool>("general.private_mode")
        };

        privateToggle.Toggled += (s, e) =>
        {
            SettingsStore.SetValue("general.private_mode", privateToggle.IsOn);
        };

        privateSection.Children.Add(privateToggle);
        settingsPanel.Children.Add(privateSection);

        var guardrailsSection = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, 24)
        };

        var guardrailsHeader = new TextBlock
        {
            Text = "Automation Guardrails",
            Style = (Style)Application.Current.Resources["SectionHeaderTextBlockStyle"],
            Margin = new Thickness(0, 0, 0, 8)
        };
        guardrailsSection.Children.Add(guardrailsHeader);

        var guardrailsCombo = new ComboBox
        {
            Header = "Automation guardrails",
            Width = 300,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        guardrailsCombo.Items.Add(new ComboBoxItem { Content = "Conservative" });
        guardrailsCombo.Items.Add(new ComboBoxItem { Content = "Standard" });
        guardrailsCombo.Items.Add(new ComboBoxItem { Content = "Strict" });

        var currentGuardrails = SettingsStore.GetValue<string>("general.automation_guardrails") ?? "Conservative";
        var guardrailsIndex = currentGuardrails switch
        {
            "Standard" => 1,
            "Strict" => 2,
            _ => 0
        };
        guardrailsCombo.SelectedIndex = guardrailsIndex;

        guardrailsCombo.SelectionChanged += (s, e) =>
        {
            var selected = guardrailsCombo.SelectedIndex switch
            {
                1 => "Standard",
                2 => "Strict",
                _ => "Conservative"
            };
            SettingsStore.SetValue("general.automation_guardrails", selected);
        };

        guardrailsSection.Children.Add(guardrailsCombo);
        settingsPanel.Children.Add(guardrailsSection);

        Grid.SetRow(settingsPanel, 1);
        contentGrid.Children.Add(settingsPanel);

        scrollViewer.Content = contentGrid;
        Content = scrollViewer;
    }

    private string _previewPresetId = string.Empty;

    private void ShowPreviewDiff(string presetId)
    {
        _previewPresetId = presetId;
        var diff = PresetManager.GetPreviewDiff(presetId);

        var diffPanel = FindName("DiffPanel") as StackPanel;
        var diffList = FindName("DiffList") as ItemsControl;

        if (diffPanel != null && diffList != null)
        {
            diffPanel.Visibility = Visibility.Visible;
            diffList.ItemsSource = diff.Changes.Take(20);

            if (diff.Changes.Count > 20)
            {
                var moreText = new TextBlock
                {
                    Text = $"... and {diff.Changes.Count - 20} more changes",
                    Margin = new Thickness(0, 8, 0, 0),
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                    Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondary"]
                };
            }
        }
    }

    private void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        var presetCombo = FindName("PresetCombo") as ComboBox;
        if (presetCombo?.SelectedItem is ComboBoxItem item && item.Tag is string presetId)
        {
            ShowPreviewDiff(presetId);
        }
    }

    private async void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_previewPresetId))
        {
            await PresetManager.ApplyPresetAsync(_previewPresetId);

            var diffPanel = FindName("DiffPanel") as StackPanel;
            if (diffPanel != null)
            {
                diffPanel.Visibility = Visibility.Collapsed;
            }
        }
    }

    private async void RevertButton_Click(object sender, RoutedEventArgs e)
    {
        var presetCombo = FindName("PresetCombo") as ComboBox;
        if (presetCombo?.SelectedItem is ComboBoxItem item && item.Tag is string presetId)
        {
            await PresetManager.ApplyPresetAsync(presetId);
        }
    }
}
