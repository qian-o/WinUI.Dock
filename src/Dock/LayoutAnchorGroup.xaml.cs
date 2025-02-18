using Microsoft.UI.Xaml.Controls;

namespace Dock;

public sealed partial class LayoutAnchorGroup : Control, ILayoutGroup
{
    public LayoutAnchorGroup()
    {
        DefaultStyleKey = typeof(LayoutAnchorGroup);
    }
}
