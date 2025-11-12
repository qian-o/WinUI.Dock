using Example.Uno.Views;

namespace Example.Uno;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    public static Window MainWindow { get; } = new();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow.Content = new MainView();
        MainWindow.Activate();
    }
}
