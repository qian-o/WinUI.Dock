using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Windows.Foundation;
using Windows.Graphics;

namespace WinUI.Dock;

internal static class DragService
{
    private const int DragThreshold = 5;
    private const int FloatingThreshold = 30;
    private const int PollIntervalMs = 16;
    private const int FloatingWindowTitleBarHeight = 32;

    // Timer
    private static DispatcherQueueTimer? pollTimer;

    // Source state
    private static Document? dragDocument;
    private static DocumentGroup? sourceGroup;
    private static int sourceIndex;
    private static PointInt32 startScreenPoint;
    private static DockManager? sourceManager;
    private static SidePopup? sourceSidePopup;
    private static FloatingWindow? sourceFloatingWindow;
    private static FrameworkElement? sourceTabElement;

    // Main window (cross-platform via AppWindow)
    private static AppWindow? mainWindowAppWindow;
    private static PointInt32 mainWindowClientOrigin;

    // Drag mode
    private static bool isActive;
    private static bool isTabReorder;
    private static bool isFloatingDrag;

    // Preview window (real FloatingWindow used as semi-transparent drag preview)
    private static FloatingWindow? previewFloatingWindow;
    private static nint previewNativeHandle;
    private static PointInt32 previewOffset;
    private static SizeInt32 capturedSourceSize;

    // Hit-test state
    private static DockManager? hoveredManager;
    private static FloatingWindow? hoveredFloatingWindow;
    private static DocumentGroup? hoveredGroup;
    private static DockTarget? hoveredTarget;

    // Pointer-event-based button state (cross-platform, does not rely on P/Invoke).
    private static bool pointerReleasedByEvent;
    private static bool hasPointerCapture;

    public static bool IsDragging => isActive;

    public static void BeginTabDrag(Document document, DocumentGroup group, int index, FrameworkElement tabElement, Point cursorInClient)
    {
        if (isActive)
        {
            return;
        }

        try
        {
            dragDocument = document;
            sourceGroup = group;
            sourceIndex = index;
            sourceManager = document.Root!;
            sourceSidePopup = null;
            sourceTabElement = tabElement;
            startScreenPoint = PointerHelpers.GetPointerPosition();
            hasPointerCapture = true;

            DetectSourceWindow(cursorInClient);
            StartTimer();
        }
        catch
        {
            Cleanup();
        }
    }

    public static void BeginSidePopupDrag(Document document, DockManager manager, SidePopup popup, Point cursorInClient)
    {
        if (isActive)
        {
            return;
        }

        try
        {
            dragDocument = document;
            sourceGroup = null;
            sourceIndex = -1;
            sourceManager = manager;
            sourceSidePopup = popup;
            startScreenPoint = PointerHelpers.GetPointerPosition();
            hasPointerCapture = true;

            DetectSourceWindow(cursorInClient);
            StartTimer();
        }
        catch
        {
            Cleanup();
        }
    }

    /// <summary>
    /// Called by the originating element's PointerReleased/PointerCaptureLost handler
    /// to signal that the pointer button has been released.
    /// </summary>
    public static void NotifyPointerReleased()
    {
        pointerReleasedByEvent = true;
    }

    /// <summary>
    /// Called when the originating element loses pointer capture
    /// (e.g., when the element is removed from the visual tree during floating drag).
    /// </summary>
    public static void NotifyCaptureLost()
    {
        hasPointerCapture = false;
    }

    public static void Cancel()
    {
        if (!isActive)
        {
            return;
        }

        HideAllDockTargets();

        if (isFloatingDrag && dragDocument is not null)
        {
            // Detach document from the preview FloatingWindow.
            dragDocument.Detach();
            ClosePreviewWindow();

            if (sourceSidePopup is not null)
            {
                // SidePopup content was already detached — cannot restore, create floating window.
                new FloatingWindow(sourceManager!, dragDocument).Activate();
            }
            else if (sourceGroup is { Owner: not null })
            {
                int idx = Math.Min(sourceIndex, sourceGroup.Children.Count);

                sourceGroup.Children.Insert(idx, dragDocument);
            }
            else
            {
                // Source group was destroyed — create floating window.
                new FloatingWindow(sourceManager!, dragDocument).Activate();
            }
        }
        else
        {
            ClosePreviewWindow();
        }

        Cleanup();
    }

