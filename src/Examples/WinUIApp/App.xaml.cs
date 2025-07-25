using Microsoft.UI.Xaml;

namespace WinUIApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    public static MainWindow MainWindow { get; } = new();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow.Activate();
    }
}
