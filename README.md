# WinUI.Dock

[![NuGet Version](https://img.shields.io/nuget/v/WinUI.Dock)](https://nuget.org/packages/WinUI.Dock)

WinUI.Dock is a docking control similar to Visual Studio, based on WinUI 3. Its design is inspired by [AvalonDock](https://github.com/Dirkster99/AvalonDock) and [ImGui](https://github.com/ocornut/imgui).

## Supported Platforms
- [x] WinUI 3.0
- [x] Uno Platform (partially available)

## Preview
| WinUI | Uno |
| ----- | --- |
| ![image](https://raw.githubusercontent.com/qian-o/WinUI.Dock/master/Screenshots/W1.png) | ![image](https://raw.githubusercontent.com/qian-o/WinUI.Dock/master/Screenshots/U1.png) |
| ![image](https://raw.githubusercontent.com/qian-o/WinUI.Dock/master/Screenshots/W2.png) | ![image](https://raw.githubusercontent.com/qian-o/WinUI.Dock/master/Screenshots/U2.png) |

## Quick Start
1. Install the NuGet package
```nuget
Install-Package WinUI.Dock
```

2. Add the DockManager control in MainView.xaml
```xaml
xmlns:dock="using:WinUI.Dock"

<dock:DockManager x:Name="Manager"
                  Adapter="{Binding}"
                  Behavior="{Binding}">
    <dock:LayoutPanel Orientation="Vertical">
        <dock:LayoutPanel Orientation="Horizontal">
            <dock:DocumentGroup ShowWhenEmpty="True">
                <dock:Document Title="MainView.xaml"
                               CanClose="True">
                    <Button VerticalAlignment="Top"
                            Command="{Binding OpenPropertiesCommand}"
                            CommandParameter="{Binding RelativeSource={RelativeSource Mode=Self}}"
                            Content="Open Properties" />
                </dock:Document>
                <dock:Document Title="MainViewModel.cs"
                               CanClose="True" />
            </dock:DocumentGroup>

            <dock:DocumentGroup Width="200"
                                CompactTabs="True"
                                TabPosition="Bottom">
                <dock:Document Title="Bottom##Solution Explorer"
                               CanClose="True"
                               CanPin="True" />
                <dock:Document Title="Bottom##Git Changes"
                               CanClose="True"
                               CanPin="True" />
            </dock:DocumentGroup>
        </dock:LayoutPanel>

        <dock:LayoutPanel Height="200"
                          Orientation="Horizontal">
            <dock:DocumentGroup TabPosition="Bottom">
                <dock:Document Title="Bottom##Error List"
                               CanClose="True"
                               CanPin="True" />
            </dock:DocumentGroup>

            <dock:DocumentGroup CompactTabs="True"
                                TabPosition="Bottom">
                <dock:Document Title="Bottom##Output"
                               CanClose="True"
                               CanPin="True" />
                <dock:Document Title="Bottom##Terminal"
                               CanClose="True"
                               CanPin="True" />
            </dock:DocumentGroup>
        </dock:LayoutPanel>
    </dock:LayoutPanel>
</dock:DockManager>
```

3. Implement the Adapter and Behavior interfaces in MainViewModel.cs
```csharp
public partial class MainViewModel : ObservableObject, IDockAdapter, IDockBehavior
{
    private readonly Document propertiesDocument = new()
    {
        Title = "Bottom##Properties",
        Content = "This is a properties document.",
        CanClose = true
    };

    [RelayCommand]
    private void OpenProperties(object content)
    {
        propertiesDocument.Width = 200;
        propertiesDocument.DockTo(DocumentHelpers.GetDocument(content)!, DockTarget.SplitRight);
    }

    void IDockAdapter.OnCreated(Document document)
    {
        document.Content = new TextBlock()
        {
            Text = document.ActualTitle,
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    void IDockAdapter.OnCreated(DocumentGroup group, Document? draggedDocument)
    {
        if (draggedDocument?.Title.Contains("Bottom##") is true)
        {
            group.TabPosition = TabPosition.Bottom;
        }
    }

    object? IDockAdapter.GetFloatingWindowTitleBar(Document? draggedDocument)
    {
        return new TextBlock()
        {
            IsHitTestVisible = false,
            Text = "Example Floating Window",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 16,
            FontWeight = FontWeights.Bold
        };
    }

    void IDockBehavior.ActivateMainWindow()
    {
        App.MainWindow.Activate();
    }

    void IDockBehavior.OnDocked(Document src, DockManager dest, DockTarget target)
    {
        Debug.WriteLine($"Document '{src.ActualTitle}' docked to DockManager at target '{target}'.");
    }

    void IDockBehavior.OnDocked(Document src, DocumentGroup dest, DockTarget target)
    {
        Debug.WriteLine($"Document '{src.ActualTitle}' docked to DocumentGroup at target '{target}'.");
    }

    void IDockBehavior.OnFloating(Document document)
    {
        Debug.WriteLine($"Document '{document.ActualTitle}' is now floating.");
    }
}
```

4. Run the program to see the effect.

## Known Issues
- Limited MVVM support - I don't have a good solution for data source binding (document and layout creation).
- Uno Platform's support for custom windows and cross-window drag-and-drop remains limited.
