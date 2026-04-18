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
    #endregion

    #region Library Imports (Windows)
    [LibraryImport("USER32.dll")]
    private static partial int GetCursorPos(LPPoint* lpPoint);

    [LibraryImport("USER32.dll")]
    private static partial short GetAsyncKeyState(int vKey);

    [LibraryImport("USER32.dll")]
    private static partial nint GetActiveWindow();

    [LibraryImport("USER32.dll")]
    private static partial nint GetWindowLongPtrW(nint hWnd, int nIndex);

    [LibraryImport("USER32.dll")]
    private static partial nint SetWindowLongPtrW(nint hWnd, int nIndex, nint dwNewLong);

    [LibraryImport("USER32.dll")]
    private static partial int SetLayeredWindowAttributes(nint hwnd, uint crKey, byte bAlpha, uint dwFlags);
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

    [LibraryImport("libX11.so", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint XInternAtom(nint display, string atomName, [MarshalAs(UnmanagedType.I1)] bool onlyIfExists);

    [LibraryImport("libX11.so")]
    private static partial int XGetWindowProperty(nint display,
                                                  nint window,
                                                  nint property,
                                                  nint longOffset,
                                                  nint longLength,
                                                  [MarshalAs(UnmanagedType.I1)] bool delete,
                                                  nint reqType,
                                                  out nint actualTypeReturn,
                                                  out int actualFormatReturn,
                                                  out nint nItemsReturn,
                                                  out nint bytesAfterReturn,
                                                  out nint propReturn);

    [LibraryImport("libX11.so")]
    private static partial int XFree(nint data);

    [LibraryImport("libXext.so.6")]
    private static partial void XShapeCombineRectangles(nint display, nint window, int destKind,
                                                        int xOff, int yOff, nint rectangles,
                                                        int nRects, int op, int ordering);

    [LibraryImport("libXext.so.6")]
    private static partial void XShapeCombineMask(nint display, nint window, int destKind,
                                                   int xOff, int yOff, nint src, int op);

    [LibraryImport("libX11.so")]
    private static partial int XChangeProperty(nint display, nint window, nint property,
                                               nint type, int format, int mode,
                                               byte* data, int nElements);

    [LibraryImport("libX11.so")]
    private static partial int XDeleteProperty(nint display, nint window, nint property);
    #endregion

    #region Library Imports (macOS)
    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial CGPoint CGEventGetLocation(nint eventRef);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial byte CGEventSourceButtonState(int stateID, uint button);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial byte CGEventSourceKeyState(int stateID, ushort key);

    [LibraryImport("libobjc.dylib")]
    private static partial nint objc_getClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [LibraryImport("libobjc.dylib")]
    private static partial nint sel_registerName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static partial nint ObjCMsgSend(nint receiver, nint selector);

    [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static partial void ObjCMsgSendBool(nint receiver, nint selector, [MarshalAs(UnmanagedType.I1)] bool value);

    [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static partial void ObjCMsgSendDouble(nint receiver, nint selector, double value);
    #endregion

    #region Constants
    private const int VK_LBUTTON = 0x01;
    private const int VK_ESCAPE = 0x1B;
    private const int GWL_EXSTYLE = -20;
    private const nint WS_EX_TRANSPARENT = 0x00000020;
    private const nint WS_EX_TOOLWINDOW = 0x00000080;
    private const nint WS_EX_TOPMOST = 0x00000008;
    private const nint WS_EX_NOACTIVATE = 0x08000000;
    private const nint WS_EX_LAYERED = 0x00080000;
    private const uint LWA_ALPHA = 0x02;
    private const int X11_ESCAPE_KEYCODE = 9;
    private const ushort MACOS_ESCAPE_KEYCODE = 53;
    private const int ShapeInput = 2;
    private const int ShapeSet = 0;
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

    public static nint GetActiveWindowHandle()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetActiveWindow();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetActiveWindowLinux();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetActiveWindowMacOS();
        }

        return nint.Zero;
    }

    public static void SetWindowTransparent(nint handle)
    {
        if (handle == nint.Zero)
        {
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            nint exStyle = GetWindowLongPtrW(handle, GWL_EXSTYLE);

            _ = SetWindowLongPtrW(handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            SetWindowTransparentLinux(handle);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            ObjCMsgSendBool(handle, sel_registerName("setIgnoresMouseEvents:"), true);
        }
    }

    public static void ClearWindowTransparent(nint handle)
    {
        if (handle == nint.Zero)
        {
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            nint exStyle = GetWindowLongPtrW(handle, GWL_EXSTYLE);

            _ = SetWindowLongPtrW(handle, GWL_EXSTYLE, exStyle & ~(WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            ClearWindowTransparentLinux(handle);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            ObjCMsgSendBool(handle, sel_registerName("setIgnoresMouseEvents:"), false);
        }
    }

    public static void SetWindowAlpha(nint handle, double alpha)
    {
        if (handle == nint.Zero)
        {
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            nint exStyle = GetWindowLongPtrW(handle, GWL_EXSTYLE);

            _ = SetWindowLongPtrW(handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
            _ = SetLayeredWindowAttributes(handle, 0, (byte)(alpha * 255), LWA_ALPHA);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            SetWindowAlphaLinux(handle, alpha);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            ObjCMsgSendDouble(handle, sel_registerName("setAlphaValue:"), alpha);
        }
    }

    public static void ClearWindowAlpha(nint handle)
    {
        if (handle == nint.Zero)
        {
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            nint exStyle = GetWindowLongPtrW(handle, GWL_EXSTYLE);

            _ = SetWindowLongPtrW(handle, GWL_EXSTYLE, exStyle & ~WS_EX_LAYERED);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            ClearWindowAlphaLinux(handle);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            ObjCMsgSendDouble(handle, sel_registerName("setAlphaValue:"), 1.0);
        }
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
    private static nint GetActiveWindowLinux()
    {
        nint display = XOpenDisplay(nint.Zero);

        if (display == nint.Zero)
        {
            return nint.Zero;
        }

        nint root = XDefaultRootWindow(display);
        nint atom = XInternAtom(display, "_NET_ACTIVE_WINDOW", false);

        int result = XGetWindowProperty(display, root, atom,
                                        0, 1, false, nint.Zero,
                                        out _, out _, out nint nItems,
                                        out _, out nint prop);

        nint activeWindow = nint.Zero;

        if (result == 0 && nItems > 0 && prop != nint.Zero)
        {
            activeWindow = Marshal.ReadIntPtr(prop);

            _ = XFree(prop);
        }

        _ = XCloseDisplay(display);

        return activeWindow;
    }

    private static void SetWindowTransparentLinux(nint xWindow)
    {
        nint display = XOpenDisplay(nint.Zero);

        if (display == nint.Zero)
        {
            return;
        }

        // Set input shape to empty region → window becomes click-through.
        XShapeCombineRectangles(display, xWindow, ShapeInput, 0, 0, nint.Zero, 0, ShapeSet, 0);

        _ = XCloseDisplay(display);
    }

    private static void ClearWindowTransparentLinux(nint xWindow)
    {
        nint display = XOpenDisplay(nint.Zero);

        if (display == nint.Zero)
        {
            return;
        }

        // Remove input shape → restore default rectangular input region.
        XShapeCombineMask(display, xWindow, ShapeInput, 0, 0, nint.Zero, ShapeSet);

        _ = XCloseDisplay(display);
    }

    private static void SetWindowAlphaLinux(nint xWindow, double alpha)
    {
        nint display = XOpenDisplay(nint.Zero);

        if (display == nint.Zero)
        {
            return;
        }

        nint opacityAtom = XInternAtom(display, "_NET_WM_WINDOW_OPACITY", false);
        nint cardinalAtom = XInternAtom(display, "CARDINAL", false);

        // _NET_WM_WINDOW_OPACITY uses a 32-bit cardinal, 0 = fully transparent, 0xFFFFFFFF = fully opaque.
        uint opacityValue = (uint)(alpha * 0xFFFFFFFF);

        _ = XChangeProperty(display, xWindow, opacityAtom, cardinalAtom, 32, 0, (byte*)&opacityValue, 1);
        _ = XCloseDisplay(display);
    }

    private static void ClearWindowAlphaLinux(nint xWindow)
    {
        nint display = XOpenDisplay(nint.Zero);

        if (display == nint.Zero)
        {
            return;
        }

        nint opacityAtom = XInternAtom(display, "_NET_WM_WINDOW_OPACITY", false);

        _ = XDeleteProperty(display, xWindow, opacityAtom);
        _ = XCloseDisplay(display);
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
    private static nint GetActiveWindowMacOS()
    {
        nint nsAppClass = objc_getClass("NSApplication");
        nint sharedApp = ObjCMsgSend(nsAppClass, sel_registerName("sharedApplication"));

        return ObjCMsgSend(sharedApp, sel_registerName("keyWindow"));
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

    private static bool IsEscapePressedMacOS()
    {
        return CGEventSourceKeyState(0, MACOS_ESCAPE_KEYCODE) != 0;
    }
    #endregion
}