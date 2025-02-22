using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.UI.Xaml.Markup;

namespace Dock.Abstracts;

[ContentProperty(Name = nameof(Children))]
public abstract class ChildrenContainer<T> : Container where T : IComponent
{
    protected ChildrenContainer()
    {
        Children.CollectionChanged += OnCollectionChanged;
    }

    public ObservableCollection<T> Children { get; } = [];

    protected virtual void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (object? child in e.NewItems)
            {
                if (child is IComponent component)
                {
                    component.AttachTo(this);
                }
            }
        }

        if (e.OldItems is not null)
        {
            foreach (object? child in e.OldItems)
            {
                if (child is IComponent component)
                {
                    component.Detach();
                }
            }
        }
    }
}
