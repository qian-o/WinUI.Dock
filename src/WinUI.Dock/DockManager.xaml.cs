using System.Collections.ObjectModel;

namespace WinUI.Dock;

[ContentProperty(Name = nameof(Panel))]
public partial class DockManager : Control
{
    public static readonly DependencyProperty PanelProperty = DependencyProperty.Register(nameof(Panel),
                                                                                          typeof(LayoutPanel),
                                                                                          typeof(DockManager),
                                                                                          new PropertyMetadata(null));

    public static readonly DependencyProperty ActiveDocumentProperty = DependencyProperty.Register(nameof(ActiveDocument),
                                                                                                   typeof(Document),
                                                                                                   typeof(DockManager),
                                                                                                   new PropertyMetadata(null));

    public DockManager()
    {
        DefaultStyleKey = typeof(DockManager);
    }

    public LayoutPanel? Panel
    {
        get => (LayoutPanel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
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
