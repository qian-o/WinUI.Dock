﻿using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls.Primitives;
using WinUI.Dock.Enums;

namespace WinUI.Dock.Controls;

public sealed partial class Sidebar : UserControl
{
    public static readonly DependencyProperty DockSideProperty = DependencyProperty.Register(nameof(DockSide),
                                                                                             typeof(DockSide),
                                                                                             typeof(Sidebar),
                                                                                             new PropertyMetadata(DockSide.Left, OnDockSideChanged));

    public static readonly DependencyProperty DocumentsProperty = DependencyProperty.Register(nameof(Documents),
                                                                                              typeof(ObservableCollection<Document>),
                                                                                              typeof(Sidebar),
                                                                                              new PropertyMetadata(null));

    public static readonly DependencyProperty DockManagerProperty = DependencyProperty.Register(nameof(DockManager),
                                                                                                typeof(DockManager),
                                                                                                typeof(Sidebar),
                                                                                                new PropertyMetadata(null));

    public Sidebar()
    {
        InitializeComponent();
    }

    public DockSide DockSide
    {
        get => (DockSide)GetValue(DockSideProperty);
        set => SetValue(DockSideProperty, value);
    }

    public ObservableCollection<Document>? Documents
    {
        get => (ObservableCollection<Document>?)GetValue(DocumentsProperty);
        set => SetValue(DocumentsProperty, value);
    }

    public DockManager? DockManager
    {
        get => (DockManager)GetValue(DockManagerProperty);
        set => SetValue(DockManagerProperty, value);
    }

    private static void OnDockSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Sidebar sidebar)
        {
            VisualStateManager.GoToState(sidebar, sidebar.DockSide.ToString(), false);
        }
    }

    private void Document_Click(object sender, RoutedEventArgs _)
    {
        Document document = (Document)((Button)sender).DataContext;

        PopupDocument popupDocument = new(document, DockSide);

        Popup popup = new()
        {
            XamlRoot = DockManager!.PopupContainer!.XamlRoot,
            IsLightDismissEnabled = true,
            LightDismissOverlayMode = LightDismissOverlayMode.Auto,
            Child = popupDocument,
            IsOpen = true
        };

        popup.Closed += (_, _) =>
        {
            DockManager!.PopupContainer!.Child = null;

            if (DockSide is DockSide.Left or DockSide.Right)
            {
                document.DockWidth = popupDocument.Width;
            }
            else
            {
                document.DockHeight = popupDocument.Height;
            }
        };

        switch (DockSide)
        {
            case DockSide.Left:
                {
                    popupDocument.Width = double.IsNaN(document.DockWidth) ? DockManager.PopupContainer.ActualWidth / 3 : document.DockWidth;
                    popupDocument.Height = DockManager.PopupContainer.ActualHeight;
                }
                break;
            case DockSide.Top:
                {
                    popupDocument.Width = DockManager.PopupContainer.ActualWidth;
                    popupDocument.Height = double.IsNaN(document.DockHeight) ? DockManager.PopupContainer.ActualHeight / 3 : document.DockHeight;
                }
                break;
            case DockSide.Right:
                {
                    popupDocument.Width = double.IsNaN(document.DockWidth) ? DockManager.PopupContainer.ActualWidth / 3 : document.DockWidth;
                    popupDocument.Height = DockManager.PopupContainer.ActualHeight;
                }
                break;
            case DockSide.Bottom:
                {
                    popupDocument.Width = DockManager.PopupContainer.ActualWidth;
                    popupDocument.Height = double.IsNaN(document.DockHeight) ? DockManager.PopupContainer.ActualHeight / 3 : document.DockHeight;
                }
                break;
        }

        if (DockSide is DockSide.Right)
        {
            popup.HorizontalOffset = DockManager.PopupContainer.ActualWidth - popupDocument.Width;
        }
        else if (DockSide is DockSide.Bottom)
        {
            popup.VerticalOffset = DockManager.PopupContainer.ActualHeight - popupDocument.Height;
        }

        DockManager.PopupContainer.Child = popup;
    }
}
