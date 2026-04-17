using System.ComponentModel;
using System.Numerics;
using System.Text.Json.Nodes;
using Microsoft.UI.Xaml.Hosting;

namespace WinUI.Dock;

[TemplatePart(Name = "PART_Root", Type = typeof(TabView))]
[TemplatePart(Name = "PART_Preview", Type = typeof(Preview))]
public partial class DocumentGroup : DockContainer
{
    public static readonly DependencyProperty TabPositionProperty = DependencyProperty.Register(nameof(TabPosition),
                                                                                                typeof(TabPosition),
                                                                                                typeof(DocumentGroup),
                                                                                                new PropertyMetadata(TabPosition.Top, OnTabPositionChanged));

    public static readonly DependencyProperty CompactTabsProperty = DependencyProperty.Register(nameof(CompactTabs),
                                                                                                typeof(bool),
                                                                                                typeof(DocumentGroup),
                                                                                                new PropertyMetadata(false, OnCompactTabsChanged));

    public static readonly DependencyProperty ShowWhenEmptyProperty = DependencyProperty.Register(nameof(ShowWhenEmpty),
                                                                                                  typeof(bool),
                                                                                                  typeof(DocumentGroup),
                                                                                                  new PropertyMetadata(false));

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

    public new LayoutPanel? Owner
    {
        get => (LayoutPanel)GetValue(OwnerProperty);
        internal set => SetValue(OwnerProperty, value);
    }

    public TabPosition TabPosition
    {
        get => (TabPosition)GetValue(TabPositionProperty);
        set => SetValue(TabPositionProperty, value);
    }

    public bool CompactTabs
    {
        get => (bool)GetValue(CompactTabsProperty);
        set => SetValue(CompactTabsProperty, value);
    }