    private static void DetectSourceWindow(Point cursorInClient)
    {
        // Determine if the drag started from a FloatingWindow.
        sourceFloatingWindow = null;

        if (sourceManager is not null)
        {
            foreach (FloatingWindow fw in FloatingWindowHelpers.GetWindows(sourceManager))
            {
                PointInt32 pos = fw.AppWindow.Position;
                SizeInt32 size = fw.AppWindow.Size;

                if (startScreenPoint.X >= pos.X && startScreenPoint.X <= pos.X + size.Width &&
                    startScreenPoint.Y >= pos.Y && startScreenPoint.Y <= pos.Y + size.Height)
                {
                    sourceFloatingWindow = fw;

                    break;
                }
            }
        }

        // If not from a FloatingWindow, capture the main window's AppWindow.
        if (sourceFloatingWindow is null)
        {
            mainWindowAppWindow = sourceManager?.Behavior?.MainWindow?.AppWindow;

            // Compute client origin from the cursor screen position and its visual-tree position.
            // clientOrigin = screenPoint - cursorInClient * scale
            double scale = sourceManager?.XamlRoot?.RasterizationScale ?? 1.0;

            mainWindowClientOrigin = new PointInt32
            {
                X = startScreenPoint.X - (int)(cursorInClient.X * scale),
                Y = startScreenPoint.Y - (int)(cursorInClient.Y * scale)
            };
        }
    }

    private static void StartTimer()
    {
        isActive = true;
        isTabReorder = false;
        isFloatingDrag = false;
        pointerReleasedByEvent = false;

        pollTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        pollTimer.Interval = TimeSpan.FromMilliseconds(PollIntervalMs);
        pollTimer.Tick += OnTimerTick;
        pollTimer.Start();
    }

    private static void OnTimerTick(DispatcherQueueTimer sender, object args)
    {
        // Use pointer-event flag as primary signal (works cross-platform).
        // Fall back to P/Invoke only when pointer capture has been lost
        // (e.g., during floating drag after the source element is destroyed).
        if (pointerReleasedByEvent || (!hasPointerCapture && !PointerHelpers.IsPointerButtonPressed()))
        {
            CompleteDrag();

            return;
        }

        if (PointerHelpers.IsEscapePressed())
        {
            Cancel();

            return;
        }

        PointInt32 screenPoint = PointerHelpers.GetPointerPosition();

        if (!isTabReorder && !isFloatingDrag)
        {
            int dx = screenPoint.X - startScreenPoint.X;
            int dy = screenPoint.Y - startScreenPoint.Y;

            if (dx * dx + dy * dy < DragThreshold * DragThreshold)
            {
                return;
            }

            if (sourceSidePopup is not null)
            {
                StartFloatingDrag();
            }
            else if (Math.Abs(dy) > FloatingThreshold)
            {
                StartFloatingDrag();
            }
            else
            {
                isTabReorder = true;
            }
        }

        if (isTabReorder)
        {
            HandleTabReorder(screenPoint);
        }
        else if (isFloatingDrag)
        {
            UpdateFloatingDrag(screenPoint);
        }
    }

    private static void HandleTabReorder(PointInt32 screenPoint)
    {
        if (sourceGroup is null || dragDocument is null)
        {
            return;
        }

        if (Math.Abs(screenPoint.Y - startScreenPoint.Y) > FloatingThreshold)
        {
            StartFloatingDrag();

            return;
        }

        int currentIndex = sourceGroup.Children.IndexOf(dragDocument);

        if (currentIndex is -1)
        {
            return;
        }

        int targetIndex = CalculateTargetTabIndex(screenPoint);

        if (targetIndex is not -1 && targetIndex != currentIndex)
        {
            sourceGroup.Children.Move(currentIndex, targetIndex);
        }
    }

