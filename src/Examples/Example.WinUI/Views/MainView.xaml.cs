using Example.WinUI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Example.WinUI.Views;

public sealed partial class MainView : Page
{
    public MainView()
    {
        InitializeComponent();

        ViewModel = new();
    }

    public MainViewModel ViewModel { get; }
}
