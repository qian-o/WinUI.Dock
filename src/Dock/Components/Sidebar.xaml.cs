using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Dock.Components;

internal sealed partial class Sidebar : UserControl
{
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation),
                                                                                                typeof(Orientation),
                                                                                                typeof(Sidebar),
                                                                                                new PropertyMetadata(Orientation.Horizontal));

    public static readonly DependencyProperty ContainersProperty = DependencyProperty.Register(nameof(Containers),
                                                                                               typeof(ObservableCollection<DocumentContainer>),
                                                                                               typeof(Sidebar),
                                                                                               new PropertyMetadata(null));

    public Sidebar()
    {
        InitializeComponent();
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public ObservableCollection<DocumentContainer> Containers
    {
        get => (ObservableCollection<DocumentContainer>)GetValue(ContainersProperty);
        set => SetValue(ContainersProperty, value);
    }

    private void Document_Opened(object sender, object _)
    {
        Popup popup = (Popup)sender;
        Document document = (Document)popup.DataContext;
        DocumentContainer container = (DocumentContainer)document.Owner!;
        DockingManager manager = document.Manager!;

        if (manager.Left.Contains(container))
        {
        }
        else if (manager.Top.Contains(container))
        {
        }
        else if (manager.Right.Contains(container))
        {
        }
        else if (manager.Bottom.Contains(container))
        {
        }
    }
}
