using System.Collections.Specialized;
using CommunityToolkit.WinUI.Controls;
using Dock.Abstracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using static CommunityToolkit.WinUI.Controls.GridSplitter;

namespace Dock;

[TemplatePart(Name = "PART_Root", Type = typeof(Grid))]
public partial class LayoutContainer : ChildrenContainer<IContainer>
{
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation),
                                                                                                typeof(Orientation),
                                                                                                typeof(LayoutContainer),
                                                                                                new PropertyMetadata(Orientation.Horizontal));

    private Grid? root;

    public LayoutContainer()
    {
        DefaultStyleKey = typeof(LayoutContainer);
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        root = GetTemplateChild("PART_Root") as Grid;

        Update();
    }

    protected override void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Update();
    }

    private void Update()
    {
        if (root is null)
        {
            return;
        }

        if (Orientation is Orientation.Horizontal)
        {
            UpdateColumnDefinitions();
        }
        else
        {
            UpdateRowDefinitions();
        }

        root.Children.Clear();

        for (int i = 0; i < Children.Count; i++)
        {
            Control control = (Control)Children[i];

            Grid.SetColumn(control, i);
            Grid.SetRow(control, i);

            root.Children.Add(control);

            if (i > 0)
            {
                GridSplitter splitter = new();

                Grid.SetColumn(splitter, i);
                Grid.SetRow(splitter, i);

                if (Orientation is Orientation.Horizontal)
                {
                    splitter.HorizontalAlignment = HorizontalAlignment.Left;
                    splitter.VerticalAlignment = VerticalAlignment.Stretch;
                    splitter.ResizeDirection = GridResizeDirection.Columns;
                    splitter.RenderTransform = new TranslateTransform() { X = -12 };
                }
                else
                {
                    splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    splitter.VerticalAlignment = VerticalAlignment.Top;
                    splitter.ResizeDirection = GridResizeDirection.Rows;
                    splitter.RenderTransform = new TranslateTransform() { Y = -12 };
                }

                root.Children.Add(splitter);
            }
        }
    }

    private void UpdateColumnDefinitions()
    {
        if (root is null)
        {
            return;
        }

        if (root.ColumnDefinitions.Count < Children.Count)
        {
            for (int i = root.ColumnDefinitions.Count; i < Children.Count; i++)
            {
                IContainer container = Children[i];

                root.ColumnDefinitions.Add(new()
                {
                    MinWidth = container.DesignMinWidth,
                    MaxWidth = container.DesignMaxWidth,
                    Width = container.DesignWidth
                });
            }
        }
        else if (root.ColumnDefinitions.Count > Children.Count)
        {
            for (int i = root.ColumnDefinitions.Count - 1; i >= Children.Count; i--)
            {
                root.ColumnDefinitions.RemoveAt(i);
            }
        }
    }

    private void UpdateRowDefinitions()
    {
        if (root is null)
        {
            return;
        }

        if (root.RowDefinitions.Count < Children.Count)
        {
            for (int i = root.RowDefinitions.Count; i < Children.Count; i++)
            {
                IContainer container = Children[i];

                root.RowDefinitions.Add(new()
                {
                    MinHeight = container.DesignMinHeight,
                    MaxHeight = container.DesignMaxHeight,
                    Height = container.DesignHeight
                });
            }
        }
        else if (root.RowDefinitions.Count > Children.Count)
        {
            for (int i = root.RowDefinitions.Count - 1; i >= Children.Count; i--)
            {
                root.RowDefinitions.RemoveAt(i);
            }
        }
    }
}
