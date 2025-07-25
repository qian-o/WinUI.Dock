using System.Text.Json.Nodes;
using Microsoft.UI.Xaml.Data;

namespace WinUI.Dock;

[TemplatePart(Name = "PART_Root", Type = typeof(TabView))]
[TemplatePart(Name = "PART_Preview", Type = typeof(Preview))]
public partial class DocumentGroup : DockContainer
{
    public static readonly DependencyProperty TabPositionProperty = DependencyProperty.Register(nameof(TabPosition),
                                                                                                typeof(TabPosition),
                                                                                                typeof(DocumentGroup),
                                                                                                new PropertyMetadata(TabPosition.Top, OnTabPositionChanged));

    public static readonly DependencyProperty UseCompactTabsProperty = DependencyProperty.Register(nameof(UseCompactTabs),
                                                                                                   typeof(bool),
                                                                                                   typeof(DocumentGroup),
                                                                                                   new PropertyMetadata(false, OnUseCompactTabsChanged));

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex),
                                                                                                  typeof(int),
                                                                                                  typeof(DocumentGroup),
                                                                                                  new PropertyMetadata(-1));

    private TabView? root;
    private Preview? preview;

    public DocumentGroup()
    {
        DefaultStyleKey = typeof(DocumentGroup);
    }

    public TabPosition TabPosition
    {
        get => (TabPosition)GetValue(TabPositionProperty);
        set => SetValue(TabPositionProperty, value);
    }

    public bool UseCompactTabs
    {
        get => (bool)GetValue(UseCompactTabsProperty);
        set => SetValue(UseCompactTabsProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
        base.OnDragEnter(e);

        if (e.DataView.Contains(DragDropHelpers.DocumentId))
        {
            VisualStateManager.GoToState(this, "ShowDockTargets", false);
        }
    }

    protected override void OnDragLeave(DragEventArgs e)
    {
        base.OnDragLeave(e);

        VisualStateManager.GoToState(this, "HideDockTargets", false);
    }

    protected override void InitTemplate()
    {
        root = GetTemplateChild("PART_Root") as TabView;
        preview = GetTemplateChild("PART_Preview") as Preview;

        if (root is not null)
        {
            root.Loaded += (_, _) => UpdateVisualState();
            root.SizeChanged += (_, _) => UpdateTabWidths();
        }
    }

    protected override void LoadChildren()
    {
        if (root is null)
        {
            return;
        }

        foreach (Document document in Children.Cast<Document>())
        {
            DocumentTabItem tabItem = new(document);

            tabItem.SetBinding(BorderBrushProperty, new Binding()
            {
                Source = root,
                Path = new(nameof(BorderBrush))
            });

            root.TabItems.Add(tabItem);
        }

        if (SelectedIndex < 0)
        {
            SelectedIndex = 0;
        }
        else if (SelectedIndex >= Children.Count)
        {
            SelectedIndex = Children.Count - 1;
        }

        UpdateVisualState();
        UpdateTabWidths();
    }

    protected override void UnloadChildren()
    {
        if (root is null)
        {
            return;
        }

        foreach (DocumentTabItem tabItem in root.TabItems.Cast<DocumentTabItem>())
        {
            tabItem.Detach();
        }

        root.TabItems.Clear();
    }

    protected override bool ValidateChildren()
    {
        return Children.All(static item => item is Document);
    }

    protected override void OnRootChanged(DockManager? oldRoot, DockManager? newRoot)
    {
        base.OnRootChanged(oldRoot, newRoot);

        if (oldRoot is not null)
        {
            oldRoot.ActiveDocumentChanged -= OnActiveDocumentChanged;
        }

        if (newRoot is not null)
        {
            newRoot.ActiveDocumentChanged += OnActiveDocumentChanged;
        }
    }

    internal void ShowDockPreview(DockTarget dockTarget)
    {
        switch (dockTarget)
        {
            case DockTarget.Center:
                preview?.Show(double.NaN,
                              double.NaN,
                              HorizontalAlignment.Stretch,
                              VerticalAlignment.Stretch);
                break;
            case DockTarget.SplitLeft:
                preview?.Show(ActualWidth / 2,
                              double.NaN,
                              HorizontalAlignment.Left,
                              VerticalAlignment.Stretch);
                break;
            case DockTarget.SplitTop:
                preview?.Show(double.NaN,
                              ActualHeight / 2,
                              HorizontalAlignment.Stretch,
                              VerticalAlignment.Top);
                break;
            case DockTarget.SplitRight:
                preview?.Show(ActualWidth / 2,
                              double.NaN,
                              HorizontalAlignment.Right,
                              VerticalAlignment.Stretch);
                break;
            case DockTarget.SplitBottom:
                preview?.Show(double.NaN,
                              ActualHeight / 2,
                              HorizontalAlignment.Stretch,
                              VerticalAlignment.Bottom);
                break;
        }
    }

    internal void HideDockPreview()
    {
        preview?.Hide();
    }

    internal void Dock(Document document, DockTarget dockTarget)
    {
        VisualStateManager.GoToState(this, "HideDockTargets", false);

        if (document.Owner == this && Children.Count is 1)
        {
            return;
        }

        document.Detach();

        if (dockTarget is DockTarget.Center)
        {
            Children.Add(document);

            SelectedIndex = Children.Count - 1;
        }
        else
        {
            LayoutPanel owner = (LayoutPanel)Owner!;
            DockManager root = owner.Root!;

            int index = owner.Children.IndexOf(this);

            DocumentGroup group = new();
            group.CopySizeFrom(this);
            group.Children.Add(document);

            root.InvokeNewGroup(document.Title, group);

            if ((dockTarget is DockTarget.SplitLeft or DockTarget.SplitRight && owner.Orientation is Orientation.Horizontal)
                || (dockTarget is DockTarget.SplitTop or DockTarget.SplitBottom && owner.Orientation is Orientation.Vertical))
            {
                switch (dockTarget)
                {
                    case DockTarget.SplitLeft or DockTarget.SplitTop:
                        {
                            owner.Children.Insert(index, group);
                        }
                        break;
                    case DockTarget.SplitRight or DockTarget.SplitBottom:
                        {
                            owner.Children.Insert(index + 1, group);
                        }
                        break;
                }
            }
            else
            {
                Detach(false);

                LayoutPanel panel = new();
                panel.CopySizeFrom(this);
                panel.Children.Add(group);

                switch (dockTarget)
                {
                    case DockTarget.SplitLeft:
                        {
                            panel.Orientation = Orientation.Horizontal;

                            panel.Children.Add(this);
                        }
                        break;
                    case DockTarget.SplitTop:
                        {
                            panel.Orientation = Orientation.Vertical;

                            panel.Children.Add(this);
                        }
                        break;
                    case DockTarget.SplitRight:
                        {
                            panel.Orientation = Orientation.Horizontal;

                            panel.Children.Insert(0, this);
                        }
                        break;
                    case DockTarget.SplitBottom:
                        {
                            panel.Orientation = Orientation.Vertical;

                            panel.Children.Insert(0, this);
                        }
                        break;
                }

                owner.Children.Insert(index, panel);
            }
        }
    }

    internal override void SaveLayout(JsonObject writer)
    {
        writer.WriteByModuleType(this);
        writer.WriteDockModuleProperties(this);
        writer.WriteDockContainerChildren(this);

        writer[nameof(TabPosition)] = (int)TabPosition;
        writer[nameof(UseCompactTabs)] = UseCompactTabs;
        writer[nameof(SelectedIndex)] = SelectedIndex;
    }

    internal override void LoadLayout(JsonObject reader)
    {
        reader.ReadDockModuleProperties(this);
        reader.ReadDockContainerChildren(this);

        TabPosition = (TabPosition)reader[nameof(TabPosition)].Deserialize<int>();
        UseCompactTabs = reader[nameof(UseCompactTabs)].Deserialize<bool>();
        SelectedIndex = reader[nameof(SelectedIndex)].Deserialize<int>();
    }

    private void UpdateVisualState()
    {
        if (Root?.ActiveDocument is not null && Children.IndexOf(Root.ActiveDocument) is int index && index is not -1)
        {
            SelectedIndex = index;

            VisualStateManager.GoToState(this, "Active", false);
        }
        else
        {
            VisualStateManager.GoToState(this, "Inactive", false);
        }

        VisualStateManager.GoToState(this, TabPosition.ToString(), false);

        if (root is null)
        {
            return;
        }

        VisualStateManager.GoToState(root, TabPosition is TabPosition.Bottom && Children.Count is 1 ? "SingleView" : "MultiView", false);

        foreach (DocumentTabItem tabItem in root.TabItems.Cast<DocumentTabItem>())
        {
            tabItem.UpdateVisualState(TabPosition);
        }
    }

    private void UpdateTabWidths()
    {
        if (root is null)
        {
            return;
        }

        if (UseCompactTabs)
        {
            foreach (DocumentTabItem tabItem in root.TabItems.Cast<DocumentTabItem>())
            {
                tabItem.TabWidth = double.NaN;
            }
        }
        else
        {
            const double minTabWidth = 48.0;
            const double maxTabWidth = 200.0;

            double tabWidth = Math.Clamp(root.ActualWidth / Children.Count, minTabWidth, maxTabWidth);

            foreach (DocumentTabItem tabItem in root.TabItems.Cast<DocumentTabItem>())
            {
                tabItem.TabWidth = tabWidth;
            }
        }
    }

    private void OnActiveDocumentChanged(object? sender, ActiveDocumentChangedEventArgs e)
    {
        UpdateVisualState();
    }

    private static void OnTabPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DocumentGroup)d).UpdateVisualState();
    }

    private static void OnUseCompactTabsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DocumentGroup)d).UpdateTabWidths();
    }
}