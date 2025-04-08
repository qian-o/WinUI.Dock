using WinUI.Dock.Controls;

namespace WinUI.Dock.Helpers;

internal static class DockWindowHelpers
{
    private static readonly Dictionary<DockManager, List<DockWindow>> windows = [];

    public static void AddWindow(DockManager manager, DockWindow window)
    {
        if (!windows.TryGetValue(manager, out List<DockWindow>? value))
        {
            windows[manager] = value = [];
        }

        value.Add(window);
    }

    public static void RemoveWindow(DockManager manager, DockWindow window)
    {
        if (windows.TryGetValue(manager, out List<DockWindow>? value))
        {
            value.Remove(window);

            if (value.Count is 0)
            {
                windows.Remove(manager);
            }
        }
    }

    public static DockWindow[] GetWindows(DockManager manager)
    {
        return windows.TryGetValue(manager, out List<DockWindow>? value) ? [.. value] : [];
    }

    public static void CloseWindows(DockManager manager)
    {
        if (windows.TryGetValue(manager, out List<DockWindow>? value))
        {
            foreach (DockWindow window in value.ToArray())
            {
                window.Close();
            }

            windows.Remove(manager);
        }
    }
}
