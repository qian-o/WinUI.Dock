using Microsoft.UI.Xaml.Controls;

namespace Dock;

public sealed partial class LayoutDocumentGroup : Control, ILayoutGroup
{
    public LayoutDocumentGroup()
    {
        DefaultStyleKey = typeof(LayoutDocumentGroup);
    }
}
