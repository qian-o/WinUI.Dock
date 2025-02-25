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

    public void ClearEmptyGroup()
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i] is DockControlGroup group)
            {
                group.ClearEmptyGroup();

                if (group.Children.Count is 0)
                {
                    Children.RemoveAt(i);
                }
            }
        }
    }

    protected virtual void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (DockControl control in Children)
        {
            control.Attach(this);
        }
    }
}