    private static int CalculateTargetTabIndex(PointInt32 screenPoint)
    {
        if (sourceGroup is null)
        {
            return -1;
        }

        int childCount = sourceGroup.Children.Count;

        if (childCount is 0)
        {
            return -1;
        }

        double scale = sourceGroup.XamlRoot?.RasterizationScale ?? 1.0;

        // Use actual rendered tab width instead of estimated group-based width.
        double actualTabWidth = sourceGroup.GetActualTabItemWidth();

        if (actualTabWidth <= 0)
        {
            return -1;
        }

        double estimatedTabWidth = actualTabWidth * scale;

        int dx = screenPoint.X - startScreenPoint.X;
        int targetIndex = sourceIndex + (int)Math.Round(dx / estimatedTabWidth);

        return Math.Clamp(targetIndex, 0, childCount - 1);
    }

    private static void StartFloatingDrag()
    {
        isTabReorder = false;
        isFloatingDrag = true;

        // Capture the actual rendered size BEFORE the document is detached.
        if (sourceFloatingWindow is not null)
        {
            // Dragging from a FloatingWindow — use the window's own size (already includes title bar).
            capturedSourceSize = sourceFloatingWindow.AppWindow.Size;
        }
        else if (sourceGroup is not null)
        {
            double scale = sourceGroup.XamlRoot?.RasterizationScale ?? 1.0;
            int titleBarPixels = (int)(FloatingWindowTitleBarHeight * scale);

            capturedSourceSize = new SizeInt32
            {
                Width = (int)(sourceGroup.ActualWidth * scale),
                // Add the FloatingWindow title bar height since the preview is a FloatingWindow.
                Height = (int)(sourceGroup.ActualHeight * scale) + titleBarPixels
            };
        }
        else
        {
            capturedSourceSize = default;
        }

        if (sourceSidePopup is not null)
        {
            sourceSidePopup.DetachForDrag();
        }

        // FloatingWindow constructor will detach the document from its source.
        CreatePreviewWindow();

        // Close the source FloatingWindow if it became empty after detaching the document.
        // This prevents it from blocking hit-testing during the drag.
        if (sourceFloatingWindow is not null && sourceManager is not null)
        {
            FloatingWindowHelpers.CloseEmptyWindows(sourceManager);
            sourceFloatingWindow = null;
        }
    }

