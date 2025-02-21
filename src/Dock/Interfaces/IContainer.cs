using Microsoft.UI.Xaml;

namespace Dock.Interfaces;

public interface IContainer
{
    double DockMinWidth { get; set; }

    double DockMaxWidth { get; set; }

    GridLength DockWidth { get; set; }

    double DockMinHeight { get; set; }

    double DockMaxHeight { get; set; }

    GridLength DockHeight { get; set; }
}
