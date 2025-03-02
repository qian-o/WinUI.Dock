using WinUI.Dock.Enums;

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
}