    private static void UpdateFloatingDrag(PointInt32 screenPoint)
    {
        MovePreviewWindow(screenPoint);

        // Determine which window the cursor is over.
        DockManager? manager = null;
        FloatingWindow? floatingWindow = null;

        // Check floating windows first (they're on top).
        if (sourceManager is not null)
        {
            foreach (FloatingWindow fw in FloatingWindowHelpers.GetWindows(sourceManager))
            {
                if (fw == previewFloatingWindow)
                {
                    continue;
                }

                PointInt32 pos = fw.AppWindow.Position;
                SizeInt32 size = fw.AppWindow.Size;

                if (screenPoint.X >= pos.X && screenPoint.X <= pos.X + size.Width &&
                    screenPoint.Y >= pos.Y && screenPoint.Y <= pos.Y + size.Height)
                {
                    floatingWindow = fw;
                    manager = sourceManager;

                    break;
                }
            }
        }

        // If not in any floating window, check main window via AppWindow bounds.
        if (manager is null && mainWindowAppWindow is not null && sourceManager is not null)
        {
            PointInt32 pos = mainWindowAppWindow.Position;
            SizeInt32 size = mainWindowAppWindow.Size;

            if (screenPoint.X >= pos.X && screenPoint.X < pos.X + size.Width &&
                screenPoint.Y >= pos.Y && screenPoint.Y < pos.Y + size.Height)
            {
                manager = sourceManager;
            }
        }

        // Window changed?
        if (manager != hoveredManager || floatingWindow != hoveredFloatingWindow)
        {
            if (hoveredManager is not null)
            {
                hoveredManager.HideDockTargets();
                hoveredManager.HideDockPreview();
            }

            if (hoveredGroup is not null)
            {
                hoveredGroup.HideDockTargets();
                hoveredGroup.HideDockPreview();
                hoveredGroup = null;
            }

            hoveredTarget = null;
            hoveredManager = manager;
            hoveredFloatingWindow = floatingWindow;

            if (hoveredManager is not null)
            {
                // Activate the target window so it comes to front (behind the preview).
                if (floatingWindow is not null)
                {
                    floatingWindow.Activate();
                }
                else
                {
                    hoveredManager.Behavior?.MainWindow?.Activate();
                }

                hoveredManager.ShowDockTargets();
            }
        }

        if (manager is null)
        {
            hoveredTarget = null;

            return;
        }

        // Convert screen point to manager/panel local coordinates.
        double localX, localY;

        if (floatingWindow is not null)
        {
            PointInt32 fwPos = floatingWindow.AppWindow.Position;
            double scale = floatingWindow.Panel.XamlRoot?.RasterizationScale ?? 1.0;

            try
            {
                Point panelPos = floatingWindow.Panel.TransformToVisual(null).TransformPoint(new Point(0, 0));

                localX = (screenPoint.X - fwPos.X) / scale - panelPos.X;
                localY = (screenPoint.Y - fwPos.Y) / scale - panelPos.Y;
            }
            catch
            {
                return;
            }

            // Floating windows only have DocumentGroup-level targets.
            UpdateGroupHitTest(floatingWindow.Panel, screenPoint, floatingWindow.AppWindow);

            return;
        }

        // On the main window — use cached client origin for coordinate conversion.
        double managerScale = manager.XamlRoot?.RasterizationScale ?? 1.0;

        try
        {
            Point managerPos = manager.TransformToVisual(null).TransformPoint(new Point(0, 0));

            localX = (screenPoint.X - mainWindowClientOrigin.X) / managerScale - managerPos.X;
            localY = (screenPoint.Y - mainWindowClientOrigin.Y) / managerScale - managerPos.Y;
        }
        catch
        {
            return;
        }

        // Check manager-level dock targets (edges).
        bool hasPanel = manager.Panel is not null && manager.Panel.Children.Count is not 0;
        DockTarget? managerTarget = GetManagerDockTarget(localX, localY, manager.ActualWidth, manager.ActualHeight, hasPanel);

        if (managerTarget.HasValue)
        {
            if (hoveredGroup is not null)
            {
                hoveredGroup.HideDockTargets();
                hoveredGroup.HideDockPreview();
                hoveredGroup = null;
            }

            if (hoveredTarget != managerTarget)
            {
                manager.HideDockPreview();
                hoveredTarget = managerTarget;
                manager.ShowDockPreview(dragDocument!, managerTarget.Value);
            }

            return;
        }

        // Not on manager edges — check DocumentGroups.
        if (manager.Panel is not null)
        {
            UpdateGroupHitTest(manager.Panel, screenPoint, mainWindowClientOrigin, managerScale);
        }
    }

    private static void UpdateGroupHitTest(LayoutPanel panel, PointInt32 screenPoint, AppWindow appWindow)
    {
        double scale = panel.XamlRoot?.RasterizationScale ?? 1.0;

        foreach (DocumentGroup group in GetAllDocumentGroups(panel))
        {
            try
            {
                PointInt32 fwPos = appWindow.Position;
                Point groupPos = group.TransformToVisual(null).TransformPoint(new Point(0, 0));

                double groupScreenX = fwPos.X + groupPos.X * scale;
                double groupScreenY = fwPos.Y + groupPos.Y * scale;
                double groupScreenW = group.ActualWidth * scale;
                double groupScreenH = group.ActualHeight * scale;

                if (screenPoint.X >= groupScreenX && screenPoint.X <= groupScreenX + groupScreenW &&
                    screenPoint.Y >= groupScreenY && screenPoint.Y <= groupScreenY + groupScreenH)
                {
                    double groupLocalX = (screenPoint.X - groupScreenX) / scale;
                    double groupLocalY = (screenPoint.Y - groupScreenY) / scale;

                    UpdateGroupTarget(group, groupLocalX, groupLocalY);

                    return;
                }
            }
            catch
            {
                // TransformToVisual can fail if element is not in tree.
            }
        }

        // Not in any group.
        if (hoveredGroup is not null)
        {
            hoveredGroup.HideDockTargets();
            hoveredGroup.HideDockPreview();
            hoveredGroup = null;
            hoveredTarget = null;
        }

        hoveredManager?.HideDockPreview();
    }

