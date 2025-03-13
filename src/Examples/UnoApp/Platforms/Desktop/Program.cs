using Uno;
using Uno.UI.Runtime.Skia;

namespace UnoApp.Platforms.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main(string[] _)
    {
        App.InitializeLogging();

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
