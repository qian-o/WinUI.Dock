﻿using System.Collections.ObjectModel;
using Dock.Abstractions;
using Dock.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Children))]
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

    public ObservableCollection<LayoutItem> Children { get; } = [];

    public static Side GetSide(LayoutAnchor obj)
    {
        return (Side)obj.GetValue(SideProperty);
    }

    public static void SetSide(LayoutAnchor obj, Side value)
    {
        obj.SetValue(SideProperty, value);
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
