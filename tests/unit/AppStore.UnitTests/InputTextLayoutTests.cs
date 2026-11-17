using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace AppStore.UnitTests.Layout;

public class InputTextLayoutTests
{
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Theory]
    [InlineData("LightAppTheme.xaml")]
    [InlineData("DarkAppTheme.xaml")]
    public void FormTextBoxContentHost_UsesTextBoxScrollSettings(string themeFile)
    {
        var document = LoadXaml(Path.Combine("Resources", "Styles", themeFile));
        var style = document.Descendants(Presentation + "Style")
            .Single(element => (string?)element.Attribute(Xaml + "Key") == "FormTextBoxStyle");
        var host = style.Descendants(Presentation + "ScrollViewer")
            .Single(element => (string?)element.Attribute(Xaml + "Name") == "PART_ContentHost");

        Assert.Equal("{TemplateBinding ScrollViewer.HorizontalScrollBarVisibility}",
            (string?)host.Attribute("HorizontalScrollBarVisibility"));
        Assert.Equal("{TemplateBinding ScrollViewer.VerticalScrollBarVisibility}",
            (string?)host.Attribute("VerticalScrollBarVisibility"));
    }

    [Theory]
    [InlineData("LightAppTheme.xaml")]
    [InlineData("DarkAppTheme.xaml")]
    public void SearchBoxContentHost_IsNotVerticallyCentered(string themeFile)
    {
        var document = LoadXaml(Path.Combine("Resources", "Styles", themeFile));
        var style = document.Descendants(Presentation + "Style")
            .Single(element => (string?)element.Attribute(Xaml + "Key") == "SearchBoxStyle");
        var host = style.Descendants(Presentation + "ScrollViewer")
            .Single(element => (string?)element.Attribute(Xaml + "Name") == "PART_ContentHost");

        Assert.NotEqual("Center", (string?)host.Attribute("VerticalAlignment"));
    }

    [Fact]
    public void ProfileTextInputs_WrapAndGrowWithoutInnerVerticalScrollbars()
    {
        var document = LoadXaml(Path.Combine("Views", "ProfilePopup.xaml"));

        foreach (var property in new[] { "FirstName", "LastName", "Email" })
        {
            var textBox = FindTextBox(document, property);
            Assert.Equal("Wrap", (string?)textBox.Attribute("TextWrapping"));
            Assert.Equal("Disabled", (string?)textBox.Attribute("HorizontalScrollBarVisibility"));
            Assert.Equal("Disabled", (string?)textBox.Attribute("VerticalScrollBarVisibility"));
            Assert.Null(textBox.Attribute("Height"));
        }
    }

    [Fact]
    public void ProfilePopup_UsesOneOuterVerticalScrollViewer()
    {
        var document = LoadXaml(Path.Combine("Views", "ProfilePopup.xaml"));
        var scrollViewer = document.Descendants(Presentation + "ScrollViewer")
            .Single(element => (string?)element.Attribute(Xaml + "Name") == "ProfileContentScroll");

        Assert.Equal("Auto", (string?)scrollViewer.Attribute("VerticalScrollBarVisibility"));
        Assert.Equal("Disabled", (string?)scrollViewer.Attribute("HorizontalScrollBarVisibility"));
    }

    [Fact]
    public void AddEditTextInputs_WrapAndGrowWhileDescriptionsUseTheParentScrollViewer()
    {
        var document = LoadXaml(Path.Combine("Views", "AddEditWindow.xaml"));

        foreach (var property in new[] { "App.ShortName", "App.FullName", "App.Developer", "App.Description", "App.DescriptionEn", "TagsText" })
        {
            var textBox = FindTextBox(document, property);
            Assert.Equal("Wrap", (string?)textBox.Attribute("TextWrapping"));
            Assert.Equal("Disabled", (string?)textBox.Attribute("HorizontalScrollBarVisibility"));
            Assert.Equal("Disabled", (string?)textBox.Attribute("VerticalScrollBarVisibility"));
            Assert.Null(textBox.Attribute("Height"));
        }

        foreach (var property in new[] { "App.Description", "App.DescriptionEn" })
        {
            var textBox = FindTextBox(document, property);
            Assert.Equal("80", (string?)textBox.Attribute("MinHeight"));
        }

        var formScrollViewer = document.Descendants(Presentation + "ScrollViewer")
            .Single(element => (string?)element.Attribute("Grid.Row") == "1");
        Assert.Equal("Auto", (string?)formScrollViewer.Attribute("VerticalScrollBarVisibility"));
    }

    [Fact]
    public void ProfilePasswordContentHosts_AreNotVerticallyCentered()
    {
        var document = LoadXaml(Path.Combine("Views", "ProfilePopup.xaml"));
        var hosts = document.Descendants(Presentation + "ScrollViewer")
            .Where(element => (string?)element.Attribute(Xaml + "Name") == "PART_ContentHost")
            .ToArray();

        Assert.Equal(2, hosts.Length);
        Assert.All(hosts, host => Assert.NotEqual("Center", (string?)host.Attribute("VerticalAlignment")));
    }

    private static XElement FindTextBox(XDocument document, string bindingProperty) =>
        document.Descendants(Presentation + "TextBox")
            .Single(element => ((string?)element.Attribute("Text"))?.Contains($"Binding {bindingProperty},", StringComparison.Ordinal) == true);

    private static XDocument LoadXaml(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Project.sln")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return XDocument.Load(Path.Combine(directory!.FullName, relativePath));
    }
}