using WinUI.Dock.Helpers;

namespace WinUI.Dock.Controls;

public sealed partial class DockWindow : Window
{
    public DockWindow(DockManager dockManager, Document document)
    {
        InitializeComponent();

        dockManager.InvokeCreateNewWindow(document, TitleBar);

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBar);

        AppWindow.Move(PointerHelpers.GetCursorPosition());
        AppWindow.Resize(new()
        {
            Width = (int)(double.IsNaN(document.DockWidth) ? 400 : document.DockWidth),
            Height = (int)(double.IsNaN(document.DockHeight) ? 400 : document.DockHeight)
        });

        Panel.Children.CollectionChanged += (sender, e) =>
        {
            if (Panel.Children.Count is 0)
            {
                Close();
            }
        };

        Panel.Root = dockManager;

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