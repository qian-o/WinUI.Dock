using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Text;
using WinUI.Dock;

namespace Example.ViewModels;

public partial class MainViewModel : ObservableObject, IDockAdapter, IDockBehavior
{
    public void OnCreated(Document document)
    {
        document.Content = new TextBlock()
        {
            Text = document.ActualTitle,
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    public void OnCreated(DocumentGroup group, Document? draggedDocument)
    {
        if (draggedDocument?.Title.Contains("Bottom##") is true)
        {
            group.TabPosition = TabPosition.Bottom;
        }
    }

    public object? GetFloatingWindowTitleBar(Document? draggedDocument)
    {
        return new TextBlock()
        {
            IsHitTestVisible = false,
            Text = "Example Floating Window",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 16,
            FontWeight = FontWeights.Bold
        };
    }

    public void ActivateMainWindow()
    {
        App.MainWindow.Activate();
    }

    public void OnDocked(Document src, DockManager dest, DockTarget target)
    {
        Debug.WriteLine($"Document '{src.ActualTitle}' docked to DockManager at target '{target}'.");
    }

    public void OnDocked(Document src, DocumentGroup dest, DockTarget target)
    {
        Debug.WriteLine($"Document '{src.ActualTitle}' docked to DocumentGroup at target '{target}'.");
    }

    public void OnFloating(Document document)
    {
        Debug.WriteLine($"Document '{document.ActualTitle}' is now floating.");
    }
}
