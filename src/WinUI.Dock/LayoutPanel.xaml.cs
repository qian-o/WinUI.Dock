using System.Text.Json.Nodes;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Media;

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
                RowDefinition row = new()
                {
                    MinHeight = container.MinHeight,
                    MaxHeight = container.MaxHeight,
                    Height = double.IsNaN(container.Height) ? new(1, GridUnitType.Star) : new(container.Height, GridUnitType.Pixel)
                };

                row.RegisterPropertyChangedCallback(RowDefinition.HeightProperty, (_, _) => container.Height = row.ActualHeight);

                root.RowDefinitions.Add(row);

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
                ColumnDefinition column = new()
                {
                    MinWidth = container.MinWidth,
                    MaxWidth = container.MaxWidth,
                    Width = double.IsNaN(container.Width) ? new(1, GridUnitType.Star) : new(container.Width, GridUnitType.Pixel)
                };

                column.RegisterPropertyChangedCallback(ColumnDefinition.WidthProperty, (_, _) => container.Width = column.ActualWidth);

                root.ColumnDefinitions.Add(column);

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

    protected override bool ConfirmEmptyContainer()
    {
        return true;
    }

    internal double CalculateHeight(DockModule module)
    {
        if (double.IsNaN(module.Height))
        {
            return Math.Clamp(ActualHeight / (Children.Count + 1), module.MinHeight, module.MaxHeight);
        }

        return Math.Clamp(module.Height, module.MinHeight, module.MaxHeight);
    }

    internal double CalculateWidth(DockModule module)
    {
        if (double.IsNaN(module.Width))
        {
            return Math.Clamp(ActualWidth / (Children.Count + 1), module.MinWidth, module.MaxWidth);
        }

        return Math.Clamp(module.Width, module.MinWidth, module.MaxWidth);
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