    private static void UpdateGroupHitTest(LayoutPanel panel, PointInt32 screenPoint, PointInt32 clientOrigin, double scale)
    {
        foreach (DocumentGroup group in GetAllDocumentGroups(panel))
        {
            if (ElementContainsScreenPoint(group, screenPoint, clientOrigin, scale))
            {
                Point groupPos;

                try
                {
                    groupPos = group.TransformToVisual(null).TransformPoint(new Point(0, 0));
                }
                catch
                {
                    continue;
                }

                double groupLocalX = (screenPoint.X - clientOrigin.X) / scale - groupPos.X;
                double groupLocalY = (screenPoint.Y - clientOrigin.Y) / scale - groupPos.Y;

                UpdateGroupTarget(group, groupLocalX, groupLocalY);

                return;
            }
        }

        // Not in any group.
        if (hoveredGroup is not null)
        {
            hoveredGroup.HideDockTargets();
            hoveredGroup.HideDockPreview();
            hoveredGroup = null;
            hoveredTarget = null;
        }

        hoveredManager?.HideDockPreview();
    }

    private static void UpdateGroupTarget(DocumentGroup group, double groupLocalX, double groupLocalY)
    {
        if (group != hoveredGroup)
        {
            if (hoveredGroup is not null)
            {
                hoveredGroup.HideDockTargets();
                hoveredGroup.HideDockPreview();
            }

            hoveredManager?.HideDockPreview();
            hoveredTarget = null;
            hoveredGroup = group;
            hoveredGroup.ShowDockTargets();
        }

        DockTarget? groupTarget = GetGroupDockTarget(groupLocalX, groupLocalY, group.ActualWidth, group.ActualHeight);

        if (groupTarget != hoveredTarget)
        {
            group.HideDockPreview();
            hoveredTarget = groupTarget;

            if (hoveredTarget.HasValue)
            {
                group.ShowDockPreview(hoveredTarget.Value);
            }
        }
    }

    private static void CompleteDrag()
    {
        HideAllDockTargets();

        if (isFloatingDrag && dragDocument is not null)
        {
            if (hoveredTarget.HasValue)
            {
                // Detach from preview, close it, then dock.
                dragDocument.Detach();
                ClosePreviewWindow();

                if (hoveredGroup is not null)
                {
                    hoveredGroup.HideDockPreview();
                    hoveredGroup.Dock(dragDocument, hoveredTarget.Value);
                }
                else if (hoveredManager is not null)
                {
                    dragDocument.ResetPreferredSide(hoveredTarget.Value);
                    hoveredManager.HideDockPreview();
                    hoveredManager.Dock(dragDocument, hoveredTarget.Value);
                }
                else
                {
                    // Safety fallback: target set but no container — float.
                    FloatingWindow fw = new(sourceManager!, dragDocument);
                    fw.Activate();
                }

                dragDocument.Root?.HideDockTargets();
            }
            else
            {
                // No target — promote the preview FloatingWindow to a normal one in-place.
                PromotePreviewToFloatingWindow();
            }

            if (sourceManager is not null)
            {
                FloatingWindowHelpers.CloseEmptyWindows(sourceManager);
            }
        }

        Cleanup();
    }

