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

    protected override void OnOwnerChanged(IComponent? oldOwner, IComponent? newOwner)
    {
        foreach (T item in Children)
        {
            item.Owner = newOwner;
        }
    }

    protected override void OnManagerChanged(DockingManager? oldManager, DockingManager? newManager)
    {
        foreach (T item in Children)
        {
            item.Manager = newManager;
        }
    }

    protected virtual void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (object? child in e.NewItems)
            {
                if (child is IComponent component)
                {
                    component.Owner = this;
                    component.Manager = Manager;
                }
            }
        }

        if (e.OldItems is not null)
        {
            foreach (object? child in e.OldItems)
            {
                if (child is IComponent component)
                {
                    component.Owner = null;
                    component.Manager = null;
                }
            }
        }
    }
}
