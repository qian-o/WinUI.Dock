using System.Collections.ObjectModel;
using Dock.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Children))]
public sealed partial class LayoutSide : Control
{
    public static readonly DependencyProperty SideProperty = DependencyProperty.Register(nameof(Side),
                                                                                         typeof(Side),
                                                                                         typeof(LayoutSide),
                                                                                         new PropertyMetadata(Side.Left));

    public LayoutSide()
    {
        DefaultStyleKey = typeof(LayoutSide);
    }

    public Side Side
    {
        get => (Side)GetValue(SideProperty);
        internal set => SetValue(SideProperty, value);
    }

    public ObservableCollection<LayoutAnchorGroup> Children { get; } = [];
}
