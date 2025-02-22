using Microsoft.UI.Xaml.Controls;

namespace Dock.Components;

public sealed partial class SideDocument : UserControl
{
    public SideDocument(Document document, Orientation orientation)
    {
        InitializeComponent();

        Document = document;
        Orientation = orientation;
    }

    public Document Document { get; }

    public Orientation Orientation { get; }

    public void Update(double width, double height)
    {
        MinWidth = Document!.MinWidth;
        MaxWidth = Document.MaxWidth;
        MinHeight = Document.MinHeight;
        MaxHeight = Document.MaxHeight;

        if (Orientation == Orientation.Horizontal)
        {
            Width = width;

            if (Height is double.NaN)
            {
                Height = Document.Height is not double.NaN ? Document.Height : height / 3;
            }
        }
        else
        {
            Height = height;

            if (Width is double.NaN)
            {
                Width = Document.Width is not double.NaN ? Document.Width : width / 3;
            }
        }
    }

    public void Uninstall()
    {
        DocumentTab.Header = null;
        DocumentTab.Content = null;
    }
}
