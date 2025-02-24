using Dock.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace Dock.Components;

public sealed partial class DockTarget : UserControl
{
    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target),
                                                                                           typeof(Target),
                                                                                           typeof(DockTarget),
                                                                                           new PropertyMetadata(Target.Center));

    public DockTarget()
    {
        InitializeComponent();

        DragOver += OnDragOver;
        DragLeave += OnDragLeave;
    }

    public Target Target
    {
        get => (Target)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        Opacity = 1;

        e.AcceptedOperation = DataPackageOperation.Move;
        e.Handled = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        Opacity = 0.4;

        e.AcceptedOperation = DataPackageOperation.None;
        e.Handled = true;
    }
}
