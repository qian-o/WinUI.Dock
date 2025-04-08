using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Graphics;
using WinUI.Dock.Helpers;

namespace WinUI.Dock.Controls;

public sealed partial class DockWindow : Window
{
    private PointInt32 dragOffset;

    public DockWindow(DockManager manager, Document? document)
    {
        InitializeComponent();

        InitializePanel(manager, document);
        InitializeWindow(manager, document);
    }

    internal void SaveLayout(JsonObject writer)
    {
        writer["Position"] = new JsonObject
        {
            ["X"] = AppWindow.Position.X,
            ["Y"] = AppWindow.Position.Y
        };

        writer["Size"] = new JsonObject
        {
            ["Width"] = AppWindow.Size.Width,
            ["Height"] = AppWindow.Size.Height
        };

        JsonObject panelWriter = [];
        Panel.SaveLayout(panelWriter);

        writer[nameof(Panel)] = panelWriter;
    }

    internal void LoadLayout(JsonObject reader)
    {
        AppWindow.Move(new()
        {
            X = reader["Position"]!.AsObject()["X"].Deserialize<int>(),
            Y = reader["Position"]!.AsObject()["Y"].Deserialize<int>()
        });

        AppWindow.Resize(new()
        {
            Width = reader["Size"]!.AsObject()["Width"].Deserialize<int>(),
            Height = reader["Size"]!.AsObject()["Height"].Deserialize<int>()
        });

        Panel.LoadLayout(reader[nameof(Panel)]!.AsObject());
    }

    private void InitializePanel(DockManager manager, Document? document)
    {
        Panel.Root = manager;

        Panel.Children.CollectionChanged += (sender, e) =>
        {
            if (Panel.Children.Count is 0)
            {
                Close();
            }
        };

        if (document is not null)
        {
            document.Detach();

            DocumentGroup group = new();
            group.Children.Add(document);

            LayoutPanel panel = new() { Orientation = Orientation.Horizontal };
            panel.Children.Add(group);

            Panel.Children.Add(panel);
        }
    }

    private void InitializeWindow(DockManager manager, Document? document)
    {
        Closed += (_, _) => DockWindowHelpers.RemoveWindow(manager, this);

        ExtendsContentIntoTitleBar = true;

#if WINDOWS
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
#endif

        AppWindow.Move(PointerHelpers.GetPointerPosition());

        if (document is not null)
        {
            AppWindow.Resize(new()
            {
                Width = (int)(double.IsNaN(document.DockWidth) ? 400 : document.DockWidth),
                Height = (int)(double.IsNaN(document.DockHeight) ? 400 : document.DockHeight)
            });
        }

        manager.InvokeCreateNewWindow(TitleBar);

        DockWindowHelpers.AddWindow(manager, this);
    }

    private void OnDragEnter(object _, DragEventArgs __)
    {
        Activate();
    }

    private void TitleBar_DragStarted(object _, DragStartedEventArgs __)
    {
        PointInt32 point = PointerHelpers.GetPointerPosition();

        dragOffset = new PointInt32
        {
            X = point.X - AppWindow.Position.X,
            Y = point.Y - AppWindow.Position.Y
        };
    }

    private void TitleBar_DragDelta(object _, DragDeltaEventArgs __)
    {
        PointInt32 point = PointerHelpers.GetPointerPosition();

        AppWindow.Move(new()
        {
            X = point.X - dragOffset.X,
            Y = point.Y - dragOffset.Y
        });
    }
}