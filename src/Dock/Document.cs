using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dock;

public partial class Document : ContentControl
{
    public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(nameof(CanClose),
                                                                                             typeof(bool),
                                                                                             typeof(Document),
                                                                                             new PropertyMetadata(true));

    public Document()
    {
        DefaultStyleKey = typeof(Document);
    }

    public bool CanClose
    {
        get => (bool)GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }
}
