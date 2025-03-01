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
        double scale = tabPosition == TabPosition.Bottom ? -1 : 1;

        HeaderScale.ScaleY = scale;
        ContentScale.ScaleY = scale;
    }

    public void Detach()
    {
        Document = null;
    }
}
