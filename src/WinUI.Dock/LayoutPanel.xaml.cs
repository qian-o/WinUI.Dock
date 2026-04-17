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
    private bool pixelHintsConverted;

    public LayoutPanel()
    {
        DefaultStyleKey = typeof(LayoutPanel);

        SizeChanged += OnSizeChanged;
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

    protected override void InitChildren()
    {
        if (root is null)
        {
            return;
        }

        root.Children.Clear();

        foreach (DockContainer container in Children.Cast<DockContainer>())
        {
            root.Children.Add(container);
        }

        UpdateLayoutStructure();
    }

    protected override void SynchronizeChildren(DockModule[] oldChildren,
                                                int oldStartingIndex,
                                                DockModule[] newChildren,
                                                int newStartingIndex)
    {
        if (root is null)
        {
            return;
        }

        foreach (DockContainer container in oldChildren.Cast<DockContainer>())
        {
            root.Children.Remove(container);
        }

        foreach (DockContainer container in newChildren.Cast<DockContainer>())
        {
            root.Children.Add(container);
        }

        UpdateLayoutStructure();
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
        return Math.Clamp(ActualHeight * 0.25, module.MinHeight, module.MaxHeight);
    }

    internal double CalculateWidth(DockModule module)
    {
        return Math.Clamp(ActualWidth * 0.25, module.MinWidth, module.MaxWidth);
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

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!pixelHintsConverted)
        {
            TryConvertPixelHints();
        }
    }

    private void TryConvertPixelHints()
    {
        if (root is null || Children.Count is 0)
        {
            return;
        }

        bool isVertical = Orientation is Orientation.Vertical;
        double totalSize = isVertical ? ActualHeight : ActualWidth;

        if (totalSize <= 0)
        {
            return;
        }

        double spacing = isVertical ? root.RowSpacing : root.ColumnSpacing;
        double totalSpacing = Math.Max(0, Children.Count - 1) * spacing;
        double availableSize = totalSize - totalSpacing;

        if (availableSize <= 0)
        {
            return;
        }

        bool hasPixelHints = false;
        double totalPixels = 0;
        double totalStars = 0;

        foreach (DockModule child in Children)
        {
            if (!double.IsNaN(child.DockSize))
            {
                totalStars += child.DockSize;
            }
            else
            {
                double pixelHint = isVertical ? child.Height : child.Width;

                if (!double.IsNaN(pixelHint))
                {
                    hasPixelHints = true;
                    totalPixels += pixelHint;
                }
                else
                {
                    totalStars += 1.0;
                }
            }
        }

        if (!hasPixelHints)
        {
            pixelHintsConverted = true;
            return;
        }

        double starSpace = availableSize - totalPixels;

        if (starSpace <= 0)
        {
            starSpace = availableSize;
        }

        double pixelsPerStar = totalStars > 0 ? starSpace / totalStars : availableSize;

        foreach (DockModule child in Children)
        {
            if (!double.IsNaN(child.DockSize))
            {
                continue;
            }

            double pixelHint = isVertical ? child.Height : child.Width;

            if (!double.IsNaN(pixelHint))
            {
                child.DockSize = pixelHint / pixelsPerStar;

                if (isVertical)
                {
                    child.Height = double.NaN;
                }
                else
                {
                    child.Width = double.NaN;
                }
            }
        }

        pixelHintsConverted = true;

        UpdateLayoutStructure();
    }

    private void UpdateLayoutStructure()
    {
        if (root is null)
        {
            return;
        }

        root.RowDefinitions.Clear();
        root.ColumnDefinitions.Clear();

        foreach (UIElement element in root.Children.OfType<GridSplitter>().ToArray())
        {
            root.Children.Remove(element);
        }

        if (Orientation is Orientation.Vertical)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                DockModule module = Children[i];
                bool isPixelFallback = double.IsNaN(module.DockSize) && !double.IsNaN(module.Height);
                double star = double.IsNaN(module.DockSize) ? 1.0 : module.DockSize;

                RowDefinition row = new()
                {
                    MinHeight = module.MinHeight,
                    MaxHeight = module.MaxHeight,
                    Height = isPixelFallback
                        ? new(module.Height, GridUnitType.Pixel)
                        : new(star, GridUnitType.Star)
                };

                if (isPixelFallback)
                {
                    row.RegisterPropertyChangedCallback(RowDefinition.HeightProperty, (_, _) => module.Height = row.Height.Value);
                }
                else
                {
                    row.RegisterPropertyChangedCallback(RowDefinition.HeightProperty, (_, _) => module.DockSize = row.Height.Value);
                }

                root.RowDefinitions.Add(row);

                Grid.SetRow(module, i);
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
            for (int i = 0; i < Children.Count; i++)
            {
                DockModule module = Children[i];
                bool isPixelFallback = double.IsNaN(module.DockSize) && !double.IsNaN(module.Width);
                double star = double.IsNaN(module.DockSize) ? 1.0 : module.DockSize;

                ColumnDefinition column = new()
                {
                    MinWidth = module.MinWidth,
                    MaxWidth = module.MaxWidth,
                    Width = isPixelFallback
                        ? new(module.Width, GridUnitType.Pixel)
                        : new(star, GridUnitType.Star)
                };

                if (isPixelFallback)
                {
                    column.RegisterPropertyChangedCallback(ColumnDefinition.WidthProperty, (_, _) => module.Width = column.Width.Value);
                }
                else
                {
                    column.RegisterPropertyChangedCallback(ColumnDefinition.WidthProperty, (_, _) => module.DockSize = column.Width.Value);
                }

                root.ColumnDefinitions.Add(column);

                Grid.SetColumn(module, i);
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
}
