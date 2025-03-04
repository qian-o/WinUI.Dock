namespace WinUI.Dock.Controls;

public sealed partial class DockWindow : Window
{
    public DockWindow(Document document)
    {
        InitializeComponent();

        Panel.Root = document.Root;

        DocumentGroup group = new();
        group.Children.Add(document);

        Panel.Children.Add(group);
    }
}
