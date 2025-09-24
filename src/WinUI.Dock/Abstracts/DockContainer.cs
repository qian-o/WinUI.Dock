using System.Collections.ObjectModel;

namespace WinUI.Dock;

[ContentProperty(Name = nameof(Children))]
public abstract partial class DockContainer : DockModule
{
    private bool isInitChildren;

    protected DockContainer()
    {
        Children.CollectionChanged += (_, e) =>
        {
            if (ValidateChildren())
            {
                DockModule[] newChildren = e.NewItems?.Cast<DockModule>().ToArray() ?? [];
                DockModule[] oldChildren = e.OldItems?.Cast<DockModule>().ToArray() ?? [];

                foreach (DockModule module in newChildren)
                {
                    module.Attach(this);
                }

                foreach (DockModule module in oldChildren)
                {
                    module.Detach();
                }

                if (isInitChildren)
                {
                    NewChildren(newChildren, e.NewStartingIndex);
                    OldChildren(oldChildren, e.OldStartingIndex);
                }
            }
        };
    }

    public ObservableCollection<DockModule> Children { get; } = [];

    internal void DetachEmptyContainer()
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i] is DockContainer container)
            {
                container.DetachEmptyContainer();
            }
        }

        if (Children.Count is 0 && ConfirmEmptyContainer())
        {
            Detach();
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        InitTemplate();
        InitChildren();

        isInitChildren = true;
    }

    protected abstract void InitTemplate();

    protected abstract void InitChildren();

    protected abstract void NewChildren(DockModule[] children, int startingIndex);

    protected abstract void OldChildren(DockModule[] children, int startingIndex);

    protected abstract bool ValidateChildren();

    protected abstract bool ConfirmEmptyContainer();

    protected override void OnRootChanged(DockManager? oldRoot, DockManager? newRoot)
    {
        foreach (DockModule module in Children)
        {
            module.Root = newRoot;
        }
    }
}
