namespace WinUI.Dock.Abstracts;

public abstract class DockModule : Control
{
    public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(nameof(Owner),
                                                                                          typeof(DockModule),
                                                                                          typeof(DockModule),
                                                                                          new PropertyMetadata(null));

    public static readonly DependencyProperty RootProperty = DependencyProperty.Register(nameof(Root),
                                                                                         typeof(DockingManager),
                                                                                         typeof(DockModule),
                                                                                         new PropertyMetadata(null));

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

    public void Detach()
    {
        if (Owner is DockContainer container)
        {
            container.Children.Remove(this);
        }

        Owner = null;
        Root = null;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        DockWidth = ActualWidth;
        DockHeight = ActualHeight;
    }
}
