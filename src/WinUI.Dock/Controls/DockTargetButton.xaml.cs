using Windows.ApplicationModel.DataTransfer;
using WinUI.Dock.Enums;
using WinUI.Dock.Helpers;

namespace WinUI.Dock.Controls;

public sealed partial class DockTargetButton : UserControl
{
    public static readonly DependencyProperty DockTargetProperty = DependencyProperty.Register(nameof(DockTarget),
                                                                                               typeof(DockTarget),
                                                                                               typeof(DockTargetButton),
                                                                                               new PropertyMetadata(DockTarget.Center));

    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target),
                                                                                           typeof(Control),
                                                                                           typeof(DockTargetButton),
                                                                                           new PropertyMetadata(null));

    public DockTargetButton()
    {
        InitializeComponent();
    }

    public DockTarget DockTarget
    {
        get => (DockTarget)GetValue(DockTargetProperty);
        set => SetValue(DockTargetProperty, value);
    }

    public Control? Target
    {
        get => (Control)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        base.OnDragOver(e);

        e.AcceptedOperation = DataPackageOperation.Move;
    }

    protected override async void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);

        if (DragDropHelpers.GetDocument((string)await e.DataView.GetDataAsync(DragDropHelpers.Format)) is Document document)
        {
            DocumentGroup group = new();
            group.CopySizeFrom(document);
            group.Children.Add(document);

            switch (DockTarget)
            {
                case DockTarget.Center:
                    break;
                case DockTarget.SplitLeft:
                    break;
                case DockTarget.SplitTop:
                    break;
                case DockTarget.SplitRight:
                    break;
                case DockTarget.SplitBottom:
                    break;
                case DockTarget.DockLeft:
                    {
                        DockManager dockManager = (DockManager)Target!;

                        LayoutPanel panel = new() { Orientation = Orientation.Horizontal };
                        panel.Children.Add(group);

                        if (dockManager.Panel is not null)
                        {
                            panel.Children.Add(dockManager.Panel);
                        }

                        dockManager.Panel = panel;
                    }
                    break;
                case DockTarget.DockTop:
                    {
                        DockManager dockManager = (DockManager)Target!;

                        LayoutPanel panel = new() { Orientation = Orientation.Vertical };
                        panel.Children.Add(group);

                        if (dockManager.Panel is not null)
                        {
                            panel.Children.Add(dockManager.Panel);
                        }

                        dockManager.Panel = panel;
                    }
                    break;
                case DockTarget.DockRight:
                    {
                        DockManager dockManager = (DockManager)Target!;

                        LayoutPanel panel = new() { Orientation = Orientation.Horizontal };
                        panel.Children.Add(group);

                        if (dockManager.Panel is not null)
                        {
                            panel.Children.Insert(0, dockManager.Panel);
                        }

                        dockManager.Panel = panel;
                    }
                    break;
                case DockTarget.DockBottom:
                    {
                        DockManager dockManager = (DockManager)Target!;

                        LayoutPanel panel = new() { Orientation = Orientation.Vertical };
                        panel.Children.Add(group);

                        if (dockManager.Panel is not null)
                        {
                            panel.Children.Insert(0, dockManager.Panel);
                        }

                        dockManager.Panel = panel;
                    }
                    break;
            }
        }
    }

    private void OnLoaded(object _, RoutedEventArgs __)
    {
        VisualStateManager.GoToState(this, DockTarget.ToString(), false);
    }
}
