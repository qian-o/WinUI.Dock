using System.Runtime.InteropServices;
using Windows.Graphics;
#if WINDOWS
using WinRT;
#endif
using WinRT.Interop;

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
    private static partial nint GetWindowLongPtrW(nint hWnd, int nIndex);

    [LibraryImport("USER32.dll")]
    private static partial nint SetWindowLongPtrW(nint hWnd, int nIndex, nint dwNewLong);

    [LibraryImport("USER32.dll")]
    private static partial int SetLayeredWindowAttributes(nint hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [LibraryImport("Dwmapi.dll")]
    private static partial int DwmExtendFrameIntoClientArea(nint hWnd, ref MARGINS pMarInset);

    [LibraryImport("Dwmapi.dll")]
    private static partial int DwmEnableBlurBehindWindow(nint hWnd, ref DWM_BLURBEHIND pBlurBehind);

    [LibraryImport("gdi32.dll")]
    private static partial nint CreateRectRgn(int x1, int y1, int x2, int y2);

    [LibraryImport("CoreMessaging.dll")]
    private static partial int CreateDispatcherQueueController(
        DispatcherQueueOptions options, out nint dispatcherQueueController);

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int Left, Right, Top, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DWM_BLURBEHIND
    {
        public uint dwFlags;
        public int fEnable;
        public nint hRgnBlur;
        public int fTransitionOnMaximized;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DispatcherQueueOptions
    {
        public int dwSize;
        public int threadType;
        public int apartmentType;
    }

#if WINDOWS
    private static Windows.UI.Composition.Compositor? _compositor;
#endif
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

    [LibraryImport("libX11.so")]
    private static partial int XGetWindowProperty(nint display, nint window, nint property,
                                                  long offset, long length,
                                                  [MarshalAs(UnmanagedType.I1)] bool delete,
                                                  nint reqType, out nint actualType,
                                                  out int actualFormat, out nuint nItems,
                                                  out nuint bytesAfter, out nint prop);

    [LibraryImport("libX11.so")]
    private static partial int XFree(nint data);
    #endregion

    #region Library Imports (macOS)
    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial nint CGEventCreate(nint source);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial CGPoint CGEventGetLocation(nint eventRef);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial byte CGEventSourceButtonState(int stateID, uint button);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial byte CGEventSourceKeyState(int stateID, ushort key);

    [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static partial void CFRelease(nint cf);

    [LibraryImport("libobjc.dylib")]
    private static partial nint sel_registerName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static partial void ObjCMsgSendBool(nint receiver, nint selector, [MarshalAs(UnmanagedType.I1)] bool value);

    [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static partial void ObjCMsgSendDouble(nint receiver, nint selector, double value);

    [LibraryImport("libobjc.dylib")]
    private static partial nint objc_getClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static partial nint ObjCMsgSend(nint receiver, nint selector);
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

    public static nint GetNativeWindowHandle(Window window)
    {
        try
        {
            nint handle = WindowNative.GetWindowHandle(window);

            if (handle != nint.Zero)
            {
                return handle;
            }
        }
        catch
        {
            // WindowNative may not be fully supported on all Uno backends.
        }

        // Platform-specific fallback: get the currently active/key window handle.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetKeyWindowMacOS();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetActiveWindowLinux();
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
            // For Uno Skia Desktop on Windows: use WS_EX_LAYERED.
            // (Uno does not use WS_EX_NOREDIRECTIONBITMAP / DirectComposition,
            //  so layered window alpha works here unlike native WinUI 3.)
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

    /// <summary>
    /// WinUI 3 window-level transparency using composition interop.
    /// Makes the window backdrop transparent and sets content opacity.
    /// </summary>
#if WINDOWS
    private static Windows.UI.Composition.Compositor EnsureCompositor()
    {
        if (_compositor is null)
        {
            // Windows.UI.Composition.Compositor needs a Windows.System.DispatcherQueue.
            // WinUI 3 may already have one; if not, create it.
            if (Windows.System.DispatcherQueue.GetForCurrentThread() is null)
            {
                DispatcherQueueOptions options = new()
                {
                    dwSize = Marshal.SizeOf<DispatcherQueueOptions>(),
                    threadType = 2,    // DQTYPE_THREAD_CURRENT
                    apartmentType = 2  // DQTAT_COM_STA
                };
                _ = CreateDispatcherQueueController(options, out _);
            }

            _compositor = new Windows.UI.Composition.Compositor();
        }

        return _compositor;
    }

    public static void SetWindowAlpha(Window window, double alpha)
    {
        try
        {
            // Layer 1: Remove the Win32 window background via DWM.
            nint hwnd = WindowNative.GetWindowHandle(window);

            if (hwnd != nint.Zero)
            {
                MARGINS margins = new() { Left = -1, Right = -1, Top = -1, Bottom = -1 };
                _ = DwmExtendFrameIntoClientArea(hwnd, ref margins);

                DWM_BLURBEHIND blur = new()
                {
                    dwFlags = 0x01 | 0x02,  // DWM_BB_ENABLE | DWM_BB_BLURREGION
                    fEnable = 1,             // TRUE
                    hRgnBlur = CreateRectRgn(-2, -2, -1, -1)
                };
                _ = DwmEnableBlurBehindWindow(hwnd, ref blur);
            }

            // Layer 2: Set a fully transparent composition backdrop
            //          to remove the XAML default opaque background.
            var target = window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>();
            var compositor = EnsureCompositor();
            target.SystemBackdrop = compositor.CreateColorBrush(
                Windows.UI.Color.FromArgb(0, 255, 255, 255));

            // Layer 3: Content opacity for the visual ghost effect.
            if (window.Content is UIElement content)
            {
                content.Opacity = alpha;
            }
        }
        catch
        {
            // Fallback: at minimum apply content opacity.
            if (window.Content is UIElement content)
            {
                content.Opacity = alpha;
            }
        }
    }

    /// <summary>
    /// Restore WinUI 3 window from transparent state.
    /// </summary>
    public static void ClearWindowAlpha(Window window)
    {
        try
        {
            var target = window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>();
            var backdrop = target.SystemBackdrop;
            target.SystemBackdrop = null;
            backdrop?.Dispose();

            if (window.Content is UIElement content)
            {
                content.Opacity = 1.0;
            }
        }
        catch
        {
            if (window.Content is UIElement content)
            {
                content.Opacity = 1.0;
            }
        }
    }
#endif

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

    private static nint GetActiveWindowLinux()
    {
        nint display = XOpenDisplay(nint.Zero);

        if (display == nint.Zero)
        {
            return nint.Zero;
        }

        nint root = XDefaultRootWindow(display);
        nint atom = XInternAtom(display, "_NET_ACTIVE_WINDOW", true);

        if (atom == nint.Zero)
        {
            _ = XCloseDisplay(display);

            return nint.Zero;
        }

        _ = XGetWindowProperty(display, root, atom, 0, 1, false, nint.Zero,
                               out _, out _, out nuint nItems, out _, out nint prop);

        nint result = nint.Zero;

        if (nItems > 0 && prop != nint.Zero)
        {
            result = *(nint*)prop;

            _ = XFree(prop);
        }

        _ = XCloseDisplay(display);

        return result;
    }
    #endregion

    #region macOS
    private static PointInt32 GetPointerPositionMacOS()
    {
        nint evt = CGEventCreate(nint.Zero);
        CGPoint cgPoint = CGEventGetLocation(evt);
        CFRelease(evt);

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

    private static nint GetKeyWindowMacOS()
    {
        nint nsApp = objc_getClass("NSApplication");
        nint sharedApp = ObjCMsgSend(nsApp, sel_registerName("sharedApplication"));

        return ObjCMsgSend(sharedApp, sel_registerName("keyWindow"));
    }
    #endregion
}