using Microsoft.UI.Xaml.Controls;

namespace Dock.Abstracts;

public abstract class Component : Control, IComponent
{
    private IComponent? owner;
    private DockingManager? manager;

    public IComponent? Owner
    {
        get => owner;
        set
        {
            if (owner != value)
            {
                IComponent? oldOwner = owner;
                IComponent? newOwner = owner = value;

                OnOwnerChanged(oldOwner, newOwner);
            }
        }
    }

    public DockingManager? Manager
    {
        get => manager;
        set
        {
            if (manager != value)
            {
                DockingManager? oldManager = manager;
                DockingManager? newManager = manager = value;

                OnManagerChanged(oldManager, newManager);
            }
        }
    }

    protected virtual void OnOwnerChanged(IComponent? oldOwner, IComponent? newOwner)
    {
    }

    protected virtual void OnManagerChanged(DockingManager? oldManager, DockingManager? newManager)
    {
    }
}
