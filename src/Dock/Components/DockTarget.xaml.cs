using Dock.Enums;
using Dock.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace Dock.Components;

public sealed partial class DockTarget : UserControl
{
    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target),
                                                                                           typeof(Target),
                                                                                           typeof(DockTarget),
                                                                                           new PropertyMetadata(Target.Center));

    public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(nameof(Document),
                                                                                             typeof(Document),
                                                                                             typeof(DockTarget),
                                                                                             new PropertyMetadata(null));

    public DockTarget()
    {
        InitializeComponent();

        DragOver += OnDragOver;
        DragLeave += OnDragLeave;
        Drop += OnDrop;
    }

    public Target Target
    {
        get => (Target)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public Document Document
    {
        get => (Document)GetValue(DocumentProperty); set => SetValue(DocumentProperty, value);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        Opacity = 1;

        e.AcceptedOperation = DataPackageOperation.Move;
        e.Handled = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        Opacity = 0.4;

        e.AcceptedOperation = DataPackageOperation.None;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        Opacity = 0.4;

        Document document = (Document)DragDropHelpers.GetData(e.DataView.GetTextAsync().GetResults());

        DocumentContainer selfContainer = (DocumentContainer)Document.Owner!;
        IContainer container = (IContainer)selfContainer.Owner!;

        switch (Target)
        {
            case Target.Center:
                {
                    selfContainer.Add(document);

                    selfContainer.Install(selfContainer.Count - 1);
                }
                break;
            case Target.SplitLeft:
                {
                    container.AutoRemove = false;

                    int index = container.IndexOf(selfContainer);

                    selfContainer.Detach();

                    DocumentContainer left = new();
                    left.Add(document);

                    left.SyncSize(document);

                    DocumentContainer right = selfContainer;

                    LayoutContainer layoutContainer = new()
                    {
                        Orientation = Orientation.Horizontal
                    };
                    layoutContainer.Add(left);
                    layoutContainer.Add(right);

                    layoutContainer.SyncSize(selfContainer);

                    container.Add(layoutContainer, index);

                    container.AutoRemove = true;
                }
                break;
            case Target.SplitTop:
                break;
            case Target.SplitRight:
                break;
            case Target.SplitBottom:
                break;
            case Target.DockLeft:
                break;
            case Target.DockTop:
                break;
            case Target.DockRight:
                break;
            case Target.DockBottom:
                break;
        }

        Document.IsDrop();
    }
}
