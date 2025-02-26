namespace WinUI.Dock;

[ContentProperty(Name = nameof(Panel))]
public partial class DockingManager : Control
{
    public static readonly DependencyProperty PanelProperty = DependencyProperty.Register(nameof(Panel),
                                                                                          typeof(SplitPanel),
                                                                                          typeof(DockingManager),
                                                                                          new PropertyMetadata(null));

    public SplitPanel? Panel
    {
        get => (SplitPanel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }
}
