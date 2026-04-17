using System.Runtime.InteropServices;
using Windows.Graphics;

namespace WinUI.Dock;

internal static unsafe partial class PointerHelpers
{
    #region Structures
    [StructLayout(LayoutKind.Sequential)]
    private struct LPPoint
    {
        public int X;

        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LPRect
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;

        public double Y;
    }
    #endregion

    #region Library Imports
    [LibraryImport("USER32.dll")]
    private static partial int GetCursorPos(LPPoint* lpPoint);

    [LibraryImport("USER32.dll")]
    private static partial short GetAsyncKeyState(int vKey);

    [LibraryImport("USER32.dll")]
    private static partial nint WindowFromPoint(LPPoint point);

    [LibraryImport("USER32.dll")]
    private static partial nint GetAncestor(nint hwnd, uint gaFlags);

    [LibraryImport("USER32.dll")]
    private static partial int ClientToScreen(nint hWnd, LPPoint* lpPoint);

    [LibraryImport("USER32.dll")]
    private static partial nint GetActiveWindow();

    [LibraryImport("USER32.dll")]
    private static partial nint GetWindowLongPtrW(nint hWnd, int nIndex);

    [LibraryImport("USER32.dll")]
    private static partial nint SetWindowLongPtrW(nint hWnd, int nIndex, nint dwNewLong);

    [LibraryImport("USER32.dll")]
    private static partial int GetWindowRect(nint hWnd, LPRect* lpRect);

