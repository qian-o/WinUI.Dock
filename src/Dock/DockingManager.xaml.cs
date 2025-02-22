using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Dock.Components;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Container))]
[TemplatePart(Name = "PART_PopupContainer", Type = typeof(Grid))]
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

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        popupContainer = (Grid)GetTemplateChild("PART_PopupContainer");
    }

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

public partial class DockingManager
{
    private Grid popupContainer = null!;

    public void Show(Document document)
    {
        DocumentContainer container = (DocumentContainer)document.Owner!;

        Popup popup = new()
        {
            XamlRoot = popupContainer.XamlRoot,
            IsLightDismissEnabled = true,
            LightDismissOverlayMode = LightDismissOverlayMode.On,
            ShouldConstrainToRootBounds = false,
            IsOpen = true
        };

        SideDocument sideDocument = new(popupContainer,
                                        popup,
                                        document,
                                        Left.Contains(container),
                                        Top.Contains(container),
                                        Right.Contains(container),
                                        Bottom.Contains(container));

        popup.Child = sideDocument;
        popup.Closed += (_, __) =>
        {
            document.DockWidth = new(sideDocument.ActualWidth, GridUnitType.Pixel);
            document.DockHeight = new(sideDocument.ActualHeight, GridUnitType.Pixel);

            sideDocument.Uninstall();
        };

        popupContainer.Children.Clear();
        popupContainer.Children.Add(popup);
    }
}
