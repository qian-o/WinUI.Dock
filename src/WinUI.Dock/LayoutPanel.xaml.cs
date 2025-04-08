using System.Text.Json;
using System.Text.Json.Nodes;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Media;
using WinUI.Dock.Abstracts;
using WinUI.Dock.Helpers;

namespace WinUI.Dock;

[TemplatePart(Name = "PART_Root", Type = typeof(Grid))]
public partial class LayoutPanel : DockContainer
{
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation),
                                                                                                typeof(Orientation),
                                                                                                typeof(LayoutPanel),
                                                                                                new PropertyMetadata(Orientation.Vertical));

    private Grid? root;

    public LayoutPanel()
    {
        DefaultStyleKey = typeof(LayoutPanel);
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    protected override void InitTemplate()
    {
        root = GetTemplateChild("PART_Root") as Grid;
    }

    protected override void LoadChildren()
    {
        if (root is null)
        {
            return;
        }

        if (Orientation is Orientation.Vertical)
        {
            foreach (DockContainer container in Children.Cast<DockContainer>())
            {
                root.RowDefinitions.Add(new()
                {
                    MinHeight = container.MinHeight,
                    MaxHeight = container.MaxHeight,
                    Height = new(CalculateHeight(container, true), GridUnitType.Star)
                });

                Grid.SetRow(container, root.RowDefinitions.Count - 1);

                root.Children.Add(container);
            }

            for (int i = 1; i < Children.Count; i++)
            {
                GridSplitter splitter = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    ResizeDirection = GridSplitter.GridResizeDirection.Rows,
                    RenderTransform = new TranslateTransform() { Y = -12 }
                };

                Grid.SetRow(splitter, i);

                root.Children.Add(splitter);
            }
        }
        else
        {
            foreach (DockContainer container in Children.Cast<DockContainer>())
            {
                root.ColumnDefinitions.Add(new()
                {
                    MinWidth = container.MinWidth,
                    MaxWidth = container.MaxWidth,
                    Width = new(CalculateWidth(container, true), GridUnitType.Star)
                });

                Grid.SetColumn(container, root.ColumnDefinitions.Count - 1);

                root.Children.Add(container);
            }

            for (int i = 1; i < Children.Count; i++)
            {
                GridSplitter splitter = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    ResizeDirection = GridSplitter.GridResizeDirection.Columns,
                    RenderTransform = new TranslateTransform() { X = -12 }
                };

                Grid.SetColumn(splitter, i);

                root.Children.Add(splitter);
            }
        }
    }

    protected override void UnloadChildren()
    {
        if (root is null)
        {
            return;
        }

        root.ColumnDefinitions.Clear();
        root.RowDefinitions.Clear();
        root.Children.Clear();
    }

    protected override bool ValidateChildren()
    {
        return Children.All(static item => item is DockContainer);
    }

    internal double CalculateHeight(DockModule module, bool isStar)
    {
        DockModule[] children = Children.Contains(module) ? [.. Children] : [.. Children, module];

        double totalHeight = children.Sum(static item => double.IsNaN(item.DockHeight) ? 1.0 : item.DockHeight);
        double moduleHeight = double.IsNaN(module.DockHeight) ? totalHeight / children.Length : module.DockHeight;

        return isStar ? moduleHeight : ActualHeight / totalHeight * moduleHeight;
    }

    internal double CalculateWidth(DockModule module, bool isStar)
    {
        DockModule[] children = Children.Contains(module) ? [.. Children] : [.. Children, module];

        double totalWidth = children.Sum(static item => double.IsNaN(item.DockWidth) ? 1.0 : item.DockWidth);
        double moduleWidth = double.IsNaN(module.DockWidth) ? totalWidth / children.Length : module.DockWidth;

        return isStar ? moduleWidth : ActualWidth / totalWidth * moduleWidth;
    }

    internal override void SaveLayout(JsonObject writer)
    {
        writer.WriteByModuleType(this);
        writer.WriteDockModuleProperties(this);
        writer.WriteDockContainerChildren(this);

        writer[nameof(Orientation)] = (int)Orientation;
    }

    internal override void LoadLayout(JsonObject reader)
    {
        reader.ReadDockModuleProperties(this);
        reader.ReadDockContainerChildren(this);

        Orientation = (Orientation)reader[nameof(Orientation)].Deserialize<int>();
    }
}
