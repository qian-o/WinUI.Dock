using Uno.UI.Runtime.Skia;

namespace UnoApp;

internal static class Program
{
    [STAThread]
    private static void Main(string[] _)
    {
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
