using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Children))]
public partial class DockingManager : Control
{
    public DockingManager()
    {
        DefaultStyleKey = typeof(DockingManager);
    }

    public ObservableCollection<Document> Left { get; } = [];

    public ObservableCollection<Document> Top { get; } = [];

    public ObservableCollection<Document> Right { get; } = [];

    public ObservableCollection<Document> Bottom { get; } = [];

    public ObservableCollection<LayoutContainer> Children { get; } = [];
}
