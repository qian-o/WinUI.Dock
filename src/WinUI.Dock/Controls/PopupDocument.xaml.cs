using WinUI.Dock.Enums;

namespace WinUI.Dock.Controls;

public sealed partial class PopupDocument : UserControl
{
    public PopupDocument(Document document, DockSide dockSide)
    {
        InitializeComponent();

        Document = document;

        switch (dockSide)
        {
            case DockSide.Left:
                break;
            case DockSide.Top:
                break;
            case DockSide.Right:
                break;
            case DockSide.Bottom:
                break;
        }
    }

    public Document Document { get; private set; }
}