    [LibraryImport("USER32.dll")]
    private static partial int SetLayeredWindowAttributes(nint hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [LibraryImport("libX11.so")]
    private static partial nint XOpenDisplay(nint display);

    [LibraryImport("libX11.so")]
    private static partial int XQueryPointer(nint display,
                                             nint window,
                                             out nint root,
                                             out nint child,
                                             out int rootX,
                                             out int rootY,
                                             out int winX,
                                             out int winY,
                                             out uint mask);

    [LibraryImport("libX11.so")]
    private static partial nint XDefaultRootWindow(nint display);

    [LibraryImport("libX11.so")]
    private static partial int XCloseDisplay(nint display);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial CGPoint CGEventGetLocation(nint eventRef);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial byte CGEventSourceButtonState(int stateID, uint button);
    #endregion

    #region Constants
    private const int VK_LBUTTON = 0x01;
    private const int VK_ESCAPE = 0x1B;
    private const uint GA_ROOT = 2;
    private const int GWL_EXSTYLE = -20;
    private const nint WS_EX_TRANSPARENT = 0x00000020;
    private const nint WS_EX_TOOLWINDOW = 0x00000080;
    private const nint WS_EX_TOPMOST = 0x00000008;
    private const nint WS_EX_NOACTIVATE = 0x08000000;
    private const nint WS_EX_LAYERED = 0x00080000;
    private const uint LWA_ALPHA = 0x02;
    #endregion

    private static readonly Func<PointInt32> getPointerPosition;
    private static readonly Func<bool> isPointerButtonPressed;
    private static readonly Func<bool> isEscapePressed;
    private static readonly Func<PointInt32, nint> getWindowAtScreenPoint;
    private static readonly Func<nint, PointInt32> getClientOrigin;

    static PointerHelpers()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            getPointerPosition = GetPointerPositionWindows;
            isPointerButtonPressed = IsPointerButtonPressedWindows;
            isEscapePressed = IsEscapePressedWindows;
            getWindowAtScreenPoint = GetWindowAtScreenPointWindows;
            getClientOrigin = GetClientOriginWindows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            getPointerPosition = GetPointerPositionLinux;
            isPointerButtonPressed = IsPointerButtonPressedLinux;
            isEscapePressed = static () => false;
            getWindowAtScreenPoint = static _ => nint.Zero;
            getClientOrigin = static _ => default;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            getPointerPosition = GetPointerPositionMacOS;
            isPointerButtonPressed = IsPointerButtonPressedMacOS;
            isEscapePressed = static () => false;
            getWindowAtScreenPoint = static _ => nint.Zero;
            getClientOrigin = static _ => default;
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform.");
        }
    }

    public static PointInt32 GetPointerPosition()
    {
        return getPointerPosition();
    }

    public static bool IsPointerButtonPressed()
    {
        return isPointerButtonPressed();
    }

    public static bool IsEscapePressed()
    {
        return isEscapePressed();
    }

    public static nint GetWindowAtScreenPoint(PointInt32 screenPoint)
    {
        return getWindowAtScreenPoint(screenPoint);
    }

    public static PointInt32 GetClientOrigin(nint hwnd)
    {
        return getClientOrigin(hwnd);
    }

    public static nint GetActiveWindowHandle()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? GetActiveWindow() : nint.Zero;
    }

    public static void SetWindowTransparent(nint hwnd)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || hwnd == nint.Zero)
        {
            return;
        }

        nint exStyle = GetWindowLongPtrW(hwnd, GWL_EXSTYLE);

        _ = SetWindowLongPtrW(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST);
    }

    public static void ClearWindowTransparent(nint hwnd)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || hwnd == nint.Zero)
        {
            return;
        }

        nint exStyle = GetWindowLongPtrW(hwnd, GWL_EXSTYLE);

        _ = SetWindowLongPtrW(hwnd, GWL_EXSTYLE, exStyle & ~(WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST));
    }

    public static bool IsPointInWindow(nint hwnd, PointInt32 point)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || hwnd == nint.Zero)
        {
            return false;
        }

        LPRect rect;

        if (GetWindowRect(hwnd, &rect) == 0)
        {
            return false;
        }

        return point.X >= rect.Left && point.X <= rect.Right
            && point.Y >= rect.Top && point.Y <= rect.Bottom;
    }

    public static void SetWindowAlpha(nint hwnd, byte alpha)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || hwnd == nint.Zero)
        {
            return;
        }

        nint exStyle = GetWindowLongPtrW(hwnd, GWL_EXSTYLE);

        _ = SetWindowLongPtrW(hwnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
        _ = SetLayeredWindowAttributes(hwnd, 0, alpha, LWA_ALPHA);
    }

    public static void ClearWindowAlpha(nint hwnd)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || hwnd == nint.Zero)
        {
            return;
        }

        nint exStyle = GetWindowLongPtrW(hwnd, GWL_EXSTYLE);

        _ = SetWindowLongPtrW(hwnd, GWL_EXSTYLE, exStyle & ~WS_EX_LAYERED);
    }

    private static PointInt32 GetPointerPositionWindows()
    {
        LPPoint point = default;

        _ = GetCursorPos(&point);

        return new()
        {
            X = point.X,
            Y = point.Y
        };
    }

    private static bool IsPointerButtonPressedWindows()
    {
        return (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
    }

    private static bool IsEscapePressedWindows()
    {
        return (GetAsyncKeyState(VK_ESCAPE) & 0x8000) != 0;
    }

    private static nint GetWindowAtScreenPointWindows(PointInt32 screenPoint)
    {
        LPPoint point = new() { X = screenPoint.X, Y = screenPoint.Y };

        nint hwnd = WindowFromPoint(point);

        return hwnd != nint.Zero ? GetAncestor(hwnd, GA_ROOT) : nint.Zero;
    }

    private static PointInt32 GetClientOriginWindows(nint hwnd)
    {
        LPPoint point = default;

        _ = ClientToScreen(hwnd, &point);

        return new()
        {
            X = point.X,
            Y = point.Y
        };
    }

    private static PointInt32 GetPointerPositionLinux()
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

        return new()
        {
            X = rootX,
            Y = rootY
        };
    }

    private static bool IsPointerButtonPressedLinux()
    {
        nint display = XOpenDisplay((nint)null);

        if (display == nint.Zero)
        {
            return false;
        }

        nint rootWindow = XDefaultRootWindow(display);

        _ = XQueryPointer(display,
                          rootWindow,
                          out _,
                          out _,
                          out _,
                          out _,
                          out _,
                          out _,
                          out uint mask);

        _ = XCloseDisplay(display);

        return (mask & 0x100) != 0;
    }

    private static PointInt32 GetPointerPositionMacOS()
    {
        CGPoint cgPoint = CGEventGetLocation((nint)null);

        return new()
        {
            X = (int)cgPoint.X,
            Y = (int)cgPoint.Y
        };
    }

    private static bool IsPointerButtonPressedMacOS()
    {
        return CGEventSourceButtonState(0, 0) != 0;
    }
}