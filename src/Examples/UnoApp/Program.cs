using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Uno.UI.Adapter.Microsoft.Extensions.Logging;
using Uno.UI.Runtime.Skia;

namespace UnoApp;

internal static class Program
{
    [STAThread]
    private static void Main(string[] _)
    {
#if DEBUG
        LogExtensionPoint.AmbientLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();

            builder.SetMinimumLevel(LogLevel.Information);
        });

        LoggingAdapter.Initialize();
#endif

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
