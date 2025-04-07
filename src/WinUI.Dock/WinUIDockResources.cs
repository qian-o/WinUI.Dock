namespace WinUI.Dock;

public partial class WinUIDockResources : ResourceDictionary
{
    public WinUIDockResources()
    {
        Source = new Uri("ms-appx:///WinUI.Dock/Themes/Resources.xaml", UriKind.Absolute);
    }
}
