﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WinUI.Dock.Enums;

namespace WinUI.Dock;

public record CreateNewWindowEventArgs(Document Document, Border TitleBar);

public record DocumentGroupReadyEventArgs(string DocumentTitle, DocumentGroup DocumentGroup);

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

    public static readonly DependencyProperty ParentWindowProperty = DependencyProperty.Register(nameof(ParentWindow),
                                                                                                 typeof(Window),
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

    public event EventHandler<CreateNewWindowEventArgs>? CreateNewWindow;

    public event EventHandler<DocumentGroupReadyEventArgs>? DocumentGroupReady;

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PopupContainer = GetTemplateChild("PART_PopupContainer") as Border;
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
        base.OnDragEnter(e);

        ParentWindow?.Activate();

        VisualStateManager.GoToState(this, Panel is null || Panel.Children.Count is 0 ? "ShowAllDockTargets" : "ShowSideDockTargets", false);
    }

    protected override void OnDragLeave(DragEventArgs e)
    {
        base.OnDragLeave(e);

        VisualStateManager.GoToState(this, "HideDockTargets", false);
    }

    internal void Dock(Document document, DockTarget target)
    {
        VisualStateManager.GoToState(this, "HideDockTargets", false);

        document.Detach();

        DocumentGroup group = new();
        group.CopySizeFrom(document);
        group.Children.Add(document);

        DocumentGroupReady?.Invoke(this, new DocumentGroupReadyEventArgs(document.Title, group));

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

    internal void InvokeCreateNewWindow(Document document, Border titleBar)
    {
        CreateNewWindow?.Invoke(this, new CreateNewWindowEventArgs(document, titleBar));
    }

    internal void InvokeDocumentGroupReady(string documentTitle, DocumentGroup documentGroup)
    {
        DocumentGroupReady?.Invoke(this, new DocumentGroupReadyEventArgs(documentTitle, documentGroup));
    }

    internal void HideDockTargets()
    {
        VisualStateManager.GoToState(this, "HideDockTargets", false);
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
