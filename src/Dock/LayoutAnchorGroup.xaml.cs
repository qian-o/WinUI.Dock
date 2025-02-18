using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Children))]
public sealed partial class LayoutAnchorGroup : Control
{
    public LayoutAnchorGroup()
    {
        DefaultStyleKey = typeof(LayoutAnchorGroup);
    }

    public ObservableCollection<LayoutAnchor> Children { get; } = [];
}
