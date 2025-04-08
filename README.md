# WinUI.Dock

[![NuGet Version](https://img.shields.io/nuget/v/WinUI.Dock)](https://nuget.org/packages/WinUI.Dock)

WinUI.Dock is a docking control similar to Visual Studio, based on WinUI 3. Its design is inspired by [AvalonDock](https://github.com/Dirkster99/AvalonDock) and [ImGui](https://github.com/ocornut/imgui).

## Supported Platforms
- [x] WinUI 3.0
- [x] Uno Platform (partially available)

## Control Introduction
- **DockManager**: DockManager is the manager of the entire layout, responsible for managing all intermediate layouts, windows, and sidebars.
    - **Panel**: Intermediate layout, its type is LayoutPanel.
    - **ActiveDocument**: The currently active Document.
    - **ParentWindow**: Parent window, used to activate when dragging a Document to the current window.
    - **LeftSide, TopSide, RightSide, BottomSide**: Sidebars, their child items are of type Document.
    - **CreateNewDocument**: Event triggered when restoring the layout.
    - **CreateNewGroup**: Event triggered when dragging a Document to a specified target.
    - **CreateNewWindow**: Event triggered when dragging a Document outside the window.
    - **ActiveDocumentChanged**: Event triggered when the active Document changes.
    - **ClearLayout, SaveLayout, LoadLayout**: Clear layout, save layout, load layout.

- **Document**: Document.
    - **Title**: Document title.
    - **Content**: Document content.
    - **CanPin**: Whether it can be pinned.
    - **CanClose**: Whether it can be closed.

- **DocumentGroup**: Document group.
    - **Children**: Document group child items, their type is Document.
    - **TabPosition**: Tab position.
    - **IsTabWidthBasedOnContent**: Whether the tab width is based on content.
    - **SelectedIndex**: Selected index.

- **LayoutPanel**: Layout panel.
    - **Children**: Layout panel child items, their type is LayoutPanel or DocumentGroup.
    - **Orientation**: Layout orientation.

- **Document, DocumentGroup, LayoutPanel**: Common properties.
    - **Owner**: Owner, representing the parent.
    - **Root**: Root node, representing the top-level DockManager.
    - **DockMinWidth, DockMinHeight**: Minimum width, minimum height.
    - **DockMaxWidth, DockMaxHeight**: Maximum width, maximum height.
    - **DockWidth, DockHeight**: Width, height. (When in the intermediate layout, it will be allocated as a proportion, similar to Grid's RowDefinition and ColumnDefinition)

## Quick Start
1. Install the NuGet package
```nuget
Install-Package WinUI.Dock
```

2. Add the WinUIDockResources in App.xaml
```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
            <dock:WinUIDockResources xmlns:dock="using:WinUI.Dock" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

3. Add the DockManager control in MainWindow.xaml
```xaml
<Window xmlns:dock="using:WinUI.Dock"
        x:Name="Main">
    <dock:DockManager CreateNewDocument="OnCreateNewDocument"
                      CreateNewGroup="OnCreateNewGroup"
                      CreateNewWindow="OnCreateNewWindow"
                      ParentWindow="{Binding ElementName=Main}">
        <dock:LayoutPanel Orientation="Vertical">
            <dock:LayoutPanel DockHeight="2" Orientation="Horizontal">
                <dock:DocumentGroup DockWidth="2">
                    <dock:Document Title="Document" CanClose="False" CanPin="False" />
                </dock:DocumentGroup>

                <dock:DocumentGroup DockWidth="1">
                    <dock:Document Title="Solution Explorer" />
                </dock:DocumentGroup>
            </dock:LayoutPanel>

            <dock:LayoutPanel DockHeight="1" Orientation="Horizontal">
                <dock:DocumentGroup IsTabWidthBasedOnContent="True" TabPosition="Bottom">
                    <dock:Document Title="Side##Error List" />
                </dock:DocumentGroup>
                <dock:DocumentGroup IsTabWidthBasedOnContent="True" TabPosition="Bottom">
                    <dock:Document Title="Side##Output" />
                </dock:DocumentGroup>
            </dock:LayoutPanel>
        </dock:LayoutPanel>
    </dock:DockManager>
</Window>
```

3. Add the following code in MainWindow.xaml.cs
```csharp
// This event is triggered when restoring the layout, you can add content to the Document in this event.
private void OnCreateNewDocument(object _, CreateNewDocumentEventArgs e)
{
    e.Document.Content = new TextBlock()
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Text = $"New Document {e.Title}"
    };
}

// A special character "##" is added in the Title, and only the content after "##" will be displayed in the final interface.
// When dragging a Document to a specified target, a new DocumentGroup will be created, and this event will be triggered, allowing you to customize the properties of the DocumentGroup.
private void OnCreateNewGroup(object _, CreateNewGroupEventArgs e)
{
    if (e.Title.Contains("Side"))
    {
        e.Group.TabPosition = TabPosition.Bottom;
        e.Group.IsTabWidthBasedOnContent = true;
    }
}

// When dragging a Document out, a new window will be created, and this event will be triggered, allowing you to customize the title bar of the window.
private void OnCreateNewWindow(object _, CreateNewWindowEventArgs e)
{
    e.TitleBar.Child = new TextBlock()
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Text = "Custom Title"
    };
}
```

4. Run the program to see the effect.

## Notes
- The project is currently in an early stage and may have many issues. Please do not use it in production environments.
- The Uno Platform has only been tested on the Skia platform and does not support cross-window dragging. It works properly if used within a single window.

## Preview
| WinUI | Uno |
| ----- | --- |
| ![image](https://raw.githubusercontent.com/qian-o/WinUI.Dock/master/Screenshots/W1.png) | ![image](https://raw.githubusercontent.com/qian-o/WinUI.Dock/master/Screenshots/U1.png) |
| ![image](https://raw.githubusercontent.com/qian-o/WinUI.Dock/master/Screenshots/W2.png) | ![image](https://raw.githubusercontent.com/qian-o/WinUI.Dock/master/Screenshots/U2.png) |
