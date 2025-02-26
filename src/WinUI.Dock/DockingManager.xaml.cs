using System.Collections.ObjectModel;

namespace WinUI.Dock;

[ContentProperty(Name = nameof(Panel))]
public partial class DockingManager : Control
{
    public static readonly DependencyProperty PanelProperty = DependencyProperty.Register(nameof(Panel),
                                                                                          typeof(SplitPanel),
                                                                                          typeof(DockingManager),
                                                                                          new PropertyMetadata(null));

    public DockingManager()
    {
        DefaultStyleKey = typeof(DockingManager);
    }

    public SplitPanel? Panel
    {
        get => (SplitPanel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    public ObservableCollection<Document> LeftSide { get; } = [];

    public ObservableCollection<Document> TopSide { get; } = [];

    public ObservableCollection<Document> RightSide { get; } = [];

    public ObservableCollection<Document> BottomSide { get; } = [];
}
