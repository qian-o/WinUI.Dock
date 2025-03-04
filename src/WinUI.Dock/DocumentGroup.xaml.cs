using WinUI.Dock.Abstracts;
using WinUI.Dock.Controls;
using WinUI.Dock.Enums;

namespace WinUI.Dock;

[TemplatePart(Name = "PART_Root", Type = typeof(TabView))]
public partial class DocumentGroup : DockContainer
{
    public static readonly DependencyProperty TabPositionProperty = DependencyProperty.Register(nameof(TabPosition),
                                                                                                typeof(TabPosition),
                                                                                                typeof(DocumentGroup),
                                                                                                new PropertyMetadata(TabPosition.Top, OnTabPositionChanged));

    public static readonly DependencyProperty IsTabWidthBasedOnContentProperty = DependencyProperty.Register(nameof(IsTabWidthBasedOnContent),
                                                                                                             typeof(bool),
                                                                                                             typeof(DocumentGroup),
                                                                                                             new PropertyMetadata(false, OnIsTabWidthBasedOnContentChanged));

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex),
                                                                                                  typeof(int),
                                                                                                  typeof(DocumentGroup),
                                                                                                  new PropertyMetadata(-1));

    private TabView? root;

    public DocumentGroup()
    {
        DefaultStyleKey = typeof(DocumentGroup);
    }

    public TabPosition TabPosition
    {
        get => (TabPosition)GetValue(TabPositionProperty);
        set => SetValue(TabPositionProperty, value);
    }

    public bool IsTabWidthBasedOnContent
    {
        get => (bool)GetValue(IsTabWidthBasedOnContentProperty);
        set => SetValue(IsTabWidthBasedOnContentProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
        base.OnDragEnter(e);

        VisualStateManager.GoToState(this, "ShowDockTargets", false);
    }

    protected override void OnDragLeave(DragEventArgs e)
    {
        base.OnDragLeave(e);

        VisualStateManager.GoToState(this, "HideDockTargets", false);
    }

    protected override void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);

        VisualStateManager.GoToState(this, "HideDockTargets", false);
    }

    protected override void InitTemplate()
    {
        root = GetTemplateChild("PART_Root") as TabView;

        UpdateVisualState();
    }

    protected override void LoadChildren()
    {
        if (root is null)
        {
            return;
        }

        foreach (Document document in Children.Cast<Document>())
        {
            root.TabItems.Add(new DocumentTabItem(TabPosition, document));
        }

        if (SelectedIndex < 0)
        {
            SelectedIndex = 0;
        }
        else if (SelectedIndex >= Children.Count)
        {
            SelectedIndex = Children.Count - 1;
        }
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

    internal void Dock(Document document, DockTarget dockTarget)
    {
    }

    private void UpdateVisualState()
    {
        VisualStateManager.GoToState(this, TabPosition.ToString(), false);
        VisualStateManager.GoToState(this, IsTabWidthBasedOnContent ? "TabWidthSizeToContent" : "TabWidthEqual", false);

        if (root is null)
        {
            return;
        }

        foreach (DocumentTabItem tabItem in root.TabItems.Cast<DocumentTabItem>())
        {
            tabItem.UpdateTabPosition(TabPosition);
        }
    }

    private static void OnTabPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DocumentGroup documentGroup)
        {
            documentGroup.UpdateVisualState();
        }
    }

    private static void OnIsTabWidthBasedOnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DocumentGroup documentGroup)
        {
            documentGroup.UpdateVisualState();
        }
    }
}