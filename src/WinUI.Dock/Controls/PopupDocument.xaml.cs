﻿using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel.DataTransfer;
using WinUI.Dock.Enums;
using WinUI.Dock.Helpers;

namespace WinUI.Dock.Controls;

public sealed partial class PopupDocument : UserControl
{
    private readonly Popup popup;

    private string dragKey = string.Empty;

    public PopupDocument(DockManager dockManager, DockSide dockSide, Document document)
    {
        InitializeComponent();

        DockManager = dockManager;
        DockSide = dockSide;
        Document = document;

        switch (DockSide)
        {
            case DockSide.Left:
                {
                    Width = double.IsNaN(Document.DockWidth) ? DockManager.PopupContainer!.ActualWidth / 3 : Document.DockWidth;
                    Height = DockManager.PopupContainer!.ActualHeight;

                    Layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Star) });
                    Layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Auto) });

                    Grid.SetColumn(Root, 0);
                    Grid.SetColumn(Sizer, 1);

                    Sizer.Orientation = Orientation.Vertical;
                    Sizer.IsDragInverted = false;
                }
                break;
            case DockSide.Top:
                {
                    Width = DockManager.PopupContainer!.ActualWidth;
                    Height = double.IsNaN(Document.DockHeight) ? DockManager.PopupContainer!.ActualHeight / 3 : Document.DockHeight;

                    Layout.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Star) });
                    Layout.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Auto) });

                    Grid.SetRow(Root, 0);
                    Grid.SetRow(Sizer, 1);

                    Sizer.Orientation = Orientation.Horizontal;
                    Sizer.IsDragInverted = false;
                }
                break;
            case DockSide.Right:
                {
                    Width = double.IsNaN(Document.DockWidth) ? DockManager.PopupContainer!.ActualWidth / 3 : Document.DockWidth;
                    Height = DockManager.PopupContainer!.ActualHeight;

                    Layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Auto) });
                    Layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Star) });

                    Grid.SetColumn(Sizer, 0);
                    Grid.SetColumn(Root, 1);

                    Sizer.Orientation = Orientation.Vertical;
                    Sizer.IsDragInverted = true;
                }
                break;
            case DockSide.Bottom:
                {
                    Width = DockManager.PopupContainer!.ActualWidth;
                    Height = double.IsNaN(Document.DockHeight) ? DockManager.PopupContainer!.ActualHeight / 3 : Document.DockHeight;

                    Layout.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Auto) });
                    Layout.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Star) });

                    Grid.SetRow(Sizer, 0);
                    Grid.SetRow(Root, 1);

                    Sizer.Orientation = Orientation.Horizontal;
                    Sizer.IsDragInverted = true;
                }
                break;
        }

        popup = new()
        {
            Child = this,
            IsLightDismissEnabled = true,
            XamlRoot = DockManager.PopupContainer!.XamlRoot
        };

        popup.Closed += (_, _) => Detach();
    }

    public DockManager DockManager { get; }

    public DockSide DockSide { get; }

    public Document? Document { get; private set; }

    public void Show()
    {
        popup.IsOpen = true;

        DockManager.ActiveDocument = Document;
        DockManager.PopupContainer!.Child = popup;
    }

    private void OnSizeChanged(object _, SizeChangedEventArgs __)
    {
        if (DockSide is DockSide.Right)
        {
            popup.HorizontalOffset = DockManager.PopupContainer!.ActualWidth - ActualWidth;
        }
        else if (DockSide is DockSide.Bottom)
        {
            popup.VerticalOffset = DockManager.PopupContainer!.ActualHeight - ActualHeight;
        }
    }

    private void Header_DragStarting(UIElement _, DragStartingEventArgs args)
    {
        args.Data.SetData(DragDropHelpers.FormatId, dragKey = DragDropHelpers.GetDragKey(Document!));

        Detach(true);
    }

    private void Header_DropCompleted(UIElement _, DropCompletedEventArgs args)
    {
        if (args.DropResult is not DataPackageOperation.Move && DragDropHelpers.GetDocument(dragKey) is Document document)
        {
            DockWindow dockWindow = new(DockManager, document);

            dockWindow.Activate();
        }

        DragDropHelpers.RemoveDragKey(dragKey);
    }

    private void Pin_Click(object _, RoutedEventArgs __)
    {
        Document document = Document!;

        Detach(true);

        DockManager.Dock(document, DockSide switch
        {
            DockSide.Left => DockTarget.DockLeft,
            DockSide.Top => DockTarget.DockTop,
            DockSide.Right => DockTarget.DockRight,
            DockSide.Bottom => DockTarget.DockBottom,
            _ => throw new NotSupportedException()
        });
    }

    private void Close_Click(object _, RoutedEventArgs __)
    {
        Detach(true);
    }

    private void Detach(bool remove = false)
    {
        if (Document is not null)
        {
            if (remove)
            {
                switch (DockSide)
                {
                    case DockSide.Left:
                        DockManager.LeftSide.Remove(Document);
                        break;
                    case DockSide.Top:
                        DockManager.TopSide.Remove(Document);
                        break;
                    case DockSide.Right:
                        DockManager.RightSide.Remove(Document);
                        break;
                    case DockSide.Bottom:
                        DockManager.BottomSide.Remove(Document);
                        break;
                }
            }

            if (DockSide is DockSide.Left or DockSide.Right)
            {
                Document.DockWidth = ActualWidth;
            }
            else
            {
                Document.DockHeight = ActualHeight;
            }

            Document = null;

            Bindings.Update();
        }

        popup.IsOpen = false;

        DockManager.ActiveDocument = null;
        DockManager.PopupContainer!.Child = null;
    }
}
