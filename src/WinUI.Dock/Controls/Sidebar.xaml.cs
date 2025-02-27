using System.Collections.ObjectModel;
using WinUI.Dock.Enums;

namespace WinUI.Dock.Controls;

public sealed partial class Sidebar : UserControl
{
    public Sidebar(DockSide dockSide, DockManager dockManager)
    {
        InitializeComponent();

        DockSide = dockSide;
        DockManager = dockManager;
    }

    public DockSide DockSide { get; }

    public DockManager DockManager { get; }

    public ObservableCollection<Document> Documents { get; } = [];
}
