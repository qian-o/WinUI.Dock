using System.Collections.Specialized;
using Dock.Abstracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

        root.ColumnDefinitions.Clear();
        root.RowDefinitions.Clear();
        root.Children.Clear();

        int count = 0;
        foreach (IContainer item in Children)
        {
            Control control = (Control)item;

            if (Orientation is Orientation.Horizontal)
            {
                Grid.SetColumn(control, count++);

                root.ColumnDefinitions.Add(new ColumnDefinition { Width = new(1, GridUnitType.Star) });
            }
            else
            {
                Grid.SetRow(control, count++);

                root.RowDefinitions.Add(new RowDefinition { Height = new(1, GridUnitType.Star) });
            }

            root.Children.Add(control);
        }
    }
}
