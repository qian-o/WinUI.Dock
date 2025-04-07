using Windows.ApplicationModel.DataTransfer;
using WinUI.Dock.Enums;
using WinUI.Dock.Helpers;

namespace WinUI.Dock.Controls;

public sealed partial class DockTargetButton : UserControl
{
    public static readonly DependencyProperty DockTargetProperty = DependencyProperty.Register(nameof(DockTarget),
                                                                                               typeof(DockTarget),
                                                                                               typeof(DockTargetButton),
                                                                                               new PropertyMetadata(DockTarget.Center));

    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target),
                                                                                           typeof(Control),
                                                                                           typeof(DockTargetButton),
                                                                                           new PropertyMetadata(null));

    public DockTargetButton()
    {
        InitializeComponent();
    }

    public DockTarget DockTarget
    {
        get => (DockTarget)GetValue(DockTargetProperty);
        set => SetValue(DockTargetProperty, value);
    }

    public Control? Target
    {
        get => (Control)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    protected override async void OnDragEnter(DragEventArgs e)
    {
        base.OnDragEnter(e);

        if (Target is DockManager manager)
        {
            string dragKey = (string)await e.DataView.GetDataAsync(DragDropHelpers.FormatId);

            if (DragDropHelpers.GetDocument(dragKey) is Document document)
            {
                manager.ShowDockPreview(document, DockTarget);
            }
        }
        else if (Target is DocumentGroup group)
        {
            group.ShowDockPreview(DockTarget);
        }
    }

    protected override void OnDragLeave(DragEventArgs e)
    {
        base.OnDragLeave(e);

        if (Target is DockManager manager)
        {
            manager.HideDockPreview();
        }
        else if (Target is DocumentGroup group)
        {
            group.HideDockPreview();
        }
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        base.OnDragOver(e);

        e.AcceptedOperation = DataPackageOperation.Move;
    }

    protected override async void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);

        string dragKey = (string)await e.DataView.GetDataAsync(DragDropHelpers.FormatId);

        if (DragDropHelpers.GetDocument(dragKey) is Document document)
        {
            if (Target is DockManager manager)
            {
                manager.HideDockPreview();
                manager.Dock(document, DockTarget);
            }
            else if (Target is DocumentGroup group)
            {
                group.HideDockPreview();
                group.Dock(document, DockTarget);
            }

            document.Root!.HideDockTargets();
        }
    }

    private void OnLoaded(object _, RoutedEventArgs __)
    {
        VisualStateManager.GoToState(this, DockTarget.ToString(), false);
    }
}
