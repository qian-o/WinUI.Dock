using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace WinUI.Dock.Abstracts;

[ContentProperty(Name = nameof(Children))]
public abstract partial class DockContainer : DockModule
{
    protected DockContainer()
    {
        Children.CollectionChanged += OnCollectionChanged;
    }

    public ObservableCollection<DockModule> Children { get; } = [];

    internal bool IsListening { get; set; } = true;

    public void DetachEmptyContainer()
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i] is DockContainer container)
            {
                container.DetachEmptyContainer();
            }
        }

        if (Children.Count is 0)
        {
            Detach();
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        InitTemplate();

        LoadChildren();
    }

    protected abstract void InitTemplate();

    protected abstract void LoadChildren();

    protected abstract void UnloadChildren();

    protected abstract bool ValidateChildren();

    protected override void OnRootChanged(DockManager? oldRoot, DockManager? newRoot)
    {
        foreach (DockModule module in Children)
        {
            module.Root = newRoot;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsListening && ValidateChildren())
        {
            UnloadChildren();

            foreach (DockModule module in Children)
            {
                module.Attach(this);
            }

            LoadChildren();
        }
    }
}
