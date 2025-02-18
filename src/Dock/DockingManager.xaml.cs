using Microsoft.UI.Xaml.Controls;

namespace Dock;

public sealed partial class DockingManager : Control
{
    // ILayoutGroup

    // LayoutDocument - 代表一个 Document
    // LayoutDocumentGroup - 代表一组 LayoutDocument

    // LayoutAnchorable - 代表一个 Anchorable
    // LayoutAnchorGroup - 代表一组 LayoutAnchorable

    // LayoutSide - Left, Top, Right, Bottom

    // LayoutPanel - 代表中心区域的 Panel (Document, Anchorable)
    // LayoutPanel.Children - ILayoutGroup[] 使用 DockPanel 布局

    public DockingManager()
    {
        DefaultStyleKey = typeof(DockingManager);
    }
}
