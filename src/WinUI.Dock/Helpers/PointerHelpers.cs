using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.UI.Core;

namespace WinUI.Dock.Helpers;

public static unsafe partial class PointerHelpers
{
    [LibraryImport("USER32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(PointInt32* lpPoint);

    public static PointInt32 GetCursorPosition()
    {
        PointInt32 point = default;

#if WINDOWS
        GetCursorPos(&point);
#else
        if (CoreWindow.GetForCurrentThread() is CoreWindow coreWindow)
        {
            point.X = (int)coreWindow.PointerPosition.X;
            point.Y = (int)coreWindow.PointerPosition.Y;
        }
#endif

        return point;
    }
}