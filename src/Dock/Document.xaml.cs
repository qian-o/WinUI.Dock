using Dock.Abstracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Dock;

[ContentProperty(Name = nameof(Content))]
public partial class Document : Component
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title),
                                                                                          typeof(string),
                                                                                          typeof(Document),
                                                                                          new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(nameof(CanClose),
                                                                                             typeof(bool),
                                                                                             typeof(Document),
                                                                                             new PropertyMetadata(true));

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
                                                                                            typeof(object),
                                                                                            typeof(Document),
                                                                                            new PropertyMetadata(null));

    public Document()
    {
        DefaultStyleKey = typeof(Document);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool CanClose
    {
        get => (bool)GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}
