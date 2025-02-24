using System.Collections.Specialized;
using CommunityToolkit.WinUI.Controls;
using Dock.Abstracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using static CommunityToolkit.WinUI.Controls.GridSplitter;

namespace Dock;

[TemplatePart(Name = "PART_Root", Type = typeof(Grid))]
public partial class LayoutContainer : Container<IContainer>
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

        Install();
    }

    protected override void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(sender, e);

        if (e.NewItems is not null)
        {
            foreach (object? child in e.NewItems)
            {
                if (child is Control control)
                {
                    control.SizeChanged += OnChildrenSizeChanged;
                }
            }
        }

        if (e.OldItems is not null)
        {
            foreach (object? child in e.OldItems)
            {
                if (child is Control control)
                {
                    control.SizeChanged -= OnChildrenSizeChanged;
                }
            }
        }

        Install();
    }

    private void Install()
    {
        Uninstall();

        if (root is null)
        {
            return;
        }

        if (Orientation is Orientation.Horizontal)
        {
            for (int i = 0; i < Count; i++)
            {
                root.ColumnDefinitions.Add(new ColumnDefinition() { Width = this[i].DockWidth });
            }
        }
        else if (Orientation is Orientation.Vertical)
        {
            for (int i = 0; i < Count; i++)
            {
                root.RowDefinitions.Add(new RowDefinition() { Height = this[i].DockHeight });
            }
        }

        for (int i = 0; i < Count; i++)
        {
            if (this[i] is Control control)
            {
                Grid.SetColumn(control, i);
                Grid.SetRow(control, i);

                root.Children.Add(control);
            }

            if (i > 0)
            {
                GridSplitter splitter = new();

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

                Grid.SetColumn(splitter, i);
                Grid.SetRow(splitter, i);

                root.Children.Add(splitter);
            }
        }
    }

    private void Uninstall()
    {
        if (root is null)
        {
            return;
        }

        root.ColumnDefinitions.Clear();
        root.RowDefinitions.Clear();
        root.Children.Clear();
    }

    private void OnChildrenSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Control control = (Control)sender;
        IComponent component = (IComponent)sender;

        component.DockWidth = new(control.ActualWidth, GridUnitType.Star);
        component.DockHeight = new(control.ActualHeight, GridUnitType.Star);
    }
}
