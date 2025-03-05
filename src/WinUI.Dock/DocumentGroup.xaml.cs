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

            Detach(false);

            DocumentGroup group = new();
            group.CopySizeFrom(this);
            group.Children.Add(document);

            root.InvokeDocumentGroupReady(document.Title, group);

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