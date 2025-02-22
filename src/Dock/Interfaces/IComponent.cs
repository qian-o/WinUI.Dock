namespace Dock.Interfaces;

public interface IComponent
{
    IComponent? Owner { get; }

    void AttachTo(IComponent? owner);

    void Detach();
}
