using WinUI.Dock.Abstracts;

namespace WinUI.Dock;

public partial class SplitPanel : DockContainer
{
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation),
                                                                                                typeof(Orientation),
                                                                                                typeof(SplitPanel),
                                                                                                new PropertyMetadata(Orientation.Vertical));

    public SplitPanel()
    {
        DefaultStyleKey = typeof(SplitPanel);
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
