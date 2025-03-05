namespace WinUI.Dock.Controls;

public sealed partial class DockWindow : Window
{
    public DockWindow(Document document)
    {
        InitializeComponent();

        Panel.Children.CollectionChanged += (sender, e) =>
        {
            if (Panel.Children.Count is 0)
            {
                Close();
            }
        };

        Panel.Root = document.Root;

        document.Detach();

        DocumentGroup group = new();
        group.Children.Add(document);

        Panel.Children.Add(group);
    }
}