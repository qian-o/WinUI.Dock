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

    private async void OnDrop(object sender, DragEventArgs e)
    {
        Opacity = 0.4;

        Document document = (Document)DragDropHelpers.GetData(await e.DataView.GetTextAsync());

        DocumentContainer selfContainer = (DocumentContainer)Document.Owner!;
        IContainer container = (IContainer)selfContainer.Owner!;
        DockingManager manager = Document.Manager!;

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

                    DocumentContainer left = new()
                    {
                        CanAnchor = false
                    };
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
                {
                    container.AutoRemove = false;

                    int index = container.IndexOf(selfContainer);

                    selfContainer.Detach();

                    DocumentContainer top = new()
                    {
                        CanAnchor = false
                    };
                    top.Add(document);

                    top.SyncSize(document);

                    DocumentContainer bottom = selfContainer;

                    LayoutContainer layoutContainer = new()
                    {
                        Orientation = Orientation.Vertical
                    };
                    layoutContainer.Add(top);
                    layoutContainer.Add(bottom);

                    layoutContainer.SyncSize(selfContainer);

                    container.Add(layoutContainer, index);

                    container.AutoRemove = true;
                }
                break;
            case Target.SplitRight:
                {
                    container.AutoRemove = false;

                    int index = container.IndexOf(selfContainer);

                    selfContainer.Detach();

                    DocumentContainer left = selfContainer;

                    DocumentContainer right = new()
                    {
                        CanAnchor = false
                    };
                    right.Add(document);

                    right.SyncSize(document);

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
            case Target.SplitBottom:
                {
                    container.AutoRemove = false;

                    int index = container.IndexOf(selfContainer);

                    selfContainer.Detach();

                    DocumentContainer top = selfContainer;

                    DocumentContainer bottom = new()
                    {
                        CanAnchor = false
                    };
                    bottom.Add(document);

                    bottom.SyncSize(document);

                    LayoutContainer layoutContainer = new()
                    {
                        Orientation = Orientation.Vertical
                    };
                    layoutContainer.Add(top);
                    layoutContainer.Add(bottom);

                    layoutContainer.SyncSize(selfContainer);

                    container.Add(layoutContainer, index);

                    container.AutoRemove = true;
                }
                break;
            case Target.DockLeft:
                {
                    DocumentContainer documentContainer = new()
                    {
                        CanAnchor = true
                    };
                    documentContainer.Add(document);

                    LayoutContainer layoutContainer = new()
                    {
                        Orientation = Orientation.Horizontal
                    };

                    layoutContainer.Add(documentContainer);
                    layoutContainer.Add(manager.Container!);

                    manager.Container = layoutContainer;
                }
                break;
            case Target.DockTop:
                {
                    DocumentContainer documentContainer = new()
                    {
                        CanAnchor = true
                    };
                    documentContainer.Add(document);

                    LayoutContainer layoutContainer = new()
                    {
                        Orientation = Orientation.Vertical
                    };

                    layoutContainer.Add(documentContainer);
                    layoutContainer.Add(manager.Container!);

                    manager.Container = layoutContainer;
                }
                break;
            case Target.DockRight:
                {
                    DocumentContainer documentContainer = new()
                    {
                        CanAnchor = true
                    };
                    documentContainer.Add(document);

                    LayoutContainer layoutContainer = new()
                    {
                        Orientation = Orientation.Horizontal
                    };

                    layoutContainer.Add(manager.Container!);
                    layoutContainer.Add(documentContainer);

                    manager.Container = layoutContainer;
                }
                break;
            case Target.DockBottom:
                {
                    DocumentContainer documentContainer = new()
                    {
                        CanAnchor = true
                    };
                    documentContainer.Add(document);

                    LayoutContainer layoutContainer = new()
                    {
                        Orientation = Orientation.Vertical
                    };

                    layoutContainer.Add(manager.Container!);
                    layoutContainer.Add(documentContainer);

                    manager.Container = layoutContainer;
                }
                break;
        }

        Document.IsDrop();
    }
}
