using System.Collections.ObjectModel;

namespace WinUI.Dock;

[ContentProperty(Name = nameof(Layout))]
public partial class DockingManager : Control
{
    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(nameof(Layout),
                                                                                           typeof(DockLayout),
                                                                                           typeof(DockingManager),
                                                                                           new PropertyMetadata(null));

    public static readonly DependencyProperty ActiveDocumentProperty = DependencyProperty.Register(nameof(ActiveDocument),
                                                                                                   typeof(Document),
                                                                                                   typeof(DockingManager),
                                                                                                   new PropertyMetadata(null));

    public DockingManager()
    {
        DefaultStyleKey = typeof(DockingManager);
    }

    public DockLayout? Layout
    {
        get => (DockLayout)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public Document? ActiveDocument
    {
        get => (Document)GetValue(ActiveDocumentProperty);
        set => SetValue(ActiveDocumentProperty, value);
    }

    public ObservableCollection<Document> LeftSide { get; } = [];

    public ObservableCollection<Document> TopSide { get; } = [];

    public ObservableCollection<Document> RightSide { get; } = [];

    public ObservableCollection<Document> BottomSide { get; } = [];
}