    public bool ShowWhenEmpty
    {
        get => (bool)GetValue(ShowWhenEmptyProperty);
        set => SetValue(ShowWhenEmptyProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public new double MinWidth
    {
        get => base.MinWidth;
        set => base.MinWidth = value;
    }

    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public new double Width
    {
        get => base.Width;
        set => base.Width = value;
    }

    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public new double MaxWidth
    {
        get => base.MaxWidth;
        set => base.MaxWidth = value;
    }

    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public new double MinHeight
    {
        get => base.MinHeight;
        set => base.MinHeight = value;
    }

    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public new double Height
    {
        get => base.Height;
        set => base.Height = value;
    }

    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public new double MaxHeight
    {
        get => base.MaxHeight;
        set => base.MaxHeight = value;
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

    protected override void InitChildren()
    {
        if (root is null)
        {
            return;
        }

        root.TabItems.Clear();

        foreach (Document document in Children.Cast<Document>())
        {
            root.TabItems.Add(new DockTabItem(root, document));
        }

        if (Children.Count is not 0)
        {
            SelectedIndex = 0;
        }

        UpdateVisualState();
        UpdateTabWidths();
    }

    protected override void SynchronizeChildren(DockModule[] oldChildren,
                                                int oldStartingIndex,
                                                DockModule[] newChildren,
                                                int newStartingIndex)
    {
        if (root is null)
        {
            return;
        }

        // Detect Move operation: same single item repositioned — reuse existing DockTabItem.
        if (oldChildren.Length is 1
            && newChildren.Length is 1
            && oldChildren[0] == newChildren[0])
        {
            int from = oldStartingIndex;
            int to = newStartingIndex;

            // Measure the width of the tab being moved BEFORE the move.
            double movedWidth = root.TabItems[from] is DockTabItem srcTab ? srcTab.ActualWidth : 0;

            object tabItem = root.TabItems[from];

            root.TabItems.RemoveAt(from);
            root.TabItems.Insert(to, tabItem);

            SelectedIndex = to;

            // Animate displaced tabs using composition FLIP animation.
            if (movedWidth > 0)
            {
                int min = Math.Min(from, to);
                int max = Math.Max(from, to);
                float startOffset = from < to ? (float)movedWidth : -(float)movedWidth;

                for (int i = min; i <= max; i++)
                {
                    if (i == to)
                    {
                        continue;
                    }

                    if (root.TabItems[i] is UIElement element)
                    {
                        AnimateTabTranslation(element, startOffset);
                    }
                }
            }

            return;
        }

        for (int i = 0; i < oldChildren.Length; i++)
        {
            DockTabItem tabItem = (DockTabItem)root.TabItems[oldStartingIndex++];

            tabItem.Detach();

            root.TabItems.Remove(tabItem);
        }

        foreach (Document document in newChildren.Cast<Document>())
        {
            root.TabItems.Insert(newStartingIndex++, new DockTabItem(root, document));
        }

        UpdateVisualState();
        UpdateTabWidths();
    }

    protected override bool ValidateChildren()
    {
        return Children.All(static item => item is Document);
    }

    protected override bool ConfirmEmptyContainer()
    {
        return !ShowWhenEmpty;
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

    internal void ShowDockTargets()
    {
        VisualStateManager.GoToState(this, "ShowDockTargets", false);
    }

    internal void HideDockTargets()
    {
        VisualStateManager.GoToState(this, "HideDockTargets", false);
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

        if (dockTarget is DockTarget.Center)
        {
            Children.Add(document);

            SelectedIndex = Children.Count - 1;
        }
        else
        {
            int index = Owner!.Children.IndexOf(this);

            DocumentGroup group = new();
            group.Children.Add(document);

            group.CopyDimensions(document);

            Root!.Adapter?.OnCreated(group, document);

            if ((dockTarget is DockTarget.SplitLeft or DockTarget.SplitRight && Owner.Orientation is Orientation.Horizontal)
                || (dockTarget is DockTarget.SplitTop or DockTarget.SplitBottom && Owner.Orientation is Orientation.Vertical))
            {
                double currentDockSize = double.IsNaN(DockSize) ? 1.0 : DockSize;
                group.DockSize = currentDockSize / 2.0;
                DockSize = currentDockSize / 2.0;

                switch (dockTarget)
                {
                    case DockTarget.SplitLeft or DockTarget.SplitTop:
                        {
                            Owner.Children.Insert(index, group);
                        }
                        break;
                    case DockTarget.SplitRight or DockTarget.SplitBottom:
                        {
                            Owner.Children.Insert(index + 1, group);
                        }
                        break;
                }
            }
            else
            {
                LayoutPanel actualOwner = Owner;

                Detach(false);

                LayoutPanel panel = new();
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

                panel.CopyDimensions(this);

                actualOwner.Children.Insert(index, panel);
            }
        }

        Root!.Behavior?.OnDocked(document, this, dockTarget);
    }

    internal override void SaveLayout(JsonObject writer)
    {
        writer.WriteByModuleType(this);
        writer.WriteDockModuleProperties(this);
        writer.WriteDockContainerChildren(this);

        writer[nameof(TabPosition)] = (int)TabPosition;
        writer[nameof(CompactTabs)] = CompactTabs;
        writer[nameof(ShowWhenEmpty)] = ShowWhenEmpty;
        writer[nameof(SelectedIndex)] = SelectedIndex;
    }

    internal override void LoadLayout(JsonObject reader)
    {
        reader.ReadDockModuleProperties(this);
        reader.ReadDockContainerChildren(this);

        TabPosition = (TabPosition)reader[nameof(TabPosition)].Deserialize<int>();
        CompactTabs = reader[nameof(CompactTabs)].Deserialize<bool>();
        ShowWhenEmpty = reader[nameof(ShowWhenEmpty)].Deserialize<bool>();
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

        foreach (DockTabItem tabItem in root.TabItems.Cast<DockTabItem>())
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

        // The two constants are hardcoded in WinUI.Dock/Themes/Styles.xaml.
        // They need to be kept consistent.
        const double tabViewContainerLeftColumnWidth = 6.0;
        const double tabItemRadiusWidth = 3.0;

        double availableWidth = root.ActualWidth - tabViewContainerLeftColumnWidth - (tabItemRadiusWidth * 2.0 * Children.Count);

        double tabWidth = Math.Clamp(availableWidth / Children.Count, 0.0, 200.0);

        foreach (DockTabItem tabItem in root.TabItems.Cast<DockTabItem>())
        {
            tabItem.TabWidth = CompactTabs ? double.NaN : tabWidth;
            tabItem.TabMaxWidth = tabWidth;
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

    private static void OnCompactTabsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DocumentGroup)d).UpdateTabWidths();
    }

    private static void AnimateTabTranslation(UIElement element, float fromOffsetX)
    {
        ElementCompositionPreview.SetIsTranslationEnabled(element, true);

        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        var animation = compositor.CreateVector3KeyFrameAnimation();
        animation.InsertKeyFrame(0f, new Vector3(fromOffsetX, 0, 0));
        animation.InsertKeyFrame(1f, Vector3.Zero);
        animation.Duration = TimeSpan.FromMilliseconds(150);

        visual.StartAnimation("Translation", animation);
    }

    internal double GetActualTabItemWidth()
    {
        if (root is null || root.TabItems.Count is 0)
        {
            return 0;
        }

        if (root.TabItems[0] is DockTabItem tab)
        {
            return tab.ActualWidth;
        }

        return 0;
    }
}