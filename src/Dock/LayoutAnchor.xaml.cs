using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dock;

public sealed partial class LayoutAnchor : ContentControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title),
                                                                                          typeof(string),
                                                                                          typeof(LayoutAnchor),
                                                                                          new PropertyMetadata(string.Empty));

    public LayoutAnchor()
    {
        DefaultStyleKey = typeof(LayoutAnchor);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}
