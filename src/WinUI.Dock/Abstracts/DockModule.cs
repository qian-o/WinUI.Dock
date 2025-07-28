using System.Text.Json.Nodes;

namespace WinUI.Dock;

public abstract partial class DockModule : Control
{
    public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(nameof(Owner),
                                                                                          typeof(DockModule),
                                                                                          typeof(DockModule),
                                                                                          new PropertyMetadata(null));

    public static readonly DependencyProperty RootProperty = DependencyProperty.Register(nameof(Root),
                                                                                         typeof(DockManager),
                                                                                         typeof(DockModule),
                                                                                         new PropertyMetadata(null, (d, e) => ((DockModule)d).OnRootChanged((DockManager?)e.OldValue, (DockManager?)e.NewValue)));

    public DockModule? Owner
    {
        get => (DockModule)GetValue(OwnerProperty);
        internal set => SetValue(OwnerProperty, value);
    }

    public DockManager? Root
    {
        get => (DockManager)GetValue(RootProperty);
        internal set => SetValue(RootProperty, value);
    }

    internal void ReferenceSize(DockModule module)
    {
        MinWidth = module.MinWidth;
        MaxWidth = module.MaxWidth;
        Width = module.Width;
        MinHeight = module.MinHeight;
        MaxHeight = module.MaxHeight;
        Height = module.Height;
    }

    internal void Attach(DockModule owner)
    {
        if (Owner == owner)
        {
            return;
        }

        Detach();

        Owner = owner;
        Root = owner.Root;
    }

    internal void Detach(bool detachEmptyContainer = true)
    {
        if (Owner is DockContainer container)
        {
            container.Children.Remove(this);

            if (detachEmptyContainer)
            {
                container.DetachEmptyContainer();
            }
        }

        Owner = null;
        Root = null;
    }

    internal abstract void SaveLayout(JsonObject writer);

    internal abstract void LoadLayout(JsonObject reader);

    protected virtual void OnRootChanged(DockManager? oldRoot, DockManager? newRoot)
    {
    }
}
