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