using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Text;
using WinUI.Dock;

namespace Example.ViewModels;

public partial class MainViewModel : ObservableObject, IDockAdapter, IDockBehavior
{
    public void OnCreated(Document document)
    {
    }

    public void OnCreated(DocumentGroup group, Document? draggedDocument)
    {
    }

    public object? GetFloatingWindowTitleBar(Document? draggedDocument)
    {
        return new TextBlock()
        {
            Text = "Example Floating Window",
            FontSize = 16,
            FontWeight = FontWeights.Bold
        };
    }

    public void ActivateMainWindow()
    {
    }

    public void OnDocked(Document src, DockManager dest, DockTarget target)
    {
    }

    public void OnDocked(Document src, DocumentGroup dest, DockTarget target)
    {
    }

    public void OnFloating(Document document)
    {
    }
}
