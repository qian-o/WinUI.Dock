using Microsoft.UI.Xaml.Controls;

namespace Dock.Abstracts;

public abstract class Component : Control, IComponent
{
    public IComponent? Owner { get; private set; }

    public void AttachTo(IComponent? owner)
    {
        Owner = owner;
    }

    public void Detach()
    {
        Owner = null;
    }
}
