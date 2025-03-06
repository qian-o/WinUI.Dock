using System.Runtime.InteropServices;
using Windows.Graphics;

namespace WinUI.Dock.Helpers;

public static unsafe partial class PointerHelpers
{
    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;
    }

    [LibraryImport("USER32.dll")]
    private static partial int GetCursorPos(PointInt32* lpPoint);

    [LibraryImport("libX11.so")]
    private static partial nint XOpenDisplay(nint display);

    [LibraryImport("libX11.so")]
    private static partial int XQueryPointer(nint display,
                                             nint window,
                                             out nint root_return,
                                             out nint child_return,
                                             out int root_x_return,
                                             out int root_y_return,
                                             out int win_x_return,
                                             out int win_y_return,
                                             out uint mask_return);

    [LibraryImport("libX11.so")]
    private static partial nint XDefaultRootWindow(nint display);

    [LibraryImport("libX11.so")]
    private static partial int XCloseDisplay(nint display);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial CGPoint CGEventGetLocation(nint eventRef);

    public static PointInt32 GetPointerPosition()
    {
        PointInt32 point = default;

        if (OperatingSystem.IsWindows())
        {
            _ = GetCursorPos(&point);
        }
        else if (OperatingSystem.IsLinux())
        {
            nint display = XOpenDisplay((nint)null);

            if (display == nint.Zero)
            {
                throw new InvalidOperationException("Failed to open X display.");
            }

            nint rootWindow = XDefaultRootWindow(display);

            if (rootWindow == nint.Zero)
            {
                throw new InvalidOperationException("Failed to get root window.");
            }

            _ = XQueryPointer(display,
                              rootWindow,
                              out _,
                              out _,
                              out int rootX,
                              out int rootY,
                              out _,
                              out _,
                              out _);

            _ = XCloseDisplay(display);

            point.X = rootX;
            point.Y = rootY;
        }
        else if (OperatingSystem.IsMacOS())
        {
            CGPoint cgPoint = CGEventGetLocation(0);

            point.X = (int)cgPoint.X;
            point.Y = (int)cgPoint.Y;
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        return point;
    }
}