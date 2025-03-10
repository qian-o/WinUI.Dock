﻿using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Graphics;
using WinUI.Dock.Helpers;

namespace WinUI.Dock.Controls;

public sealed partial class DockWindow : Window
{
    private PointInt32 dragOffset;

    public DockWindow(DockManager manager, Document document)
    {
        InitializeComponent();

        InitializePanel(manager, document);
        InitializeWindow(manager, document);
    }

    private void InitializePanel(DockManager manager, Document document)
    {
        Panel.Root = manager;

        Panel.Children.CollectionChanged += (sender, e) =>
        {
            if (Panel.Children.Count is 0)
            {
                Close();
            }
        };

        document.Detach();

        DocumentGroup group = new();
        group.Children.Add(document);

        LayoutPanel panel = new() { Orientation = Orientation.Horizontal };
        panel.Children.Add(group);

        Panel.Children.Add(panel);
    }

    private void InitializeWindow(DockManager manager, Document document)
    {
        Closed += (_, _) => DockWindowHelpers.RemoveWindow(manager, this);

        ExtendsContentIntoTitleBar = true;

#if WINDOWS
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
#endif

        AppWindow.Move(PointerHelpers.GetPointerPosition());
        AppWindow.Resize(new()
        {
            Width = (int)(double.IsNaN(document.DockWidth) ? 400 : document.DockWidth),
            Height = (int)(double.IsNaN(document.DockHeight) ? 400 : document.DockHeight)
        });

        manager.InvokeCreateNewWindow(TitleBar);

        DockWindowHelpers.AddWindow(manager, this);
    }

    private void OnDragEnter(object _, DragEventArgs __)
    {
        Activate();
    }

    private void TitleBar_DragStarted(object _, DragStartedEventArgs __)
    {
        PointInt32 point = PointerHelpers.GetPointerPosition();

        dragOffset = new PointInt32
        {
            X = point.X - AppWindow.Position.X,
            Y = point.Y - AppWindow.Position.Y
        };
    }

    private void TitleBar_DragDelta(object _, DragDeltaEventArgs __)
    {
        PointInt32 point = PointerHelpers.GetPointerPosition();

        AppWindow.Move(new()
        {
            X = point.X - dragOffset.X,
            Y = point.Y - dragOffset.Y
        });
    }
}