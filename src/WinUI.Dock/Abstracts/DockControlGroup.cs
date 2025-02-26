using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.UI.Xaml.Markup;

namespace WinUI.Dock.Abstracts;

[ContentProperty(Name = nameof(Children))]
public abstract class DockControlGroup : DockControl
{
    protected DockControlGroup()
    {
        Children.CollectionChanged += Children_CollectionChanged;
    }

    public ObservableCollection<DockControl> Children { get; } = [];

    public void DetachByEmptyGroup()
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i] is DockControlGroup group)
            {
                group.DetachByEmptyGroup();
            }
        }

        if (Children.Count is 0)
        {
            Detach();
        }
    }

    protected abstract void LoadChildren();

    protected abstract void UnloadChildren();

    protected virtual void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UnloadChildren();

        foreach (DockControl control in Children)
        {
            control.Attach(this);
        }

        LoadChildren();
    }
}