    private static void CreatePreviewWindow()
    {
        if (dragDocument is null || sourceManager is null)
        {
            return;
        }

        // Calculate offset BEFORE the document is detached, while source elements are in the visual tree.
        PointInt32 cursorPos = PointerHelpers.GetPointerPosition();
        int offsetX;
        int offsetY;

        if (sourceTabElement is not null)
        {
            // Use cursor position relative to the DockTabItem being dragged.
            // Works for both main window and FloatingWindow sources.
            try
            {
                double scale = sourceTabElement.XamlRoot?.RasterizationScale ?? 1.0;
                Point tabPos = sourceTabElement.TransformToVisual(null).TransformPoint(new Point(0, 0));
                double tabScreenX, tabScreenY;

                if (sourceFloatingWindow is not null)
                {
                    PointInt32 fwPos = sourceFloatingWindow.AppWindow.Position;

                    tabScreenX = fwPos.X + tabPos.X * scale;
                    tabScreenY = fwPos.Y + tabPos.Y * scale;
                }
                else if (mainWindowAppWindow is not null)
                {
                    tabScreenX = mainWindowClientOrigin.X + tabPos.X * scale;
                    tabScreenY = mainWindowClientOrigin.Y + tabPos.Y * scale;
                }
                else
                {
                    tabScreenX = cursorPos.X - 40;
                    tabScreenY = cursorPos.Y - 16;
                }

                offsetX = Math.Max(8, cursorPos.X - (int)tabScreenX);
                offsetY = Math.Max(8, cursorPos.Y - (int)tabScreenY);
            }
            catch
            {
                offsetX = 40;
                offsetY = 16;
            }
        }
        else
        {
            offsetX = 40;
            offsetY = 16;
        }

        // Create a real FloatingWindow as the drag preview.
        // The constructor detaches the document from its source and builds the proper structure.
        previewFloatingWindow = new FloatingWindow(sourceManager, dragDocument);

        // Override with captured source size so the preview matches the original control size.
        if (capturedSourceSize.Width > 0 && capturedSourceSize.Height > 0)
        {
            previewFloatingWindow.AppWindow.Resize(capturedSourceSize);
        }

        // Apply offset to the preview window.
        int previewWidth = previewFloatingWindow.AppWindow.Size.Width;
        int previewHeight = previewFloatingWindow.AppWindow.Size.Height;
        previewOffset = new PointInt32
        {
            X = Math.Clamp(offsetX, 8, Math.Max(8, previewWidth - 8)),
            Y = Math.Clamp(offsetY, 8, Math.Max(8, previewHeight - 8))
        };

        // Reposition using the calculated offset.
        previewFloatingWindow.AppWindow.Move(new PointInt32
        {
            X = cursorPos.X - previewOffset.X,
            Y = cursorPos.Y - previewOffset.Y
        });

        // Make always-on-top during drag.
        if (previewFloatingWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsAlwaysOnTop = true;
        }

        previewFloatingWindow.Activate();

        // Make the preview window semi-transparent.
        // On Windows: uses DWM + ICompositionSupportsSystemBackdrop + Content.Opacity
        //   (WS_EX_LAYERED is incompatible with WinUI 3 DirectComposition).
        // On macOS: uses NSWindow.setAlphaValue: via objc_msgSend.
        // On Linux: uses _NET_WM_WINDOW_OPACITY X11 property.
#if WINDOWS
        PointerHelpers.SetWindowAlpha(previewFloatingWindow, 0.6);
#else
        // Defer to next dispatch cycle so the native window is ready.
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
        {
            if (previewFloatingWindow is null)
            {
                return;
            }

            previewNativeHandle = PointerHelpers.GetNativeWindowHandle(previewFloatingWindow);
            PointerHelpers.SetWindowAlpha(previewNativeHandle, 0.6);
        });
#endif

