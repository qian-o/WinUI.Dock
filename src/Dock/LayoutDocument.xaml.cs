using Dock.Abstractions;

namespace Dock;

public sealed partial class LayoutDocument : LayoutItem
{
    public LayoutDocument()
    {
        DefaultStyleKey = typeof(LayoutDocument);
    }
}