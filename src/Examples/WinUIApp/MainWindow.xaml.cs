using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.Dock;
using WinUI.Dock.Enums;

namespace WinUIApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void DockManager_CreateNewGroup(object _, CreateNewGroupEventArgs e)
    {
        if (e.Title.Contains("Side"))
        {
            e.Group.TabPosition = TabPosition.Bottom;
            e.Group.IsTabWidthBasedOnContent = true;
        }
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
}
