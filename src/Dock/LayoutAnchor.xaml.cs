using Dock.Abstractions;

namespace Dock;

public sealed partial class LayoutAnchor : LayoutItem
{
    public LayoutAnchor()
    {
        DefaultStyleKey = typeof(LayoutAnchor);
    }
}
