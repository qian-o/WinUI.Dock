using System.Collections.Specialized;
using Dock.Abstracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dock;

[TemplatePart(Name = "PART_Root", Type = typeof(TabView))]
public partial class DocumentContainer : ChildrenContainer<Document>
{
    public static readonly DependencyProperty CanAnchorProperty = DependencyProperty.Register(nameof(CanAnchor),
                                                                                              typeof(bool),
                                                                                              typeof(DocumentContainer),
                                                                                              new PropertyMetadata(true));

    private TabView? root;

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

        Update();
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
