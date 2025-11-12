using Example.Uno.ViewModels;

namespace Example.Uno.Views;

public sealed partial class MainView : Page
{
    public MainView()
    {
        InitializeComponent();

        ViewModel = new();
    }

    public MainViewModel ViewModel { get; }
}
