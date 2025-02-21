using Microsoft.UI.Xaml;

namespace Dock.Interfaces;

public interface IContainer
{
    double DesignMinWidth { get; set; }

    double DesignMaxWidth { get; set; }

    GridLength DesignWidth { get; set; }

    double DesignMinHeight { get; set; }

    double DesignMaxHeight { get; set; }

    GridLength DesignHeight { get; set; }
}
