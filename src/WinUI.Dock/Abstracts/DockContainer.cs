using System.Collections.ObjectModel;

namespace WinUI.Dock;

[ContentProperty(Name = nameof(Children))]
public abstract partial class DockContainer : DockModule
{
    protected DockContainer()
    {
        Children.CollectionChanged += (_, _) =>
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
        };
    }

    public ObservableCollection<DockModule> Children { get; } = [];

    internal bool IsListening { get; set; } = true;

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

        LoadChildren();
    }

    protected abstract void InitTemplate();

    protected abstract void LoadChildren();

    protected abstract void UnloadChildren();

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
