using Microsoft.UI.Xaml;
using WinUI.Dock;
using WinUI.Dock.Enums;

namespace WinUIApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void DockManager_DocumentGroupReady(object _, DocumentGroupReadyEventArgs e)
    {
        if (e.DocumentTitle.Contains("Side"))
        {
            e.DocumentGroup.TabPosition = TabPosition.Bottom;
            e.DocumentGroup.IsTabWidthBasedOnContent = true;
        }
    }
}
