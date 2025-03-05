using Microsoft.UI;
using WinUI.Dock;
using WinUI.Dock.Enums;

namespace UnoApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void DockManager_CreateNewWindow(object _, CreateNewWindowEventArgs e)
    {
        e.TitleBar.Background = new SolidColorBrush(Colors.DarkGray);
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
