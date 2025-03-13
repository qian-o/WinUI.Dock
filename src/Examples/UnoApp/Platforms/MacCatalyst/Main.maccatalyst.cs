using UIKit;

namespace UnoApp.Platforms.MacCatalyst;

internal static class EntryPoint
{
    public static void Main(string[] _)
    {
        App.InitializeLogging();

        UIApplication.Main([], null, typeof(App));
    }
}
