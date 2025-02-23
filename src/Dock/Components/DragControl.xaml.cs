using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Dock.Components;

[TemplatePart(Name = "PART_Thumb", Type = typeof(Thumb))]
public sealed partial class DragControl : ContentControl
{
    private Thumb thumb = null!;

    public DragControl()
    {
        DefaultStyleKey = typeof(DragControl);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        thumb = GetTemplateChild("PART_Thumb") as Thumb ?? throw new ArgumentNullException("PART_Thumb");

        thumb.DragDelta += OnDragDelta;
        thumb.DragCompleted += OnDragCompleted;
    }

    private void OnDragDelta(object sender, DragDeltaEventArgs e)
    {
    }

    private void OnDragCompleted(object sender, DragCompletedEventArgs e)
    {
    }
}
