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
        e.TitleBar.Child = new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = "Custom Title"
        };
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
