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

        Container = container;
        Popup = popup;
        Document = document;
        IsLeft = isLeft;
        IsTop = isTop;
        IsRight = isRight;
        IsBottom = isBottom;

        Container.SizeChanged += OnSizeChanged;

        Update();
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

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Update();
    }
}
