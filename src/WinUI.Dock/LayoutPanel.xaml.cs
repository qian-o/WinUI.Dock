using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Media;
using WinUI.Dock.Abstracts;

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
            foreach (DockModule module in Children)
            {
                root.RowDefinitions.Add(new()
                {
                    MinHeight = module.MinHeight,
                    MaxHeight = module.MaxHeight,
                    Height = new(double.IsNaN(module.DockHeight) ? 1.0 : module.DockHeight, GridUnitType.Star)
                });

                Grid.SetRow(module, root.RowDefinitions.Count - 1);

                root.Children.Add(module);
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
            foreach (DockModule module in Children)
            {
                root.ColumnDefinitions.Add(new()
                {
                    MinWidth = module.MinWidth,
                    MaxWidth = module.MaxWidth,
                    Width = new(double.IsNaN(module.DockWidth) ? 1.0 : module.DockWidth, GridUnitType.Star)
                });

                Grid.SetColumn(module, root.ColumnDefinitions.Count - 1);

                root.Children.Add(module);
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
}
