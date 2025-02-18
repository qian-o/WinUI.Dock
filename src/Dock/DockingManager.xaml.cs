using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(RootPanel))]
public sealed partial class DockingManager : Control
{
    public static readonly DependencyProperty LeftSideProperty = DependencyProperty.Register(nameof(LeftSide),
                                                                                             typeof(LayoutSide),
                                                                                             typeof(DockingManager),
                                                                                             new PropertyMetadata(null));

    public static readonly DependencyProperty TopSideProperty = DependencyProperty.Register(nameof(TopSide),
                                                                                            typeof(LayoutSide),
                                                                                            typeof(DockingManager),
                                                                                            new PropertyMetadata(null));

    public static readonly DependencyProperty RightSideProperty = DependencyProperty.Register(nameof(RightSide),
                                                                                              typeof(LayoutSide),
                                                                                              typeof(DockingManager),
                                                                                              new PropertyMetadata(null));

    public static readonly DependencyProperty BottomSideProperty = DependencyProperty.Register(nameof(BottomSide),
                                                                                               typeof(LayoutSide),
                                                                                               typeof(DockingManager),
                                                                                               new PropertyMetadata(null));

    public static readonly DependencyProperty RootPanelProperty = DependencyProperty.Register(nameof(RootPanel),
                                                                                              typeof(LayoutPanel),
                                                                                              typeof(DockingManager),
                                                                                              new PropertyMetadata(null));

    public DockingManager()
    {
        DefaultStyleKey = typeof(DockingManager);
    }

    public LayoutSide? LeftSide
    {
        get => (LayoutSide)GetValue(LeftSideProperty);
        set => SetValue(LeftSideProperty, value);
    }

    public LayoutSide? TopSide
    {
        get => (LayoutSide)GetValue(TopSideProperty);
        set => SetValue(TopSideProperty, value);
    }

    public LayoutSide? RightSide
    {
        get => (LayoutSide)GetValue(RightSideProperty);
        set => SetValue(RightSideProperty, value);
    }

    public LayoutSide? BottomSide
    {
        get => (LayoutSide)GetValue(BottomSideProperty);
        set => SetValue(BottomSideProperty, value);
    }

    public LayoutPanel? RootPanel
    {
        get => (LayoutPanel)GetValue(RootPanelProperty);
        set => SetValue(RootPanelProperty, value);
    }
}
