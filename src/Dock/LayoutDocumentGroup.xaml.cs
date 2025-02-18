using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Children))]
public sealed partial class LayoutDocumentGroup : Control
{
    public LayoutDocumentGroup()
    {
        DefaultStyleKey = typeof(LayoutDocumentGroup);
    }

    public ObservableCollection<LayoutDocument> Children { get; } = [];
}
