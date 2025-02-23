using Microsoft.UI.Xaml;

namespace Dock.Helpers;

public static class DockHelpers
{
    public static Window Create()
    {
        Window window = new()
        {
            ExtendsContentIntoTitleBar = true
        };

        window.Activate();

        return window;
    }
}
