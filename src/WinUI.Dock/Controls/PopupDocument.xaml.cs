using WinUI.Dock.Enums;

namespace WinUI.Dock.Controls;

public sealed partial class PopupDocument : UserControl
{
    public PopupDocument(DockSide dockSide, Document document)
    {
        InitializeComponent();

        DockSide = dockSide;
        Document = document;

        switch (DockSide)
        {
            case DockSide.Left:
                {
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
                    Layout.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Auto) });
                    Layout.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Star) });

                    Grid.SetRow(Sizer, 0);
                    Grid.SetRow(Root, 1);

                    Sizer.Orientation = Orientation.Horizontal;
                    Sizer.IsDragInverted = true;
                }
                break;
        }
    }

    public DockSide DockSide { get; }

    public Document? Document { get; private set; }

    public event EventHandler<object>? RequestClose;

    public void Detach()
    {
        Document = null;

        Bindings.Update();
    }

    private void Pin_Click(object _, RoutedEventArgs __)
    {
        RemoveDocument();

        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void Close_Click(object _, RoutedEventArgs __)
    {
        RemoveDocument();

        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void RemoveDocument()
    {
        DockManager dockManager = Document!.Root!;

        if (dockManager.ActiveDocument == Document)
        {
            dockManager.ActiveDocument = null;
        }

        switch (DockSide)
        {
            case DockSide.Left:
                dockManager.LeftSide.Remove(Document);
                break;
            case DockSide.Top:
                dockManager.TopSide.Remove(Document);
                break;
            case DockSide.Right:
                dockManager.RightSide.Remove(Document);
                break;
            case DockSide.Bottom:
                dockManager.BottomSide.Remove(Document);
                break;
        }
    }
}
