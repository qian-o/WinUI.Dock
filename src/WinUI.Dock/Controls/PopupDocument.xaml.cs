using Microsoft.UI.Xaml.Controls.Primitives;
using WinUI.Dock.Enums;

namespace WinUI.Dock.Controls;

public sealed partial class PopupDocument : UserControl
{
    private readonly Popup popup;

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
                    Width = double.IsNaN(document.DockWidth) ? DockManager.PopupContainer!.ActualWidth / 3 : document.DockWidth;
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
                    Height = double.IsNaN(document.DockHeight) ? DockManager.PopupContainer!.ActualHeight / 3 : document.DockHeight;

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
                    Width = double.IsNaN(document.DockWidth) ? DockManager.PopupContainer!.ActualWidth / 3 : document.DockWidth;
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
                    Height = double.IsNaN(document.DockHeight) ? DockManager.PopupContainer!.ActualHeight / 3 : document.DockHeight;

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

        popup.SizeChanged += (_, _) =>
        {
            if (DockSide is DockSide.Right)
            {
                popup.HorizontalOffset = DockManager.PopupContainer!.ActualWidth - ActualWidth;
            }
            else if (DockSide is DockSide.Bottom)
            {
                popup.VerticalOffset = DockManager.PopupContainer!.ActualHeight - ActualHeight;
            }
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

    private void Pin_Click(object _, RoutedEventArgs __)
    {
        Detach();
    }

    private void Close_Click(object _, RoutedEventArgs __)
    {
        Detach();
    }

    private void Detach()
    {
        if (Document is not null)
        {
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
