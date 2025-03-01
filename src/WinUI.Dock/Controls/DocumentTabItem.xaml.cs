using Microsoft.UI.Xaml.Input;
using WinUI.Dock.Enums;

namespace WinUI.Dock.Controls;

public sealed partial class DocumentTabItem : TabViewItem
{
    public DocumentTabItem(TabPosition tabPosition, Document document)
    {
        InitializeComponent();

        Document = document;

        UpdateTabPosition(tabPosition);
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
    }

    private void Header_PointerEntered(object _, PointerRoutedEventArgs __)
    {
        Options.Opacity = 1.0;
    }

    private void Header_PointerExited(object _, PointerRoutedEventArgs __)
    {
        Options.Opacity = 0.0;
    }

    private void Pin_Click(object _, RoutedEventArgs __)
    {
    }

    private void Close_Click(object _, RoutedEventArgs __)
    {
        if (Document is not null)
        {
            LayoutPanel? panel = Document.Root?.Panel;

            Document.Detach();

            panel?.DetachByEmptyChildren();
        }
    }
}
