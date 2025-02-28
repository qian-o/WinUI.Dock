using WinUI.Dock.Abstracts;
using WinUI.Dock.Enums;

namespace WinUI.Dock;

public partial class DocumentGroup : DockContainer
{
    public static readonly DependencyProperty TabPositionProperty = DependencyProperty.Register(nameof(TabPosition),
                                                                                                typeof(TabPosition),
                                                                                                typeof(DocumentGroup),
                                                                                                new PropertyMetadata(TabPosition.Top));

    public static readonly DependencyProperty IsTabWidthBasedOnContentProperty = DependencyProperty.Register(nameof(IsTabWidthBasedOnContent),
                                                                                                             typeof(bool),
                                                                                                             typeof(DocumentGroup),
                                                                                                             new PropertyMetadata(false));

    public DocumentGroup()
    {
        DefaultStyleKey = typeof(DocumentGroup);
    }

    public TabPosition TabPosition
    {
        get => (TabPosition)GetValue(TabPositionProperty);
        set => SetValue(TabPositionProperty, value);
    }

    public bool IsTabWidthBasedOnContent
    {
        get => (bool)GetValue(IsTabWidthBasedOnContentProperty);
        set => SetValue(IsTabWidthBasedOnContentProperty, value);
    }

    protected override void LoadPart()
    {
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