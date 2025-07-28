using Example.Views;

namespace Example;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    public static Window MainWindow { get; } = new() { Content = new MainView() };

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow.Activate();
    }
}