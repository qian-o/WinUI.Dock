using System.Runtime.InteropServices;
using Windows.Graphics;

namespace Dock.Helpers;

public static partial class PointerHelpers
{
    [LibraryImport("USER32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool GetCursorPos(PointInt32* lpPoint);

    public static unsafe PointInt32 GetCursorPosition()
    {
        PointInt32 point = default;

#if WINDOWS
        GetCursorPos(&point);
#endif

        return point;
    }
}
