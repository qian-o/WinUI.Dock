using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace WinUI.Dock;

[ContentProperty(Name = nameof(Panel))]
[TemplatePart(Name = "PART_PopupContainer", Type = typeof(Border))]
public partial class DockManager : Control
{
    public static readonly DependencyProperty PanelProperty = DependencyProperty.Register(nameof(Panel),
                                                                                          typeof(LayoutPanel),
                                                                                          typeof(DockManager),
                                                                                          new PropertyMetadata(null, OnPanelChanged));

    public static readonly DependencyProperty ActiveDocumentProperty = DependencyProperty.Register(nameof(ActiveDocument),
                                                                                                   typeof(Document),
                                                                                                   typeof(DockManager),
                                                                                                   new PropertyMetadata(null));

    public DockManager()
    {
        DefaultStyleKey = typeof(DockManager);

        LeftSide.CollectionChanged += OnSideCollectionChanged;
        TopSide.CollectionChanged += OnSideCollectionChanged;
        RightSide.CollectionChanged += OnSideCollectionChanged;
        BottomSide.CollectionChanged += OnSideCollectionChanged;
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

    public Border? PopupContainer { get; private set; }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PopupContainer = GetTemplateChild("PART_PopupContainer") as Border;
    }

    private void OnSideCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (Document document in e.OldItems)
            {
                document.Root = null;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (Document document in e.NewItems)
            {
                document.Root = this;
            }
        }
    }

    private static void OnPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DockManager dockManager)
        {
            if (e.OldValue is LayoutPanel oldPanel)
            {
                oldPanel.Root = null;
            }

            if (e.NewValue is LayoutPanel newPanel)
            {
                newPanel.Root = dockManager;
            }
        }
    }
}
