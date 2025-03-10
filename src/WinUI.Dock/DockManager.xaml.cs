using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using WinUI.Dock.Enums;
using WinUI.Dock.Helpers;

namespace WinUI.Dock;

public record CreateNewDocumentEventArgs(string Title, Document Document);

public record CreateNewGroupEventArgs(string Title, DocumentGroup Group);

public record CreateNewWindowEventArgs(Border TitleBar);

[ContentProperty(Name = nameof(Panel))]
[TemplatePart(Name = "PART_PopupContainer", Type = typeof(Border))]
[TemplatePart(Name = "PART_DockPreview", Type = typeof(Border))]
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

    public static readonly DependencyProperty ParentWindowProperty = DependencyProperty.Register(nameof(ParentWindow),
                                                                                                 typeof(Window),
                                                                                                 typeof(DockManager),
                                                                                                 new PropertyMetadata(null));

    private Border? dockPreview;

    public DockManager()
    {
        DefaultStyleKey = typeof(DockManager);

        Unloaded += (_, _) => DockWindowHelpers.CloseAllWindows(this);

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

    public Window? ParentWindow
    {
        get => (Window)GetValue(ParentWindowProperty);
        set => SetValue(ParentWindowProperty, value);
    }

    public ObservableCollection<Document> LeftSide { get; } = [];

    public ObservableCollection<Document> TopSide { get; } = [];

    public ObservableCollection<Document> RightSide { get; } = [];

    public ObservableCollection<Document> BottomSide { get; } = [];

    public Border? PopupContainer { get; private set; }

    public event EventHandler<CreateNewGroupEventArgs>? CreateNewGroup;

    public event EventHandler<CreateNewWindowEventArgs>? CreateNewWindow;

    public string SaveLayout()
    {
        JsonObject writer = [];

        if (Panel is not null)
        {
            JsonObject panelWriter = [];

            Panel.SaveLayout(panelWriter);

            writer[nameof(Panel)] = panelWriter;
        }

        writer.WriteSideDocuments(LeftSide, nameof(LeftSide));
        writer.WriteSideDocuments(TopSide, nameof(TopSide));
        writer.WriteSideDocuments(RightSide, nameof(RightSide));
        writer.WriteSideDocuments(BottomSide, nameof(BottomSide));

        return writer.ToString();
    }

    public void LoadLayout(string layout)
    {
        if (string.IsNullOrEmpty(layout))
        {
            return;
        }

        using JsonDocument document = JsonDocument.Parse(layout);

        JsonObject reader = JsonObject.Create(document.RootElement)!;

        if (reader.ContainsKey(nameof(Panel)))
        {
            Panel = new() { Root = this };
            Panel.LoadLayout(reader[nameof(Panel)]!.AsObject());
        }

        LeftSide.Clear();
        reader.ReadSideDocuments(LeftSide, nameof(LeftSide));

        TopSide.Clear();
        reader.ReadSideDocuments(TopSide, nameof(TopSide));

        RightSide.Clear();
        reader.ReadSideDocuments(RightSide, nameof(RightSide));

        BottomSide.Clear();
        reader.ReadSideDocuments(BottomSide, nameof(BottomSide));
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PopupContainer = GetTemplateChild("PART_PopupContainer") as Border;

        dockPreview = GetTemplateChild("PART_DockPreview") as Border;
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
        base.OnDragEnter(e);

        ParentWindow?.Activate();

        if (e.DataView.Contains(DragDropHelpers.FormatId))
        {
            VisualStateManager.GoToState(this, Panel is null || Panel.Children.Count is 0 ? "ShowAllDockTargets" : "ShowSideDockTargets", false);
        }
    }

    protected override void OnDragLeave(DragEventArgs e)
    {
        base.OnDragLeave(e);

        VisualStateManager.GoToState(this, "HideDockTargets", false);
    }

    internal void ShowDockPreview(Document document, DockTarget dockTarget)
    {
        if (dockPreview is null)
        {
            return;
        }

        dockPreview.Visibility = Visibility.Visible;

        switch (dockTarget)
        {
            case DockTarget.Center:
                {
                    dockPreview.Width = double.NaN;
                    dockPreview.Height = double.NaN;
                    dockPreview.HorizontalAlignment = HorizontalAlignment.Stretch;
                    dockPreview.VerticalAlignment = VerticalAlignment.Stretch;
                }
                break;
            case DockTarget.DockLeft:
                {
                    dockPreview.Width = Panel!.CalculateWidth(document, false);
                    dockPreview.Height = double.NaN;
                    dockPreview.HorizontalAlignment = HorizontalAlignment.Left;
                    dockPreview.VerticalAlignment = VerticalAlignment.Stretch;
                }
                break;
            case DockTarget.DockTop:
                {
                    dockPreview.Width = double.NaN;
                    dockPreview.Height = Panel!.CalculateHeight(document, false);
                    dockPreview.HorizontalAlignment = HorizontalAlignment.Stretch;
                    dockPreview.VerticalAlignment = VerticalAlignment.Top;
                }
                break;
            case DockTarget.DockRight:
                {
                    dockPreview.Width = Panel!.CalculateWidth(document, false);
                    dockPreview.Height = double.NaN;
                    dockPreview.HorizontalAlignment = HorizontalAlignment.Right;
                    dockPreview.VerticalAlignment = VerticalAlignment.Stretch;
                }
                break;
            case DockTarget.DockBottom:
                {
                    dockPreview.Width = double.NaN;
                    dockPreview.Height = Panel!.CalculateHeight(document, false);
                    dockPreview.HorizontalAlignment = HorizontalAlignment.Stretch;
                    dockPreview.VerticalAlignment = VerticalAlignment.Bottom;
                }
                break;
        }
    }

    internal void HideDockPreview()
    {
        if (dockPreview is null)
        {
            return;
        }

        dockPreview.Visibility = Visibility.Collapsed;
    }

    internal void Dock(Document document, DockTarget target)
    {
        VisualStateManager.GoToState(this, "HideDockTargets", false);

        document.Detach();

        DocumentGroup group = new();
        group.CopySizeFrom(document);
        group.Children.Add(document);

        CreateNewGroup?.Invoke(this, new CreateNewGroupEventArgs(document.Title, group));

        LayoutPanel panel = new();
        panel.Children.Add(group);

        switch (target)
        {
            case DockTarget.DockLeft:
                {
                    panel.Orientation = Orientation.Horizontal;

                    if (Panel is not null)
                    {
                        panel.Children.Add(Panel);
                    }
                }
                break;
            case DockTarget.DockTop:
                {
                    panel.Orientation = Orientation.Vertical;

                    if (Panel is not null)
                    {
                        panel.Children.Add(Panel);
                    }
                }
                break;
            case DockTarget.DockRight:
                {
                    panel.Orientation = Orientation.Horizontal;

                    if (Panel is not null)
                    {
                        panel.Children.Insert(0, Panel);
                    }
                }
                break;
            case DockTarget.DockBottom:
                {
                    panel.Orientation = Orientation.Vertical;

                    if (Panel is not null)
                    {
                        panel.Children.Insert(0, Panel);
                    }
                }
                break;
        }

        Panel = panel;
    }

    internal void HideDockTargets()
    {
        VisualStateManager.GoToState(this, "HideDockTargets", false);
    }

    internal void InvokeCreateNewGroup(string title, DocumentGroup group)
    {
        CreateNewGroup?.Invoke(this, new CreateNewGroupEventArgs(title, group));
    }

    internal void InvokeCreateNewWindow(Border titleBar)
    {
        CreateNewWindow?.Invoke(this, new CreateNewWindowEventArgs(titleBar));
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
        if (d is DockManager manager)
        {
            if (e.OldValue is LayoutPanel oldPanel)
            {
                oldPanel.Root = null;
            }

            if (e.NewValue is LayoutPanel newPanel)
            {
                newPanel.Root = manager;
            }
        }
    }
}
