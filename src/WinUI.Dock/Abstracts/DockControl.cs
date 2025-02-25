using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Dock.Abstracts;

public abstract class DockControl : Control
{
    public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(nameof(Owner),
                                                                                          typeof(DockControl),
                                                                                          typeof(DockControl),
                                                                                          new PropertyMetadata(null));

    public static readonly DependencyProperty RootProperty = DependencyProperty.Register(nameof(Root),
                                                                                         typeof(DockingManager),
                                                                                         typeof(DockControl),
                                                                                         new PropertyMetadata(null));

    public static readonly DependencyProperty DockMinWidthProperty = DependencyProperty.Register(nameof(DockMinWidth),
                                                                                                 typeof(double),
                                                                                                 typeof(DockControl),
                                                                                                 new PropertyMetadata(0.0));

    public static readonly DependencyProperty DockMaxWidthProperty = DependencyProperty.Register(nameof(DockMaxWidth),
                                                                                                 typeof(double),
                                                                                                 typeof(DockControl),
                                                                                                 new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty DockWidthProperty = DependencyProperty.Register(nameof(DockWidth),
                                                                                              typeof(double),
                                                                                              typeof(DockControl),
                                                                                              new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty DockMinHeightProperty = DependencyProperty.Register(nameof(DockMinHeight),
                                                                                                  typeof(double),
                                                                                                  typeof(DockControl),
                                                                                                  new PropertyMetadata(0.0));

    public static readonly DependencyProperty DockMaxHeightProperty = DependencyProperty.Register(nameof(DockMaxHeight),
                                                                                                  typeof(double),
                                                                                                  typeof(DockControl),
                                                                                                  new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty DockHeightProperty = DependencyProperty.Register(nameof(DockHeight),
                                                                                               typeof(double),
                                                                                               typeof(DockControl),
                                                                                               new PropertyMetadata(double.NaN));

    public DockControl? Owner
    {
        get => (DockControl)GetValue(OwnerProperty);
        internal set => SetValue(OwnerProperty, value);
    }

    public DockingManager? Root
    {
        get => (DockingManager)GetValue(RootProperty);
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

    public void Attach(DockControl owner)
    {
        if (Owner == owner)
        {
            return;
        }

        Detach();

        Owner = owner;
        Root = owner.Root;
    }

    public void Detach()
    {
        if (Owner is DockControlGroup group)
        {
            group.Children.Remove(this);
        }

        Owner = null;
        Root = null;
    }
}
