namespace UnoApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow mainWindow = new();

#if DEBUG
        mainWindow.UseStudio();
#endif

        mainWindow.Activate();
    }
}