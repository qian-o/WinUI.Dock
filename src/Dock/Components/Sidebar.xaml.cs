using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dock.Components;

internal sealed partial class Sidebar : UserControl
{
    public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(nameof(Angle),
                                                                                          typeof(int),
                                                                                          typeof(Sidebar),
                                                                                          new PropertyMetadata(0));

    public static readonly DependencyProperty ContainersProperty = DependencyProperty.Register(nameof(Containers),
                                                                                               typeof(ObservableCollection<DocumentContainer>),
                                                                                               typeof(Sidebar),
                                                                                               new PropertyMetadata(null));

    public Sidebar()
    {
        InitializeComponent();
    }

    public int Angle
    {
        get => (int)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    public ObservableCollection<DocumentContainer> Containers
    {
        get => (ObservableCollection<DocumentContainer>)GetValue(ContainersProperty);
        set => SetValue(ContainersProperty, value);
    }
}
