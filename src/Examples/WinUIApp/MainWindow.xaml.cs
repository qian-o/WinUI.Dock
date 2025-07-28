using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.Dock;

namespace WinUIApp;

public class DockAdapter : IDockAdapter
{
    public void OnCreated(Document document)
    {
        document.Content = new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = $"New Document {document.Title}"
        };
    }

    public void OnCreated(DocumentGroup group, Document? draggedDocument)
    {
        if (draggedDocument is null)
        {
            return;
        }

        if (draggedDocument.Title.Contains("Side"))
        {
            group.TabPosition = TabPosition.Bottom;
            group.UseCompactTabs = true;
        }
    }

    public object? GetFloatingWindowTitleBar(Document? draggedDocument)
    {
        return new TextBlock()
        {
            Text = "Custom Title Bar",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
}

public class DockBehavior : IDockBehavior
{
    public void ActivateMainWindow()
    {
        App.MainWindow.Activate();
    }

    public void OnDocked(Document src, DockManager dest, DockTarget target)
    {
        Debug.WriteLine($"Document {src.Title} docked to {dest.Name} at {target}.");
    }

    public void OnDocked(Document src, DocumentGroup dest, DockTarget target)
    {
        Debug.WriteLine($"Document {src.Title} docked to group {dest.Name} at {target}.");
    }

    public void OnFloating(Document document)
    {
        Debug.WriteLine($"Document {document.Title} is now floating.");
    }
}

public sealed partial class MainWindow : Window
{
    private const string LayoutFile = "layout.json";

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Save_Click(object _, RoutedEventArgs __)
    {
        File.WriteAllText(LayoutFile, dockManager.SaveLayout());
    }

    private void Clear_Click(object _, RoutedEventArgs __)
    {
        dockManager.ClearLayout();
    }

    private void Open_Click(object _, RoutedEventArgs __)
    {
        if (File.Exists(LayoutFile))
        {
            dockManager.LoadLayout(File.ReadAllText(LayoutFile));
        }
    }

    private void ShowProperties_Click(object sender, RoutedEventArgs e)
    {
        if (DocumentHelpers.GetDocument(sender) is Document document)
        {
            Document properties = new()
            {
                Title = "Properties",
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = $"Title: {document.Title}" },
                        new TextBlock { Text = $"CanPin: {document.CanPin}" },
                        new TextBlock { Text = $"CanClose: {document.CanClose}" }
                    }
                }
            };

            properties.DockTo(document, DockTarget.SplitBottom);
        }
    }
}
