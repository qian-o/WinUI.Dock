using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dock.Abstractions;

public abstract class LayoutItem : ContentControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title),
                                                                                          typeof(string),
                                                                                          typeof(LayoutItem),
                                                                                          new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof(Group),
                                                                                          typeof(string),
                                                                                          typeof(LayoutItem),
                                                                                          new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Group
    {
        get => (string)GetValue(GroupProperty);
        set => SetValue(GroupProperty, value);
    }
}