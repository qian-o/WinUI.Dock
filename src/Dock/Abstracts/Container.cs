using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.UI.Xaml.Markup;

namespace Dock.Abstracts;

[ContentProperty(Name = nameof(Children))]
public abstract class Container<T> : Component, IContainer where T : IComponent
{
    protected Container()
    {
        Children.CollectionChanged += OnCollectionChanged;
    }

    public ObservableCollection<T> Children { get; } = [];

    public IComponent this[int index] => Children[index];

    public int Count => Children.Count;

    public bool AutoRemove { get; set; } = true;

    public void Add(IComponent component)
    {
        if (component is T child)
        {
            Children.Add(child);
        }
        else
        {
            throw new InvalidOperationException($"Component must be of type {typeof(T).Name}");
        }
    }

    public void Add(IComponent component, int index)
    {
        if (component is T child)
        {
            Children.Insert(index, child);
        }
        else
        {
            throw new InvalidOperationException($"Component must be of type {typeof(T).Name}");
        }
    }

    public void Clear()
    {
        Children.Clear();
    }

    public void Remove(IComponent component)
    {
        if (component is T child)
        {
            Children.Remove(child);
        }
        else
        {
            throw new InvalidOperationException($"Component must be of type {typeof(T).Name}");
        }
    }

    public void RemoveAt(int index)
    {
        Children.RemoveAt(index);
    }

    public int IndexOf(IComponent component)
    {
        if (component is T child)
        {
            return Children.IndexOf(child);
        }
        else
        {
            throw new InvalidOperationException($"Component must be of type {typeof(T).Name}");
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

        if (AutoRemove && Count is 0 && Owner is IContainer container)
        {
            container.Remove(this);
        }
    }
}
