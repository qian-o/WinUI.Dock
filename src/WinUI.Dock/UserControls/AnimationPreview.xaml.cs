using Microsoft.UI.Xaml.Media.Animation;

namespace WinUI.Dock.Controls;

public sealed partial class AnimationPreview : UserControl
{
    private int index;

    public AnimationPreview()
    {
        InitializeComponent();
    }

    public void Show(double width,
                     double height,
                     HorizontalAlignment horizontalAlignment,
                     VerticalAlignment verticalAlignment)
    {
        Border from = index is 0 ? P1 : P2;
        Border to = index is 0 ? P2 : P1;

#if WINDOWS
        ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("AnimationPreview", from);
        animation.Configuration = new BasicConnectedAnimationConfiguration();

        animation.TryStart(to);
#endif

        from.Visibility = Visibility.Collapsed;

        to.Width = width;
        to.Height = height;
        to.HorizontalAlignment = horizontalAlignment;
        to.VerticalAlignment = verticalAlignment;
        to.Visibility = Visibility.Visible;

        index = index is 0 ? 1 : 0;
    }
}