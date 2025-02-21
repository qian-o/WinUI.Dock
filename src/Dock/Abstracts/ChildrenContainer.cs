using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.UI.Xaml.Markup;

namespace Dock.Abstracts;

[ContentProperty(Name = nameof(Children))]
public abstract class ChildrenContainer<T> : Container
{
    protected ChildrenContainer()
    {
        Children.CollectionChanged += OnCollectionChanged;
    }

    public ObservableCollection<T> Children { get; } = [];

    protected virtual void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
    }
}
