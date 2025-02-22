using System.Collections.Specialized;
using Dock.Abstracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Dock;

[TemplatePart(Name = "PART_Root", Type = typeof(TabView))]
[TemplatePart(Name = "PART_PinButton", Type = typeof(ToggleButton))]
public partial class DocumentContainer : Container<Document>
{
    public static readonly DependencyProperty CanAnchorProperty = DependencyProperty.Register(nameof(CanAnchor),
                                                                                              typeof(bool),
                                                                                              typeof(DocumentContainer),
                                                                                              new PropertyMetadata(true));

    private TabView? root;
    private ToggleButton? pinButton;

    public DocumentContainer()
    {
        DefaultStyleKey = typeof(DocumentContainer);
    }

    public bool CanAnchor
    {
        get => (bool)GetValue(CanAnchorProperty);
        set => SetValue(CanAnchorProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        root = GetTemplateChild("PART_Root") as TabView;
        pinButton = GetTemplateChild("PART_PinButton") as ToggleButton;

        if (pinButton is not null)
        {
            pinButton.Click += PinButton_Click;
        }

        Update();
    }

    private void PinButton_Click(object sender, RoutedEventArgs e)
    {
        LayoutContainer container = (LayoutContainer)Owner!;
        DockingManager manager = Manager!;

        int index = container.IndexOf(this);

        if (index >= 0)
        {
            container.RemoveAt(index);
        }

        if (index is 0)
        {
            if (container.Orientation is Orientation.Horizontal)
            {
                manager.Left.Add(this);
            }
            else
            {
                manager.Top.Add(this);
            }
        }
        else if (container.Orientation is Orientation.Horizontal)
        {
            manager.Right.Add(this);
        }
        else
        {
            manager.Bottom.Add(this);
        }
    }

    protected override void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(sender, e);

        Update();
    }

    private void Update()
    {
        if (root is null)
        {
            return;
        }

        root.TabItems.Clear();

        foreach (Document item in Children)
        {
            root.TabItems.Add(new TabViewItem()
            {
                Header = item.Title,
                Content = item,
                IsClosable = item.CanClose
            });
        }
    }
}
