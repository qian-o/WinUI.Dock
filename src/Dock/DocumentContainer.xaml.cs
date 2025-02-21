using Dock.Abstracts;
using Microsoft.UI.Xaml;

namespace Dock;

public partial class DocumentContainer : Container<Document>
{
    public static readonly DependencyProperty CanAnchorProperty = DependencyProperty.Register(nameof(CanAnchor),
                                                                                              typeof(bool),
                                                                                              typeof(DocumentContainer),
                                                                                              new PropertyMetadata(true));

    public DocumentContainer()
    {
        DefaultStyleKey = typeof(DocumentContainer);
    }

    public bool CanAnchor
    {
        get => (bool)GetValue(CanAnchorProperty);
        set => SetValue(CanAnchorProperty, value);
    }
}
