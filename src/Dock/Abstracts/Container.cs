using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dock.Abstracts;

public abstract class Container : Control, IContainer
{
    public static readonly DependencyProperty DesignMinWidthProperty = DependencyProperty.Register(nameof(DesignMinWidth),
                                                                                                   typeof(double),
                                                                                                   typeof(Container),
                                                                                                   new PropertyMetadata(0.0));

    public static readonly DependencyProperty DesignMaxWidthProperty = DependencyProperty.Register(nameof(DesignMaxWidth),
                                                                                                   typeof(double),
                                                                                                   typeof(Container),
                                                                                                   new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty DesignWidthProperty = DependencyProperty.Register(nameof(DesignWidth),
                                                                                                typeof(GridLength),
                                                                                                typeof(Container),
                                                                                                new PropertyMetadata(new GridLength(1, GridUnitType.Star)));

    public static readonly DependencyProperty DesignMinHeightProperty = DependencyProperty.Register(nameof(DesignMinHeight),
                                                                                                    typeof(double),
                                                                                                    typeof(Container),
                                                                                                    new PropertyMetadata(0.0));

    public static readonly DependencyProperty DesignMaxHeightProperty = DependencyProperty.Register(nameof(DesignMaxHeight),
                                                                                                    typeof(double),
                                                                                                    typeof(Container),
                                                                                                    new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty DesignHeightProperty = DependencyProperty.Register(nameof(DesignHeight),
                                                                                                 typeof(GridLength),
                                                                                                 typeof(Container),
                                                                                                 new PropertyMetadata(new GridLength(1, GridUnitType.Star)));

    public double DesignMinWidth
    {
        get => (double)GetValue(DesignMinWidthProperty);
        set => SetValue(DesignMinWidthProperty, value);
    }

    public double DesignMaxWidth
    {
        get => (double)GetValue(DesignMaxWidthProperty);
        set => SetValue(DesignMaxWidthProperty, value);
    }

    public GridLength DesignWidth
    {
        get => (GridLength)GetValue(DesignWidthProperty);
        set => SetValue(DesignWidthProperty, value);
    }

    public double DesignMinHeight
    {
        get => (double)GetValue(DesignMinHeightProperty);
        set => SetValue(DesignMinHeightProperty, value);
    }

    public double DesignMaxHeight
    {
        get => (double)GetValue(DesignMaxHeightProperty);
        set => SetValue(DesignMaxHeightProperty, value);
    }

    public GridLength DesignHeight
    {
        get => (GridLength)GetValue(DesignHeightProperty);
        set => SetValue(DesignHeightProperty, value);
    }
}
