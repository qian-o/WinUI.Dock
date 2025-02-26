using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Dock.Components;

public sealed partial class SideDocument : UserControl
{
    public SideDocument(Grid container,
                        Popup popup,
                        Document document,
                        bool isLeft,
                        bool isTop,
                        bool isRight,
                        bool isBottom)
    {
        InitializeComponent();

        SizeChanged += (_, __) => Install();

        Container = container;
        Popup = popup;
        Document = document;
        IsLeft = isLeft;
        IsTop = isTop;
        IsRight = isRight;
        IsBottom = isBottom;

        Container.SizeChanged += (_, __) => Install();

        Install();

        PinButton.Visibility = ((DocumentContainer)Document.Owner!).CanAnchor ? Visibility.Visible : Visibility.Collapsed;

        if (IsLeft)
        {
            ContentSizer.Orientation = Orientation.Vertical;
            ContentSizer.IsDragInverted = false;

            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Star) });
            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Auto) });

            Grid.SetColumn(Border, 0);
            Grid.SetColumn(ContentSizer, 1);
        }
        else if (IsTop)
        {
            ContentSizer.Orientation = Orientation.Horizontal;
            ContentSizer.IsDragInverted = false;

            Grid.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Star) });
            Grid.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Auto) });

            Grid.SetRow(Border, 0);
            Grid.SetRow(ContentSizer, 1);
        }
        else if (IsRight)
        {
            ContentSizer.Orientation = Orientation.Vertical;
            ContentSizer.IsDragInverted = true;

            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Auto) });
            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Star) });

            Grid.SetColumn(ContentSizer, 0);
            Grid.SetColumn(Border, 1);
        }
        else if (IsBottom)
        {
            ContentSizer.Orientation = Orientation.Horizontal;
            ContentSizer.IsDragInverted = true;

            Grid.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Auto) });
            Grid.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Star) });

            Grid.SetRow(ContentSizer, 0);
            Grid.SetRow(Border, 1);
        }
    }

    public Grid Container { get; }

    public Popup Popup { get; }

    public Document Document { get; }

    public bool IsLeft { get; }

    public bool IsTop { get; }

    public bool IsRight { get; }

    public bool IsBottom { get; }

    public void Install()
    {
        double width = Container.ActualWidth;
        double height = Container.ActualHeight;

        MinWidth = Document!.DockMinWidth;
        MaxWidth = Document.DockMaxWidth;
        MinHeight = Document.DockMinHeight;
        MaxHeight = Document.DockMaxHeight;

        if (IsLeft || IsRight)
        {
            Height = height;

            if (Width is double.NaN)
            {
                Width = Document.DockWidth.GridUnitType switch
                {
                    GridUnitType.Pixel => Document.DockWidth.Value,
                    _ => width / 3
                };
            }
        }
        else
        {
            Width = width;

            if (Height is double.NaN)
            {
                Height = Document.DockHeight.GridUnitType switch
                {
                    GridUnitType.Pixel => Document.DockHeight.Value,
                    _ => height / 3
                };
            }
        }

        if (IsRight)
        {
            Popup.HorizontalOffset = width - Width;
            Popup.VerticalOffset = 0;
        }
        else if (IsBottom)
        {
            Popup.HorizontalOffset = 0;
            Popup.VerticalOffset = height - Height;
        }
    }

    public void Uninstall()
    {
        if (ActualWidth is not 0)
        {
            Document.DockWidth = new(ActualWidth);
        }

        if (ActualHeight is not 0)
        {
            Document.DockHeight = new(ActualHeight);
        }

        DocumentTab.Header = null;
        DocumentTab.Content = null;
    }

    private void PinButton_Click(object _, RoutedEventArgs __)
    {
        Popup.IsOpen = false;

        Uninstall();

        DockingManager manager = Document.Manager!;
        DocumentContainer container = (DocumentContainer)Document.Owner!;
        container.SyncSize(Document);

        if (IsLeft)
        {
            manager.Left.Remove(container);

            LayoutContainer layoutContainer = new()
            {
                Orientation = Orientation.Horizontal
            };

            layoutContainer.Add(container);
            layoutContainer.Add(manager.Container!);

            manager.Container = layoutContainer;
        }
        else if (IsTop)
        {
            manager.Top.Remove(container);

            LayoutContainer layoutContainer = new()
            {
                Orientation = Orientation.Vertical
            };

            layoutContainer.Add(container);
            layoutContainer.Add(manager.Container!);

            manager.Container = layoutContainer;
        }
        else if (IsRight)
        {
            manager.Right.Remove(container);

            LayoutContainer layoutContainer = new()
            {
                Orientation = Orientation.Horizontal
            };

            layoutContainer.Add(manager.Container!);
            layoutContainer.Add(container);

            manager.Container = layoutContainer;
        }
        else if (IsBottom)
        {
            manager.Bottom.Remove(container);

            LayoutContainer layoutContainer = new()
            {
                Orientation = Orientation.Vertical
            };

            layoutContainer.Add(manager.Container!);
            layoutContainer.Add(container);

            manager.Container = layoutContainer;
        }

        container.Install(container.IndexOf(Document));
    }

    private void DocumentTab_CloseRequested(TabViewItem _, TabViewTabCloseRequestedEventArgs __)
    {
        Popup.IsOpen = false;

        Uninstall();

        Document.Detach();
    }
}
