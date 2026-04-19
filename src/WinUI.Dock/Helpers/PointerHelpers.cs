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
    private struct CGPoint
    {
        public double X;

        public double Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGSize
    {
        public double Width;

        public double Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public CGPoint Origin;

        public CGSize Size;
    }
    #endregion

    #region Library Imports (Windows)
    [LibraryImport("USER32.dll")]
    private static partial int GetCursorPos(LPPoint* lpPoint);

    [LibraryImport("USER32.dll")]
    private static partial short GetAsyncKeyState(int vKey);
    #endregion

    #region Library Imports (Linux)
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

    [LibraryImport("libX11.so")]
    private static partial int XQueryKeymap(nint display, byte* keys_return);
    #endregion

    #region Library Imports (macOS)
    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial uint CGMainDisplayID();

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial CGRect CGDisplayBounds(uint display);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial byte CGEventSourceKeyState(int stateID, ushort key);

    [LibraryImport("libobjc.dylib")]
    private static partial nint sel_registerName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [LibraryImport("libobjc.dylib")]
    private static partial nint objc_getClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static partial CGPoint ObjCMsgSend_CGPoint(nint receiver, nint selector);

    [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static partial nuint ObjCMsgSend_nuint(nint receiver, nint selector);
    #endregion

    #region Constants
    private const int VK_LBUTTON = 0x01;
    private const int VK_ESCAPE = 0x1B;
    private const int X11_ESCAPE_KEYCODE = 9;
    private const ushort MACOS_ESCAPE_KEYCODE = 53;
    #endregion

    private static readonly Func<PointInt32> getPointerPosition;
    private static readonly Func<bool> isPointerButtonPressed;
    private static readonly Func<bool> isEscapePressed;

    static PointerHelpers()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            getPointerPosition = GetPointerPositionWindows;
            isPointerButtonPressed = IsPointerButtonPressedWindows;
            isEscapePressed = IsEscapePressedWindows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            getPointerPosition = GetPointerPositionLinux;
            isPointerButtonPressed = IsPointerButtonPressedLinux;
            isEscapePressed = IsEscapePressedLinux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            getPointerPosition = GetPointerPositionMacOS;
            isPointerButtonPressed = IsPointerButtonPressedMacOS;
            isEscapePressed = IsEscapePressedMacOS;
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

    #region Windows
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
    #endregion

    #region Linux
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

    private static bool IsEscapePressedLinux()
    {
        nint display = XOpenDisplay((nint)null);

        if (display == nint.Zero)
        {
            return false;
        }

        byte* keys = stackalloc byte[32];

        _ = XQueryKeymap(display, keys);
        _ = XCloseDisplay(display);

        return (keys[X11_ESCAPE_KEYCODE / 8] & (1 << (X11_ESCAPE_KEYCODE % 8))) != 0;
    }
    #endregion

    #region macOS
    private static PointInt32 GetPointerPositionMacOS()
    {
        nint nsEvent = objc_getClass("NSEvent");
        CGPoint point = ObjCMsgSend_CGPoint(nsEvent, sel_registerName("mouseLocation"));
        CGRect bounds = CGDisplayBounds(CGMainDisplayID());

        return new()
        {
            X = (int)point.X,
            Y = (int)(bounds.Size.Height - point.Y)
        };
    }

    private static bool IsPointerButtonPressedMacOS()
    {
        nint nsEvent = objc_getClass("NSEvent");
        nuint buttons = ObjCMsgSend_nuint(nsEvent, sel_registerName("pressedMouseButtons"));

        return (buttons & 1) != 0;
    }

    private static bool IsEscapePressedMacOS()
    {
        return CGEventSourceKeyState(0, MACOS_ESCAPE_KEYCODE) != 0;
    }
    #endregion
}