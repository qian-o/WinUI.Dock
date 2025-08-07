using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Text;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WinUI.Dock;

namespace Example.ViewModels;

public partial class MainViewModel : ObservableObject, IDockAdapter, IDockBehavior
{
    [RelayCommand]
    private static async Task Open(DockManager manager)
    {
        FileOpenPicker openPicker = new()
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            FileTypeFilter = { ".json" }
        };

        InitializeWithWindow.Initialize(openPicker, WindowNative.GetWindowHandle(App.MainWindow));

        if (await openPicker.PickSingleFileAsync() is StorageFile file)
        {
            manager.LoadLayout(File.ReadAllText(file.Path));
        }
    }

    [RelayCommand]
    private static async Task Save(DockManager manager)
    {
        FileSavePicker savePicker = new()
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            FileTypeChoices = { { "JSON", new List<string> { ".json" } } }
        };

        InitializeWithWindow.Initialize(savePicker, WindowNative.GetWindowHandle(App.MainWindow));

        if (await savePicker.PickSaveFileAsync() is StorageFile file)
        {
            File.WriteAllText(file.Path, manager.SaveLayout());
        }
    }

    [RelayCommand]
    private static void Exit()
    {
        App.MainWindow.Close();
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
