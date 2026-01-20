using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Vantus.App.Services;
using Vantus.Core.Models;
using Vantus.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Vantus.App.Pages;

public class SettingsPageBase : Page
{
    protected SettingsSchema Schema = null!;
    protected string Category = string.Empty;
    protected string PageName = string.Empty;
    protected SettingsStore SettingsStore = null!;
    protected SettingsControlFactory ControlFactory = null!;
    protected PolicyEngine PolicyEngine = null!;
    protected PresetManager PresetManager = null!;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is NavigationParams @params)
        {
            Schema = @params.Schema;
            Category = @params.Category;
            PageName = @params.Page;

            SettingsStore = App.Services.GetRequiredService<SettingsStore>();
            ControlFactory = App.Services.GetRequiredService<SettingsControlFactory>();
            PolicyEngine = App.Services.GetRequiredService<PolicyEngine>();
            PresetManager = App.Services.GetRequiredService<PresetManager>();

            LoadPage();
        }
    }

    protected virtual void LoadPage() { }

    protected void AddSection(StackPanel panel, string header, string description)
    {
        var sectionHeader = new TextBlock
        {
            Text = header,
            Style = (Style)Application.Current.Resources["SectionHeaderTextBlockStyle"],
            Margin = new Thickness(0, 24, 0, 8)
        };
        panel.Children.Add(sectionHeader);

        if (!string.IsNullOrEmpty(description))
        {
            var sectionDesc = new TextBlock
            {
                Text = description,
                Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondary"],
                Margin = new Thickness(0, 0, 0, 16),
                TextWrapping = TextWrapping.Wrap
            };
            panel.Children.Add(sectionDesc);
        }
    }

    protected void AddSetting(StackPanel panel, SettingDefinition definition)
    {
        var control = ControlFactory.CreateControl(definition, definition.Scope);
        panel.Children.Add(control);
    }

    protected void AddDangerZone(StackPanel panel)
    {
        var warningBar = new InfoBar
        {
            Title = "Danger Zone",
            Message = "Actions in this section can result in data loss. Proceed with caution.",
            Severity = InfoBarSeverity.Warning,
            IsOpen = true,
            Margin = new Thickness(0, 24, 0, 8)
        };
        panel.Children.Add(warningBar);
    }

    public void ScrollToTop()
    {
        if (this.Parent is ScrollViewer scrollViewer)
        {
            scrollViewer.ChangeView(null, 0, null, true);
        }
    }
}
