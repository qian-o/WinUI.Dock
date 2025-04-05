using System.Text.Json;
using System.Text.Json.Nodes;
using WinUI.Dock.Abstracts;
using WinUI.Dock.Controls;
using WinUI.Dock.Enums;
using WinUI.Dock.Helpers;

namespace WinUI.Dock;

[TemplatePart(Name = "PART_Root", Type = typeof(TabView))]
[TemplatePart(Name = "PART_Preview", Type = typeof(AnimationPreview))]
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
    private AnimationPreview? preview;

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

        if (e.DataView.Contains(DragDropHelpers.FormatId))
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
        preview = GetTemplateChild("PART_Preview") as AnimationPreview;

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

        UpdateActiveDocumentStyle();
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
        if (preview is null)
        {
            return;
        }

        preview.Visibility = Visibility.Visible;

        switch (dockTarget)
        {
            case DockTarget.Center:
                preview.Show(double.NaN,
                             double.NaN,
                             HorizontalAlignment.Stretch,
                             VerticalAlignment.Stretch);
                break;
            case DockTarget.SplitLeft:
                preview.Show(ActualWidth / 2,
                             double.NaN,
                             HorizontalAlignment.Left,
                             VerticalAlignment.Stretch);
                break;
            case DockTarget.SplitTop:
                preview.Show(double.NaN,
                             ActualHeight / 2,
                             HorizontalAlignment.Stretch,
                             VerticalAlignment.Top);
                break;
            case DockTarget.SplitRight:
                preview.Show(ActualWidth / 2,
                             double.NaN,
                             HorizontalAlignment.Right,
                             VerticalAlignment.Stretch);
                break;
            case DockTarget.SplitBottom:
                preview.Show(double.NaN,
                             ActualHeight / 2,
                             HorizontalAlignment.Stretch,
                             VerticalAlignment.Bottom);
                break;
        }
    }

    internal void HideDockPreview()
    {
        if (preview is null)
        {
            return;
        }

        preview.Visibility = Visibility.Collapsed;
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

            root.InvokeCreateNewGroup(document.Title, group);

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

    internal void TryReorder(DocumentTabItem tabItem, Document document)
    {
        VisualStateManager.GoToState(this, "HideDockTargets", false);

        if (Children.Count is 1)
        {
            return;
        }

        int index1 = root!.TabItems.IndexOf(tabItem);
        int index2 = Children.IndexOf(document);

        if (index1 != index2)
        {
            IsListening = false;

            Children.Move(index2, index1);

            IsListening = true;
        }
    }

    internal override void SaveLayout(JsonObject writer)
    {
        writer.WriteByModuleType(this);
        writer.WriteDockModuleProperties(this);
        writer.WriteDockContainerChildren(this);

        writer[nameof(TabPosition)] = (int)TabPosition;
        writer[nameof(IsTabWidthBasedOnContent)] = IsTabWidthBasedOnContent;
        writer[nameof(SelectedIndex)] = SelectedIndex;
    }

    internal override void LoadLayout(JsonObject reader)
    {
        reader.ReadDockModuleProperties(this);
        reader.ReadDockContainerChildren(this);

        TabPosition = (TabPosition)reader[nameof(TabPosition)].Deserialize<int>(LayoutHelpers.SerializerOptions);
        IsTabWidthBasedOnContent = reader[nameof(IsTabWidthBasedOnContent)].Deserialize<bool>(LayoutHelpers.SerializerOptions);
        SelectedIndex = reader[nameof(SelectedIndex)].Deserialize<int>(LayoutHelpers.SerializerOptions);
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

    private void UpdateActiveDocumentStyle()
    {
        if (root is null)
        {
            return;
        }

        if (Root!.ActiveDocument is not null && Children.IndexOf(Root.ActiveDocument) is int index && index is not -1)
        {
            SelectedIndex = index;

            ResourceDictionary activeResources = root.Resources.MergedDictionaries.First(static item => item.Source!.ToString().Contains("TabViewActiveResources"));

            root.Resources.MergedDictionaries.Remove(activeResources);
            root.Resources.MergedDictionaries.Add(activeResources);
        }
        else
        {
            ResourceDictionary defaultResources = root.Resources.MergedDictionaries.First(static item => item.Source!.ToString().Contains("TabViewDefaultResources"));

            root.Resources.MergedDictionaries.Remove(defaultResources);
            root.Resources.MergedDictionaries.Add(defaultResources);
        }

        // For now, this is a workaround.
        root.RequestedTheme = Application.Current.RequestedTheme is ApplicationTheme.Light ? ElementTheme.Dark : ElementTheme.Light;
        root.RequestedTheme = ElementTheme.Default;
    }

    private void OnActiveDocumentChanged(object? sender, ActiveDocumentChangedEventArgs e)
    {
        UpdateActiveDocumentStyle();
    }

    private static void OnTabPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DocumentGroup group)
        {
            group.UpdateVisualState();
        }
    }

    private static void OnIsTabWidthBasedOnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DocumentGroup group)
        {
            group.UpdateVisualState();
        }
    }
}