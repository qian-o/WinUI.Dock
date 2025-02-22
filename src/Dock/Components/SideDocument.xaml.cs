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

    public Document Document { get; set; }

    public Orientation Orientation { get; set; }

    public void Update(double width, double height)
    {
        MinWidth = Document!.MinWidth;
        MaxWidth = Document.MaxWidth;
        MinHeight = Document.MinHeight;
        MaxHeight = Document.MaxHeight;

        if (Orientation == Orientation.Horizontal)
        {
            Width = width;
            Height = Document.Height is not double.NaN ? Document.Height : height / 3;
        }
        else
        {
            Width = Document.Width is not double.NaN ? Document.Width : width / 3;
            Height = height;
        }
    }

    public void Close()
    {
        DocumentTab.Header = null;
        DocumentTab.Content = null;
    }
}
