using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;

namespace Dock;

public partial class DocumentContainer : Control
{
    public DocumentContainer()
    {
        DefaultStyleKey = typeof(DocumentContainer);
    }

    public ObservableCollection<Document> Children { get; } = [];
}
