using System.Collections.ObjectModel;
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
                                                                                              new PropertyMetadata(null));

    public DockingManager()
    {
        DefaultStyleKey = typeof(DockingManager);
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
}
