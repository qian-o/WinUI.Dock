﻿using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using WinUI.Dock.Converters;
using WinUI.Dock.Enums;

namespace WinUI.Dock.Controls;

public sealed partial class DocumentTabItem : TabViewItem
{
    public DocumentTabItem(TabPosition tabPosition, Document document)
    {
        InitializeComponent();

        Document = document;

        UpdateTabPosition(tabPosition);

        ActiveIndicator.SetBinding(VisibilityProperty, new Binding
        {
            Source = Document.Root,
            Path = new(nameof(DockManager.ActiveDocument)),
            Converter = new ActiveDocumentToVisibilityConverter(),
            ConverterParameter = Document
        });
    }

    public Document? Document { get; private set; }

    public void UpdateTabPosition(TabPosition tabPosition)
    {
        double scale = tabPosition == TabPosition.Bottom ? -1.0 : 1.0;

        HeaderScale.ScaleY = scale;
        ContentScale.ScaleY = scale;
    }

    public void Detach()
    {
        Document = null;

        Bindings.Update();
    }

    private void Header_PointerEntered(object _, PointerRoutedEventArgs __)
    {
        Options.Opacity = 1.0;
    }

    private void Header_PointerExited(object _, PointerRoutedEventArgs __)
    {
        Options.Opacity = 0.0;
    }

    private void Document_PointerPressed(object _, PointerRoutedEventArgs __)
    {
        Document!.Root!.ActiveDocument = Document;
    }

    private void Pin_Click(object _, RoutedEventArgs __)
    {
        DockManager dockManager = Document!.Root!;

        Point point = TransformToVisual(dockManager).TransformPoint(new Point(0, 0));

        double left = point.X;
        double top = point.Y;
        double right = dockManager.ActualWidth - point.X;
        double bottom = dockManager.ActualHeight - point.Y;

        if (Document.ActualWidth < Document.ActualHeight)
        {
            if (left < right)
            {
                dockManager.LeftSide.Add(Document);
            }
            else
            {
                dockManager.RightSide.Add(Document);
            }
        }
        else if (top < bottom)
        {
            dockManager.TopSide.Add(Document);
        }
        else
        {
            dockManager.BottomSide.Add(Document);
        }

        Document.Detach(true);
    }

    private void Close_Click(object _, RoutedEventArgs __)
    {
        if (Document!.Root!.ActiveDocument == Document)
        {
            Document.Root.ActiveDocument = null;
        }

        Document.Detach(true);
    }
}
