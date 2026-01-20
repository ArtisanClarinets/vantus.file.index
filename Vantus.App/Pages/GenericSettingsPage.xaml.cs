using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vantus.Core.Models;
using Vantus.Core.Services;

namespace Vantus.App.Pages;

public sealed partial class GenericSettingsPage : SettingsPageBase
{
    public GenericSettingsPage()
    {
        this.InitializeComponent();
    }

    protected override void LoadPage()
    {
        if (!Schema.Categories.TryGetValue(Category, out var categoryData))
        {
            Content = new TextBlock { Text = "Category not found" };
            return;
        }

        if (!categoryData.Pages.TryGetValue(PageName, out var pageData))
        {
            Content = new TextBlock { Text = "Page not found" };
            return;
        }

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var contentGrid = new Grid
        {
            MaxWidth = 1000,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        var headerPanel = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, 24)
        };

        var titleText = new TextBlock
        {
            Text = pageData.Name,
            Style = (Style)Application.Current.Resources["TitleTextBlockStyle"],
            Margin = new Thickness(0, 0, 0, 8)
        };
        headerPanel.Children.Add(titleText);

        var introText = new TextBlock
        {
            Text = GetPageIntro(),
            Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondary"],
            TextWrapping = TextWrapping.Wrap
        };
        headerPanel.Children.Add(introText);

        Grid.SetRow(headerPanel, 0);
        contentGrid.Children.Add(headerPanel);

        var settingsPanel = new StackPanel
        {
            Name = "SettingsPanel"
        };
        Grid.SetRow(settingsPanel, 1);
        contentGrid.Children.Add(settingsPanel);

        scrollViewer.Content = contentGrid;
        Content = scrollViewer;

        RenderSettings(settingsPanel, pageData);
    }

    private string GetPageIntro()
    {
        return $"Configure {PageName.ToLower().Replace("_", " ")} settings for Vantus.";
    }

    private void RenderSettings(StackPanel panel, SettingsPage pageData)
    {
        var currentSection = string.Empty;

        foreach (var setting in pageData.Settings)
        {
            var control = ControlFactory.CreateControl(setting, setting.Scope);
            panel.Children.Add(control);
        }
    }
}
