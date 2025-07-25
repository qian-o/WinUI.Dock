namespace WinUI.Dock;

public interface IDockBehavior
{
    void ActivateMainWindow();

    void OnDocked(Document src, object dest, DockTarget target);

    void OnFloating(Document document);
}
