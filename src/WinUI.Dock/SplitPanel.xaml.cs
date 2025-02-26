using WinUI.Dock.Abstracts;

namespace WinUI.Dock;

public partial class SplitPanel : DockContainer
{
    protected override void LoadChildren()
    {
    }

    protected override void UnloadChildren()
    {
    }

    protected override bool ValidateChildren()
    {
        return Children.All(static item => item is DockContainer);
    }
}
