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

    // Main window HWND (discovered lazily, persists across drags)
    private static nint mainWindowHwnd;

    // Drag mode
    private static bool isActive;
    private static bool isTabReorder;
    private static bool isFloatingDrag;

    // Preview window (real FloatingWindow used as semi-transparent drag preview)
    private static FloatingWindow? previewFloatingWindow;
    private static nint previewHwnd;
    private static PointInt32 previewOffset;
    private static SizeInt32 capturedSourceSize;

    // Hit-test state
    private static DockManager? hoveredManager;
    private static nint hoveredWindowHwnd;
    private static FloatingWindow? hoveredFloatingWindow;
    private static DocumentGroup? hoveredGroup;
    private static DockTarget? hoveredTarget;

    public static bool IsDragging => isActive;

    public static void BeginTabDrag(Document document, DocumentGroup group, int index, FrameworkElement tabElement)
    {
        if (isActive)
        {
            return;
        }

        dragDocument = document;
        sourceGroup = group;
        sourceIndex = index;
        sourceManager = document.Root!;
        sourceSidePopup = null;
        sourceTabElement = tabElement;
        startScreenPoint = PointerHelpers.GetPointerPosition();

        DetectSourceWindow();
        StartTimer();
    }

    public static void BeginSidePopupDrag(Document document, DockManager manager, SidePopup popup)
    {
        if (isActive)
        {
            return;
        }

        dragDocument = document;
        sourceGroup = null;
        sourceIndex = -1;
        sourceManager = manager;
        sourceSidePopup = popup;
        startScreenPoint = PointerHelpers.GetPointerPosition();

        DetectSourceWindow();
        StartTimer();
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

    private static void DetectSourceWindow()
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

        // If not from a FloatingWindow, capture the main window HWND.
        if (sourceFloatingWindow is null)
        {
            nint hwnd = PointerHelpers.GetWindowAtScreenPoint(startScreenPoint);

            if (hwnd != nint.Zero)
            {
                mainWindowHwnd = hwnd;
            }
        }
    }

    private static void StartTimer()
    {
        isActive = true;
        isTabReorder = false;
        isFloatingDrag = false;

        pollTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        pollTimer.Interval = TimeSpan.FromMilliseconds(PollIntervalMs);
        pollTimer.Tick += OnTimerTick;
        pollTimer.Start();
    }

    private static void OnTimerTick(DispatcherQueueTimer sender, object args)
    {
        if (!PointerHelpers.IsPointerButtonPressed())
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
        nint hwnd = nint.Zero;
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

        // If not in any floating window, check main window by bounds.
        // We avoid WindowFromPoint here because the preview window (WS_EX_TRANSPARENT)
        // intercepts it, preventing the main window from being detected.
        if (manager is null && mainWindowHwnd != nint.Zero && sourceManager is not null)
        {
            if (PointerHelpers.IsPointInWindow(mainWindowHwnd, screenPoint))
            {
                manager = sourceManager;
                hwnd = mainWindowHwnd;
            }
        }

        // Manager changed?
        if (manager != hoveredManager || hwnd != hoveredWindowHwnd)
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
            hoveredWindowHwnd = hwnd;
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
                    hoveredManager.Behavior?.ActivateMainWindow();
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

        if (hwnd == nint.Zero)
        {
            return;
        }

        double managerScale = manager.XamlRoot?.RasterizationScale ?? 1.0;
        PointInt32 clientOrigin = PointerHelpers.GetClientOrigin(hwnd);

        try
        {
            Point managerPos = manager.TransformToVisual(null).TransformPoint(new Point(0, 0));

            localX = (screenPoint.X - clientOrigin.X) / managerScale - managerPos.X;
            localY = (screenPoint.Y - clientOrigin.Y) / managerScale - managerPos.Y;
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
            UpdateGroupHitTest(manager.Panel, screenPoint, hwnd, managerScale, clientOrigin);
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

    private static void UpdateGroupHitTest(LayoutPanel panel, PointInt32 screenPoint, nint hwnd, double scale, PointInt32 clientOrigin)
    {
        foreach (DocumentGroup group in GetAllDocumentGroups(panel))
        {
            if (ElementContainsScreenPoint(group, screenPoint, hwnd, scale, clientOrigin))
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
                else if (mainWindowHwnd != nint.Zero)
                {
                    PointInt32 clientOrigin = PointerHelpers.GetClientOrigin(mainWindowHwnd);

                    tabScreenX = clientOrigin.X + tabPos.X * scale;
                    tabScreenY = clientOrigin.Y + tabPos.Y * scale;
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

        // Make the preview window click-through so it doesn't interfere with hit-testing.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            previewHwnd = PointerHelpers.GetActiveWindowHandle();
            PointerHelpers.SetWindowTransparent(previewHwnd);

            // Set window-level semi-transparency (alpha ≈ 0.6).
            PointerHelpers.SetWindowAlpha(previewHwnd, 153);
        }
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
            previewHwnd = nint.Zero;
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

        // Remove window-level transparency and click-through so the window is interactive again.
        PointerHelpers.ClearWindowAlpha(previewHwnd);
        PointerHelpers.ClearWindowTransparent(previewHwnd);

        // Detach from DragService tracking — the FloatingWindow is now a normal one.
        previewFloatingWindow = null;
        previewHwnd = nint.Zero;
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

    private static bool ElementContainsScreenPoint(FrameworkElement element, PointInt32 screenPoint, nint hwnd, double scale, PointInt32 clientOrigin)
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

        dragDocument = null;
        sourceGroup = null;
        sourceManager = null;
        sourceSidePopup = null;
        sourceFloatingWindow = null;
        sourceTabElement = null;

        hoveredManager = null;
        hoveredWindowHwnd = nint.Zero;
        hoveredFloatingWindow = null;
        hoveredGroup = null;
        hoveredTarget = null;

        previewOffset = default;
        capturedSourceSize = default;
    }
}
