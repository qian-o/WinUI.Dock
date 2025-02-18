using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Children))]
public sealed partial class LayoutPanel : Control
{
    public LayoutPanel()
    {
        DefaultStyleKey = typeof(LayoutPanel);
    }

    public ObservableCollection<Control> Children { get; } = [];
}
