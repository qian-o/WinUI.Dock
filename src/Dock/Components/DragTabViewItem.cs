using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace Dock.Components;

public partial class DragTabViewItem : TabViewItem
{
    public DragTabViewItem()
    {
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;

        AddHandler(PointerPressedEvent, new PointerEventHandler((_, e) =>
        {
            if (e.Pointer.IsInContact)
            {
                IsSelected = true;
            }
        }), true);
    }
}
