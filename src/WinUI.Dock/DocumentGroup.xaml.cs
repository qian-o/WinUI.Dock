using WinUI.Dock.Abstracts;

namespace WinUI.Dock;

public partial class DocumentGroup : DockContainer
{
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