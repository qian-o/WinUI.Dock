using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Children))]
public sealed partial class LayoutSide : Control
{
    public LayoutSide()
    {
        DefaultStyleKey = typeof(LayoutSide);
    }

    public ObservableCollection<LayoutAnchorGroup> Children { get; } = [];
}
