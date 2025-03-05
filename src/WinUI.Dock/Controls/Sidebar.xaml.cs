using System.Collections.ObjectModel;
using WinUI.Dock.Enums;

namespace WinUI.Dock.Controls;

public sealed partial class Sidebar : UserControl
{
    public static readonly DependencyProperty DockSideProperty = DependencyProperty.Register(nameof(DockSide),
                                                                                             typeof(DockSide),
                                                                                             typeof(Sidebar),
                                                                                             new PropertyMetadata(DockSide.Left));

    public static readonly DependencyProperty DocumentsProperty = DependencyProperty.Register(nameof(Documents),
                                                                                              typeof(ObservableCollection<Document>),
                                                                                              typeof(Sidebar),
                                                                                              new PropertyMetadata(null));

    public static readonly DependencyProperty DockManagerProperty = DependencyProperty.Register(nameof(DockManager),
                                                                                                typeof(DockManager),
                                                                                                typeof(Sidebar),
                                                                                                new PropertyMetadata(null));

    public Sidebar()
    {
        InitializeComponent();
    }

    public DockSide DockSide
    {
        get => (DockSide)GetValue(DockSideProperty);
        set => SetValue(DockSideProperty, value);
    }

    public ObservableCollection<Document>? Documents
    {
        get => (ObservableCollection<Document>?)GetValue(DocumentsProperty);
        set => SetValue(DocumentsProperty, value);
    }

    public DockManager? DockManager
    {
        get => (DockManager)GetValue(DockManagerProperty);
        set => SetValue(DockManagerProperty, value);
    }

    private void OnLoaded(object _, RoutedEventArgs __)
    {
        VisualStateManager.GoToState(this, DockSide.ToString(), false);
    }

    private void Document_Click(object sender, RoutedEventArgs _)
    {
        Document document = (Document)((Button)sender).DataContext;

        new PopupDocument(DockManager!, DockSide, document).Show();
    }
}
