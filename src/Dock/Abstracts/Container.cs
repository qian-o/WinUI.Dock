using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dock.Abstracts;

public abstract class Container : Control, IContainer
{
    public static readonly DependencyProperty DockMinWidthProperty = DependencyProperty.Register(nameof(DockMinWidth),
                                                                                                 typeof(double),
                                                                                                 typeof(Container),
                                                                                                 new PropertyMetadata(0.0));

    public static readonly DependencyProperty DockMaxWidthProperty = DependencyProperty.Register(nameof(DockMaxWidth),
                                                                                                 typeof(double),
                                                                                                 typeof(Container),
                                                                                                 new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty DockWidthProperty = DependencyProperty.Register(nameof(DockWidth),
                                                                                              typeof(GridLength),
                                                                                              typeof(Container),
                                                                                              new PropertyMetadata(new GridLength(1, GridUnitType.Star)));

    public static readonly DependencyProperty DockMinHeightProperty = DependencyProperty.Register(nameof(DockMinHeight),
                                                                                                  typeof(double),
                                                                                                  typeof(Container),
                                                                                                  new PropertyMetadata(0.0));

    public static readonly DependencyProperty DockMaxHeightProperty = DependencyProperty.Register(nameof(DockMaxHeight),
                                                                                                  typeof(double),
                                                                                                  typeof(Container),
                                                                                                  new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty DockHeightProperty = DependencyProperty.Register(nameof(DockHeight),
                                                                                               typeof(GridLength),
                                                                                               typeof(Container),
                                                                                               new PropertyMetadata(new GridLength(1, GridUnitType.Star)));

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

    public GridLength DockWidth
    {
        get => (GridLength)GetValue(DockWidthProperty);
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

    public GridLength DockHeight
    {
        get => (GridLength)GetValue(DockHeightProperty);
        set => SetValue(DockHeightProperty, value);
    }
}
