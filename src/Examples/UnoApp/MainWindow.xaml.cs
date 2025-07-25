using WinUI.Dock;

namespace UnoApp;

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

    private void DockManager_FillDocument(object _, FillDocumentEventArgs e)
    {
        e.Document.Content = new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = $"New Document {e.Title}"
        };
    }

    private void DockManager_NewGroup(object _, NewGroupEventArgs e)
    {
        if (e.Title.Contains("Side"))
        {
            e.Group.TabPosition = TabPosition.Bottom;
            e.Group.UseCompactTabs = true;
        }
    }

    private void DockManager_NewWindow(object _, NewWindowEventArgs e)
    {
        e.Window.Title = "Custom Window Title";

        e.TitleBar.Child = new TextBlock
        {
            Text = "Custom Title Bar",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
}
