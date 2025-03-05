using System.Runtime.InteropServices;
using Windows.Graphics;

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
#endif

        return point;
    }
}