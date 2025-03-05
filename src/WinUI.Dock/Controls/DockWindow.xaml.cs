using WinUI.Dock.Helpers;

namespace WinUI.Dock.Controls;

public sealed partial class DockWindow : Window
{
    public DockWindow(Document document)
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBar);

        AppWindow.Move(PointerHelpers.GetCursorPosition());
        AppWindow.Resize(new()
        {
            Width = (int)document.DockWidth,
            Height = (int)document.DockHeight
        });

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

        LayoutPanel panel = new() { Orientation = Orientation.Horizontal };
        panel.Children.Add(group);

        Panel.Children.Add(panel);
    }

    private void OnDragEnter(object _, DragEventArgs __)
    {
        Activate();
    }
}