        // OS-level click-through via P/Invoke.
        // Defer to the next dispatch cycle so WinUI has finished
        // processing Activate() and applying its own window styles.
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
        {
            if (previewFloatingWindow is null)
            {
                return;
            }

            if (previewNativeHandle == nint.Zero)
            {
                previewNativeHandle = PointerHelpers.GetNativeWindowHandle(previewFloatingWindow);
            }

            PointerHelpers.SetWindowTransparent(previewNativeHandle);
        });
    }

    private static void MovePreviewWindow(PointInt32 screenPoint)
    {
        previewFloatingWindow?.AppWindow.Move(new PointInt32
        {
            X = screenPoint.X - previewOffset.X,
            Y = screenPoint.Y - previewOffset.Y
        });
    }

    private static void ClosePreviewWindow()
    {
        if (previewFloatingWindow is not null)
        {
            previewFloatingWindow.Close();
            previewFloatingWindow = null;
            previewNativeHandle = nint.Zero;
        }
    }

    private static void PromotePreviewToFloatingWindow()
    {
        if (previewFloatingWindow is null)
        {
            return;
        }

        // Remove always-on-top.
        if (previewFloatingWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsAlwaysOnTop = false;
        }

        // Restore window state.
#if WINDOWS
        PointerHelpers.ClearWindowAlpha(previewFloatingWindow);
#else
        PointerHelpers.ClearWindowAlpha(previewNativeHandle);
#endif

        PointerHelpers.ClearWindowTransparent(previewNativeHandle);

        // Detach from DragService tracking — the FloatingWindow is now a normal one.
        previewFloatingWindow = null;
        previewNativeHandle = nint.Zero;
    }

    private static void HideAllDockTargets()
    {
        if (hoveredGroup is not null)
        {
            hoveredGroup.HideDockTargets();
            hoveredGroup.HideDockPreview();
        }

        if (hoveredManager is not null)
        {
            hoveredManager.HideDockTargets();
            hoveredManager.HideDockPreview();
        }
    }

    private static DockTarget? GetManagerDockTarget(double localX, double localY, double managerW, double managerH, bool hasPanel)
    {
        const double edgeSize = 36;

        if (localX >= 0 && localX < edgeSize && localY >= 0 && localY <= managerH)
        {
            return DockTarget.DockLeft;
        }

        if (localX > managerW - edgeSize && localX <= managerW && localY >= 0 && localY <= managerH)
        {
            return DockTarget.DockRight;
        }

        if (localY >= 0 && localY < edgeSize && localX >= 0 && localX <= managerW)
        {
            return DockTarget.DockTop;
        }

        if (localY > managerH - edgeSize && localY <= managerH && localX >= 0 && localX <= managerW)
        {
            return DockTarget.DockBottom;
        }

        if (!hasPanel)
        {
            double cx = managerW / 2;
            double cy = managerH / 2;

            if (Math.Abs(localX - cx) <= 20 && Math.Abs(localY - cy) <= 20)
            {
                return DockTarget.Center;
            }
        }

        return null;
    }

    private static DockTarget? GetGroupDockTarget(double localX, double localY, double groupW, double groupH)
    {
        double cx = groupW / 2;
        double cy = groupH / 2;

        // Only detect within the center 124×124 area (matching DockTargets overlay size).
        if (Math.Abs(localX - cx) > 62 || Math.Abs(localY - cy) > 62)
        {
            return null;
        }

        // Grid-relative coordinates within the 120×120 inner area.
        double gridX = (groupW - 124) / 2 + 2;
        double gridY = (groupH - 124) / 2 + 2;
        double innerX = localX - gridX;
        double innerY = localY - gridY;

        if (innerX < 0 || innerX >= 120 || innerY < 0 || innerY >= 120)
        {
            return null;
        }

        const double cellSize = 40;

        int col = (int)(innerX / cellSize);
        int row = (int)(innerY / cellSize);

        return (row, col) switch
        {
            (1, 0) => DockTarget.SplitLeft,
            (0, 1) => DockTarget.SplitTop,
            (1, 1) => DockTarget.Center,
            (1, 2) => DockTarget.SplitRight,
            (2, 1) => DockTarget.SplitBottom,
            _ => null
        };
    }

    private static IEnumerable<DocumentGroup> GetAllDocumentGroups(DockModule module)
    {
        if (module is DocumentGroup group)
        {
            yield return group;
        }
        else if (module is DockContainer container)
        {
            foreach (DockModule child in container.Children)
            {
                foreach (DocumentGroup g in GetAllDocumentGroups(child))
                {
                    yield return g;
                }
            }
        }
    }

    private static bool ElementContainsScreenPoint(FrameworkElement element, PointInt32 screenPoint, PointInt32 clientOrigin, double scale)
    {
        try
        {
            Point elementPos = element.TransformToVisual(null).TransformPoint(new Point(0, 0));

            double elementScreenX = clientOrigin.X + elementPos.X * scale;
            double elementScreenY = clientOrigin.Y + elementPos.Y * scale;
            double elementScreenW = element.ActualWidth * scale;
            double elementScreenH = element.ActualHeight * scale;

            return screenPoint.X >= elementScreenX
                && screenPoint.X <= elementScreenX + elementScreenW
                && screenPoint.Y >= elementScreenY
                && screenPoint.Y <= elementScreenY + elementScreenH;
        }
        catch
        {
            return false;
        }
    }

    private static void Cleanup()
    {
        pollTimer?.Stop();
        pollTimer = null;

        isActive = false;
        isTabReorder = false;
        isFloatingDrag = false;
        pointerReleasedByEvent = false;
        hasPointerCapture = false;

        dragDocument = null;
        sourceGroup = null;
        sourceManager = null;
        sourceSidePopup = null;
        sourceFloatingWindow = null;
        sourceTabElement = null;

        hoveredManager = null;
        hoveredFloatingWindow = null;
        hoveredGroup = null;
        hoveredTarget = null;

        previewOffset = default;
        capturedSourceSize = default;
    }
}
