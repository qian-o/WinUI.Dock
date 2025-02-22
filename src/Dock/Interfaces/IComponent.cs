namespace Dock.Interfaces;

public interface IComponent
{
    IComponent? Owner { get; set; }

    DockingManager? Manager { get; set; }
}
