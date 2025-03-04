using System.Diagnostics;
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

    public DockTargetButton()
    {
        InitializeComponent();
    }

    public DockTarget DockTarget
    {
        get => (DockTarget)GetValue(DockTargetProperty);
        set => SetValue(DockTargetProperty, value);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        base.OnDragOver(e);

        e.AcceptedOperation = DataPackageOperation.Move;
    }

    protected override async void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);

        if (DragDropHelpers.GetDocument((string)await e.DataView.GetDataAsync(DragDropHelpers.Format)) is Document document)
        {
            Debug.WriteLine($"Document Title: {document.Title}");
        }
    }

    private void OnLoaded(object _, RoutedEventArgs __)
    {
        VisualStateManager.GoToState(this, DockTarget.ToString(), false);
    }
}
