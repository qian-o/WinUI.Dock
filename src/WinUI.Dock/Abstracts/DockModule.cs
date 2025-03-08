﻿using System.Text.Json.Nodes;
using WinUI.Dock.Helpers;

namespace WinUI.Dock.Abstracts;

public abstract class DockModule : Control
{
    public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(nameof(Owner),
                                                                                          typeof(DockModule),
                                                                                          typeof(DockModule),
                                                                                          new PropertyMetadata(null));

    public static readonly DependencyProperty RootProperty = DependencyProperty.Register(nameof(Root),
                                                                                         typeof(DockManager),
                                                                                         typeof(DockModule),
                                                                                         new PropertyMetadata(null, (d, e) => ((DockModule)d).OnRootChanged((DockManager?)e.OldValue, (DockManager?)e.NewValue)));

    public static readonly DependencyProperty DockMinWidthProperty = DependencyProperty.Register(nameof(DockMinWidth),
                                                                                                 typeof(double),
                                                                                                 typeof(DockModule),
                                                                                                 new PropertyMetadata(0.0));

    public static readonly DependencyProperty DockMaxWidthProperty = DependencyProperty.Register(nameof(DockMaxWidth),
                                                                                                 typeof(double),
                                                                                                 typeof(DockModule),
                                                                                                 new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty DockWidthProperty = DependencyProperty.Register(nameof(DockWidth),
                                                                                              typeof(double),
                                                                                              typeof(DockModule),
                                                                                              new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty DockMinHeightProperty = DependencyProperty.Register(nameof(DockMinHeight),
                                                                                                  typeof(double),
                                                                                                  typeof(DockModule),
                                                                                                  new PropertyMetadata(0.0));

    public static readonly DependencyProperty DockMaxHeightProperty = DependencyProperty.Register(nameof(DockMaxHeight),
                                                                                                  typeof(double),
                                                                                                  typeof(DockModule),
                                                                                                  new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty DockHeightProperty = DependencyProperty.Register(nameof(DockHeight),
                                                                                               typeof(double),
                                                                                               typeof(DockModule),
                                                                                               new PropertyMetadata(double.NaN));

    protected DockModule()
    {
        SizeChanged += OnSizeChanged;
    }

    public DockModule? Owner
    {
        get => (DockModule)GetValue(OwnerProperty);
        internal set => SetValue(OwnerProperty, value);
    }

    public DockManager? Root
    {
        get => (DockManager)GetValue(RootProperty);
        internal set => SetValue(RootProperty, value);
    }

    public double DockMinWidth
    {
        get => (double)GetValue(DockMinWidthProperty);
        set => SetValue(DockMinWidthProperty, value);
    }

    public double DockMaxWidth
    {
        get => (double)GetValue(DockMaxWidthProperty);
        set => SetValue(DockMaxWidthProperty, value);
    }

    public double DockWidth
    {
        get => (double)GetValue(DockWidthProperty);
        set => SetValue(DockWidthProperty, value);
    }

    public double DockMinHeight
    {
        get => (double)GetValue(DockMinHeightProperty);
        set => SetValue(DockMinHeightProperty, value);
    }

    public double DockMaxHeight
    {
        get => (double)GetValue(DockMaxHeightProperty);
        set => SetValue(DockMaxHeightProperty, value);
    }

    public double DockHeight
    {
        get => (double)GetValue(DockHeightProperty);
        set => SetValue(DockHeightProperty, value);
    }

    public void CopySizeFrom(DockModule module)
    {
        DockMinWidth = module.DockMinWidth;
        DockMaxWidth = module.DockMaxWidth;
        DockWidth = module.DockWidth;
        DockMinHeight = module.DockMinHeight;
        DockMaxHeight = module.DockMaxHeight;
        DockHeight = module.DockHeight;
    }

    public void Attach(DockModule owner)
    {
        if (Owner == owner)
        {
            return;
        }

        Detach();

        Owner = owner;
        Root = owner.Root;
    }

    public void Detach(bool detachEmptyContainer = true)
    {
        if (Owner is DockContainer container)
        {
            container.Children.Remove(this);

            if (detachEmptyContainer)
            {
                container.DetachEmptyContainer();
            }
        }

        Owner = null;
        Root = null;
    }

    protected virtual void OnRootChanged(DockManager? oldRoot, DockManager? newRoot)
    {
    }

    internal virtual JsonObject Serialize()
    {
        JsonObject json = [];

        json.Add(nameof(DockMinWidth), DockMinWidth);
        json.Add(nameof(DockMaxWidth), DockMaxWidth);
        json.Add(nameof(DockWidth), DockWidth);
        json.Add(nameof(DockMinHeight), DockMinHeight);
        json.Add(nameof(DockMaxHeight), DockMaxHeight);
        json.Add(nameof(DockHeight), DockHeight);

        return json;
    }

    internal virtual void Deserialize(JsonObject json)
    {
        DockMinWidth = json.Get<double>(nameof(DockMinWidth));
        DockMaxWidth = json.Get<double>(nameof(DockMaxWidth));
        DockWidth = json.Get<double>(nameof(DockWidth));
        DockMinHeight = json.Get<double>(nameof(DockMinHeight));
        DockMaxHeight = json.Get<double>(nameof(DockMaxHeight));
        DockHeight = json.Get<double>(nameof(DockHeight));
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        DockWidth = ActualWidth;
        DockHeight = ActualHeight;
    }
}
