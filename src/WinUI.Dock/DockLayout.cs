using WinUI.Dock.Abstracts;

namespace WinUI.Dock;

public partial class DockLayout : DockContainer
{
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation),
                                                                                                typeof(Orientation),
                                                                                                typeof(DockLayout),
                                                                                                new PropertyMetadata(Orientation.Vertical));

    public DockLayout()
    {
        DefaultStyleKey = typeof(DockLayout);
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    protected override void LoadChildren()
    {
    }

    protected override void UnloadChildren()
    {
    }

    protected override bool ValidateChildren()
    {
        return Children.All(static item => item is DockContainer);
    }
}
