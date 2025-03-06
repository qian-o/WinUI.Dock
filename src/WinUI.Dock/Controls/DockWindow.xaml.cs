using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Graphics;
using WinUI.Dock.Helpers;

namespace WinUI.Dock.Controls;

public sealed partial class DockWindow : Window
{
    private PointInt32 dragStart;

    public DockWindow(DockManager dockManager, Document document)
    {
        InitializeComponent();

        dockManager.InvokeCreateNewWindow(TitleBar);

        ExtendsContentIntoTitleBar = true;

#if WINDOWS
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
#endif

        AppWindow.Move(PointerHelpers.GetPointerPosition());
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

    private void TitleBar_DragStarted(object _, DragStartedEventArgs __)
    {
        dragStart = PointerHelpers.GetPointerPosition();
    }

    private void TitleBar_DragDelta(object _, DragDeltaEventArgs __)
    {
        PointInt32 dragEnd = PointerHelpers.GetPointerPosition();

        AppWindow.Move(new()
        {
            X = AppWindow.Position.X + (dragEnd.X - dragStart.X),
            Y = AppWindow.Position.Y + (dragEnd.Y - dragStart.Y)
        });

        dragStart = dragEnd;
    }
}