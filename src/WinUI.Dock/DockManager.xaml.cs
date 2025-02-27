using System.Collections.ObjectModel;
using WinUI.Dock.Controls;
using WinUI.Dock.Enums;

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

    private readonly Sidebar leftSidebar;
    private readonly Sidebar topSidebar;
    private readonly Sidebar rightSidebar;
    private readonly Sidebar bottomSidebar;

    public DockManager()
    {
        leftSidebar = new(DockSide.Left, this);
        topSidebar = new(DockSide.Top, this);
        rightSidebar = new(DockSide.Right, this);
        bottomSidebar = new(DockSide.Bottom, this);

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

    public ObservableCollection<Document> LeftSide => leftSidebar.Documents;

    public ObservableCollection<Document> TopSide => topSidebar.Documents;

    public ObservableCollection<Document> RightSide => rightSidebar.Documents;

    public ObservableCollection<Document> BottomSide => bottomSidebar.Documents;
}
