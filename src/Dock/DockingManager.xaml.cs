using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.Controls;
using Dock.Abstractions;
using Dock.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Children))]
[TemplatePart(Name = "PART_DockPanel", Type = typeof(DockPanel))]
public sealed partial class DockingManager : Control
{
    public static readonly DependencyProperty LeftSideProperty = DependencyProperty.Register(nameof(LeftSide),
                                                                                             typeof(LayoutSide),
                                                                                             typeof(DockingManager),
                                                                                             new PropertyMetadata(null, OnLeftSideChanged));

    public static readonly DependencyProperty TopSideProperty = DependencyProperty.Register(nameof(TopSide),
                                                                                            typeof(LayoutSide),
                                                                                            typeof(DockingManager),
                                                                                            new PropertyMetadata(null, OnTopSideChanged));

    public static readonly DependencyProperty RightSideProperty = DependencyProperty.Register(nameof(RightSide),
                                                                                              typeof(LayoutSide),
                                                                                              typeof(DockingManager),
                                                                                              new PropertyMetadata(null, OnRightSideChanged));

    public static readonly DependencyProperty BottomSideProperty = DependencyProperty.Register(nameof(BottomSide),
                                                                                               typeof(LayoutSide),
                                                                                               typeof(DockingManager),
                                                                                               new PropertyMetadata(null, OnBottomSideChanged));

    public static readonly DependencyProperty SideProperty = DependencyProperty.RegisterAttached("Side",
                                                                                                 typeof(Side),
                                                                                                 typeof(DockingManager),
                                                                                                 new PropertyMetadata(Side.Left));

    private DockPanel? dockPanel;

    public DockingManager()
    {
        DefaultStyleKey = typeof(DockingManager);

        Children.CollectionChanged += (s, e) => UpdateDockPanel();
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

    public ObservableCollection<LayoutItem> Children { get; } = [];

    public static Side GetSide(LayoutItem obj)
    {
        return (Side)obj.GetValue(SideProperty);
    }

    public static void SetSide(LayoutItem obj, Side value)
    {
        obj.SetValue(SideProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        dockPanel = GetTemplateChild("PART_DockPanel") as DockPanel;

        UpdateDockPanel();
    }

    private void UpdateDockPanel()
    {
        if (dockPanel is null)
        {
            return;
        }

        dockPanel.Children.Clear();

        string[] anchorGroups = [.. Children.Where(item => item is LayoutAnchor).Select(x => x.Group).Distinct()];
        string[] documentGroups = [.. Children.Where(item => item is LayoutDocument).Select(x => x.Group).Distinct()];
    }

    private static void OnLeftSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is LayoutSide layoutSide)
        {
            layoutSide.Side = Side.Left;
        }
    }

    private static void OnTopSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is LayoutSide layoutSide)
        {
            layoutSide.Side = Side.Top;
        }
    }

    private static void OnRightSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is LayoutSide layoutSide)
        {
            layoutSide.Side = Side.Right;
        }
    }

    private static void OnBottomSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is LayoutSide layoutSide)
        {
            layoutSide.Side = Side.Bottom;
        }
    }
}
