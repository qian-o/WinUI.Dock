using Microsoft.Extensions.Logging;
using Uno;
using Uno.Extensions;
using Uno.UI.Adapter.Microsoft.Extensions.Logging;
using Uno.UI.Runtime.Skia;

namespace UnoApp;

internal static class Program
{
    [STAThread]
    private static void Main(string[] _)
    {
        LogExtensionPoint.AmbientLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();

            builder.SetMinimumLevel(LogLevel.Information);
        });

        LoggingAdapter.Initialize();

        CompositionConfiguration.Configuration = CompositionConfiguration.Options.Enabled;

        SkiaHost host = SkiaHostBuilder.Create()
                                       .App(() => new App())
                                       .UseX11()
                                       .UseMacOS()
                                       .UseWindows()
                                       .UseLinuxFrameBuffer()
                                       .Build();

        host.Run();
    }
}
