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

        SizeChanged += (_, __) => Update();

        Container = container;
        Popup = popup;
        Document = document;
        IsLeft = isLeft;
        IsTop = isTop;
        IsRight = isRight;
        IsBottom = isBottom;

        Container.SizeChanged += (_, __) => Update();

        Update();

        if (IsLeft)
        {
            ContentSizer.Orientation = Orientation.Vertical;
            ContentSizer.FlowDirection = FlowDirection.LeftToRight;

            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Star) });
            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Auto) });

            Grid.SetColumn(Border, 0);
            Grid.SetColumn(ContentSizer, 1);
        }
        else if (IsTop)
        {
            ContentSizer.Orientation = Orientation.Horizontal;
            ContentSizer.FlowDirection = FlowDirection.LeftToRight;

            Grid.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Star) });
            Grid.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Auto) });

            Grid.SetRow(Border, 0);
            Grid.SetRow(ContentSizer, 1);
        }
        else if (IsRight)
        {
            ContentSizer.Orientation = Orientation.Vertical;
            ContentSizer.FlowDirection = FlowDirection.RightToLeft;

            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Auto) });
            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Star) });

            Grid.SetColumn(ContentSizer, 0);
            Grid.SetColumn(Border, 1);
        }
        else if (IsBottom)
        {
            ContentSizer.Orientation = Orientation.Horizontal;
            ContentSizer.FlowDirection = FlowDirection.RightToLeft;

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

    public void Update()
    {
        double width = Container.ActualWidth;
        double height = Container.ActualHeight;

        MinWidth = Document!.MinWidth;
        MaxWidth = Document.MaxWidth;
        MinHeight = Document.MinHeight;
        MaxHeight = Document.MaxHeight;

        if (IsLeft || IsRight)
        {
            Height = height;

            if (Width is double.NaN)
            {
                Width = Document.Width is not double.NaN ? Document.Width : width / 3;
            }
        }
        else
        {
            Width = width;

            if (Height is double.NaN)
            {
                Height = Document.Height is not double.NaN ? Document.Height : height / 3;
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
        DocumentTab.Header = null;
        DocumentTab.Content = null;
    }
}
