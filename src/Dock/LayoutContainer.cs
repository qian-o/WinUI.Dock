using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dock;

public partial class LayoutContainer : Control
{
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation),
                                                                                                typeof(Orientation),
                                                                                                typeof(LayoutContainer),
                                                                                                new PropertyMetadata(Orientation.Horizontal));

    public LayoutContainer()
    {
        DefaultStyleKey = typeof(LayoutContainer);
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public ObservableCollection<DocumentContainer> Children { get; } = [];
}
