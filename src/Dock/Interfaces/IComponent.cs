using Microsoft.UI.Xaml;

namespace Dock.Interfaces;

public interface IComponent
{
    double DockMinWidth { get; set; }

    double DockMaxWidth { get; set; }

    GridLength DockWidth { get; set; }

    double DockMinHeight { get; set; }

    double DockMaxHeight { get; set; }

    GridLength DockHeight { get; set; }

    IComponent? Owner { get; set; }

    DockingManager? Manager { get; set; }

    void SyncSize(IComponent component);

    void Detach();
}
