namespace WinUI.Dock;

public interface IDockBehavior
{
    Window? MainWindow { get; }

    void OnDocked(Document src, DockManager dest, DockTarget target);

    void OnDocked(Document src, DocumentGroup dest, DockTarget target);

    void OnFloating(Document document);
}
