using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dock;

public partial class DocumentContainer : Control
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

    public ObservableCollection<Document> Children { get; } = [];
}
