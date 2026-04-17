using System.ComponentModel;

namespace WinUI.Dock;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class DockTargetButton : UserControl
{
    public static readonly DependencyProperty DestinationProperty = DependencyProperty.Register(nameof(Destination),
                                                                                                typeof(Control),
                                                                                                typeof(DockTargetButton),
                                                                                                new PropertyMetadata(null));

    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target),
                                                                                           typeof(DockTarget),
                                                                                           typeof(DockTargetButton),
                                                                                           new PropertyMetadata(DockTarget.Center));

    public DockTargetButton()
    {
        InitializeComponent();
    }

    public Control? Destination
    {
        get => (Control)GetValue(DestinationProperty);
        set => SetValue(DestinationProperty, value);
    }

    public DockTarget Target
    {
        get => (DockTarget)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    private void OnLoaded(object _, RoutedEventArgs __)
    {
        VisualStateManager.GoToState(this, Target.ToString(), false);
    }
}
