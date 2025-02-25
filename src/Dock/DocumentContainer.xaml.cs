using System.Collections.Specialized;
using Dock.Abstracts;
using Dock.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;

namespace Dock;

[TemplatePart(Name = "PART_Root", Type = typeof(TabView))]
[TemplatePart(Name = "PART_PinButton", Type = typeof(ToggleButton))]
public partial class DocumentContainer : Container<Document>
{
    public static readonly DependencyProperty CanAnchorProperty = DependencyProperty.Register(nameof(CanAnchor),
                                                                                              typeof(bool),
                                                                                              typeof(DocumentContainer),
                                                                                              new PropertyMetadata(true));

    private TabView? root;
    private ToggleButton? pinButton;

    public DocumentContainer()
    {
        DefaultStyleKey = typeof(DocumentContainer);
    }

    public bool CanAnchor
    {
        get => (bool)GetValue(CanAnchorProperty);
        set => SetValue(CanAnchorProperty, value);
    }

    public void Install(int selectedIndex = -1)
    {
        Uninstall();

        if (root is null || Owner is null)
        {
            return;
        }

        if (selectedIndex < 0)
        {
            if (root.SelectedIndex > 0)
            {
                selectedIndex = root.SelectedIndex;
            }
            else
            {
                selectedIndex = 0;
            }
        }

        foreach (Document item in Children)
        {
            TabViewItem tabViewItem = new()
            {
                Header = item.Title,
                Content = item,
                IsClosable = item.CanClose
            };

            tabViewItem.DragStarting += async (_, e) =>
            {
                e.Data.SetText(DragDropHelpers.AddData(item));

                RenderTargetBitmap renderTargetBitmap = new();
                await renderTargetBitmap.RenderAsync(this);
                IBuffer buffer = await renderTargetBitmap.GetPixelsAsync();

                using InMemoryRandomAccessStream stream = new();
                await stream.WriteAsync(buffer);

                BitmapImage bitmapImage = new();
                bitmapImage.SetSource(stream);

                e.DragUI.SetContentFromBitmapImage(bitmapImage);

                item.SyncSize(item.Owner!);

                item.Detach();
            };

            tabViewItem.DropCompleted += (_, e) =>
            {
                if (e.DropResult is DataPackageOperation.Link or DataPackageOperation.None)
                {
                    DocumentContainer documentContainer = new()
                    {
                        CanAnchor = false
                    };
                    documentContainer.Add(item);

                    LayoutContainer layoutContainer = new()
                    {
                        Orientation = Orientation.Horizontal
                    };
                    layoutContainer.Add(documentContainer);

                    DockingManager dockingManager = new()
                    {
                        Container = layoutContainer
                    };

                    Window window = new()
                    {
                        ExtendsContentIntoTitleBar = true,
                        Content = dockingManager
                    };

                    window.Activate();
                }
            };

            tabViewItem.CloseRequested += (_, _) => item.Detach();

            root.TabItems.Add(tabViewItem);
        }

        if (selectedIndex < root.TabItems.Count)
        {
            root.SelectedIndex = selectedIndex;
        }
        else
        {
            root.SelectedIndex = root.TabItems.Count - 1;
        }

        if (pinButton is not null)
        {
            pinButton.IsChecked = true;
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        root = GetTemplateChild("PART_Root") as TabView;
        pinButton = GetTemplateChild("PART_PinButton") as ToggleButton;

        if (pinButton is not null)
        {
            pinButton.Click += PinButton_Click;
        }

        Install();
    }

    protected override void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(sender, e);

        Install();
    }

    private void Uninstall()
    {
        if (root is null)
        {
            return;
        }

        foreach (TabViewItem item in root.TabItems.Cast<TabViewItem>())
        {
            item.Content = null;
        }

        root.TabItems.Clear();
    }

    private void PinButton_Click(object sender, RoutedEventArgs e)
    {
        Uninstall();

        LayoutContainer container = (LayoutContainer)Owner!;
        DockingManager manager = Manager!;

        int index = container.IndexOf(this);

        if (index >= 0)
        {
            container.RemoveAt(index);
        }

        if (index is 0)
        {
            if (container.Orientation is Orientation.Horizontal)
            {
                manager.Left.Add(this);
            }
            else
            {
                manager.Top.Add(this);
            }
        }
        else if (container.Orientation is Orientation.Horizontal)
        {
            manager.Right.Add(this);
        }
        else
        {
            manager.Bottom.Add(this);
        }
    }
}
