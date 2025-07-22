using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Uno.UI.Adapter.Microsoft.Extensions.Logging;

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

        mainWindow.Activate();
    }

    public static void InitializeLogging()
    {
        LogExtensionPoint.AmbientLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();

            builder.SetMinimumLevel(LogLevel.Information);
        });

        LoggingAdapter.Initialize();
    }
}