using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Media;

namespace WinUI.Dock;

/// <summary>
/// A SystemBackdrop that makes the window fully transparent (no background, no blur).
/// Equivalent to WPF's AllowsTransparency + Transparent background.
/// </summary>
internal sealed class TransparentBackdrop : SystemBackdrop
{
    protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot)
    {
        base.OnTargetConnected(connectedTarget, xamlRoot);

        // connectedTarget is a Windows.UI.Composition.DesktopWindowTarget (CompositionObject).
        // Use its Compositor to create a transparent brush in the correct composition namespace.
        if (connectedTarget is Windows.UI.Composition.CompositionObject compositionObject)
        {
            connectedTarget.SystemBackdrop = compositionObject.Compositor.CreateColorBrush(
                Windows.UI.Color.FromArgb(0, 0, 0, 0));
        }
    }

    protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
    {
        base.OnTargetDisconnected(disconnectedTarget);

        disconnectedTarget.SystemBackdrop = null;
    }
}
