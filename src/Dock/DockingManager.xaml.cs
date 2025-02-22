using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Container))]
public partial class DockingManager : Control
{
    public static readonly DependencyProperty ContainerProperty = DependencyProperty.Register(nameof(Container),
                                                                                              typeof(LayoutContainer),
                                                                                              typeof(DockingManager),
                                                                                              new PropertyMetadata(null, OnContainerChanged));

    public DockingManager()
    {
        DefaultStyleKey = typeof(DockingManager);

        Left.CollectionChanged += OnCollectionChanged;
        Top.CollectionChanged += OnCollectionChanged;
        Right.CollectionChanged += OnCollectionChanged;
        Bottom.CollectionChanged += OnCollectionChanged;
    }

    public LayoutContainer? Container
    {
        get => (LayoutContainer)GetValue(ContainerProperty);
        set => SetValue(ContainerProperty, value);
    }

    public ObservableCollection<DocumentContainer> Left { get; } = [];

    public ObservableCollection<DocumentContainer> Top { get; } = [];

    public ObservableCollection<DocumentContainer> Right { get; } = [];

    public ObservableCollection<DocumentContainer> Bottom { get; } = [];

    private static void OnContainerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is LayoutContainer newContainer)
        {
            newContainer.Manager = (DockingManager)d;
        }

        if (e.OldValue is LayoutContainer oldContainer)
        {
            oldContainer.Manager = null;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (object? child in e.NewItems)
            {
                if (child is DocumentContainer container)
                {
                    container.Manager = this;
                }
            }
        }

        if (e.OldItems is not null)
        {
            foreach (object? child in e.OldItems)
            {
                if (child is DocumentContainer container)
                {
                    container.Manager = null;
                }
            }
        }
    }
}
