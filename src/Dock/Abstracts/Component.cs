using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dock.Abstracts;

public abstract class Component : Control, IComponent
{
    public static readonly DependencyProperty DockMinWidthProperty = DependencyProperty.Register(nameof(DockMinWidth),
                                                                                                 typeof(double),
                                                                                                 typeof(Component),
                                                                                                 new PropertyMetadata(0.0, (a, b) => ((Component)a).OnDockMinWidthChanged((double)b.OldValue, (double)b.NewValue)));

    public static readonly DependencyProperty DockMaxWidthProperty = DependencyProperty.Register(nameof(DockMaxWidth),
                                                                                                 typeof(double),
                                                                                                 typeof(Component),
                                                                                                 new PropertyMetadata(double.PositiveInfinity, (a, b) => ((Component)a).OnDockMaxWidthChanged((double)b.OldValue, (double)b.NewValue)));

    public static readonly DependencyProperty DockWidthProperty = DependencyProperty.Register(nameof(DockWidth),
                                                                                              typeof(GridLength),
                                                                                              typeof(Component),
                                                                                              new PropertyMetadata(new GridLength(1, GridUnitType.Star), (a, b) => ((Component)a).OnDockWidthChanged((GridLength)b.OldValue, (GridLength)b.NewValue)));

    public static readonly DependencyProperty DockMinHeightProperty = DependencyProperty.Register(nameof(DockMinHeight),
                                                                                                  typeof(double),
                                                                                                  typeof(Component),
                                                                                                  new PropertyMetadata(0.0, (a, b) => ((Component)a).OnDockMinHeightChanged((double)b.OldValue, (double)b.NewValue)));

    public static readonly DependencyProperty DockMaxHeightProperty = DependencyProperty.Register(nameof(DockMaxHeight),
                                                                                                  typeof(double),
                                                                                                  typeof(Component),
                                                                                                  new PropertyMetadata(double.PositiveInfinity, (a, b) => ((Component)a).OnDockMaxHeightChanged((double)b.OldValue, (double)b.NewValue)));

    public static readonly DependencyProperty DockHeightProperty = DependencyProperty.Register(nameof(DockHeight),
                                                                                               typeof(GridLength),
                                                                                               typeof(Component),
                                                                                               new PropertyMetadata(new GridLength(1, GridUnitType.Star), (a, b) => ((Component)a).OnDockHeightChanged((GridLength)b.OldValue, (GridLength)b.NewValue)));

    public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(nameof(Owner),
                                                                                          typeof(IComponent),
                                                                                          typeof(Component),
                                                                                          new PropertyMetadata(null, (a, b) => ((Component)a).OnOwnerChanged((IComponent)b.OldValue, (IComponent)b.NewValue)));

    public static readonly DependencyProperty ManagerProperty = DependencyProperty.Register(nameof(Manager),
                                                                                            typeof(DockingManager),
                                                                                            typeof(Component),
                                                                                            new PropertyMetadata(null, (a, b) => ((Component)a).OnManagerChanged((DockingManager)b.OldValue, (DockingManager)b.NewValue)));

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

    public IComponent? Owner
    {
        get => (IComponent)GetValue(OwnerProperty);
        set => SetValue(OwnerProperty, value);
    }

    public DockingManager? Manager
    {
        get => (DockingManager)GetValue(ManagerProperty);
        set => SetValue(ManagerProperty, value);
    }

    protected virtual void OnDockMinWidthChanged(double oldValue, double newValue)
    {
    }

    protected virtual void OnDockMaxWidthChanged(double oldValue, double newValue)
    {
    }

    protected virtual void OnDockWidthChanged(GridLength oldValue, GridLength newValue)
    {
    }

    protected virtual void OnDockMinHeightChanged(double oldValue, double newValue)
    {
    }

    protected virtual void OnDockMaxHeightChanged(double oldValue, double newValue)
    {
    }

    protected virtual void OnDockHeightChanged(GridLength oldValue, GridLength newValue)
    {
    }

    protected virtual void OnOwnerChanged(IComponent? oldOwner, IComponent? newOwner)
    {
    }

    protected virtual void OnManagerChanged(DockingManager? oldManager, DockingManager? newManager)
    {
    }
}
