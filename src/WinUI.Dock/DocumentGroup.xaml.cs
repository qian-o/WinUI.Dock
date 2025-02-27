using WinUI.Dock.Abstracts;

namespace WinUI.Dock;

public partial class DocumentGroup : DockContainer
{
    public DocumentGroup()
    {
        DefaultStyleKey = typeof(DocumentGroup);
    }

    protected override void LoadChildren()
    {
    }

    protected override void UnloadChildren()
    {
    }

    protected override bool ValidateChildren()
    {
        return Children.All(static item => item is Document);
    }
}