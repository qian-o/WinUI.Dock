using WinUI.Dock.Controls;

namespace WinUI.Dock.Helpers;

public static class DockWindowHelpers
{
    private static readonly Dictionary<DockManager, List<DockWindow>> windows = [];

    public static void AddWindow(DockManager dockManager, DockWindow window)
    {
        if (!windows.TryGetValue(dockManager, out List<DockWindow>? value))
        {
            windows[dockManager] = value = [];
        }

        value.Add(window);
    }

    public static void RemoveWindow(DockManager dockManager, DockWindow window)
    {
        if (windows.TryGetValue(dockManager, out List<DockWindow>? value))
        {
            value.Remove(window);

            if (value.Count is 0)
            {
                windows.Remove(dockManager);
            }
        }
    }

    public static void CloseAllWindows(DockManager dockManager)
    {
        if (windows.TryGetValue(dockManager, out List<DockWindow>? value))
        {
            foreach (DockWindow window in value)
            {
                window.Close();
            }
        }
    }
}
