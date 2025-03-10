using System.Text.Json.Nodes;
using WinUI.Dock.Abstracts;

namespace WinUI.Dock.Helpers;

public static class LayoutHelpers
{
    public const string Document = "Document";
    public const string DocumentGroup = "DocumentGroup";
    public const string LayoutPanel = "LayoutPanel";

    public static void WriteByModuleType(this JsonObject writer, DockModule module)
    {
        writer["Type"] = module switch
        {
            Document _ => Document,
            DocumentGroup _ => DocumentGroup,
            LayoutPanel _ => LayoutPanel,
            _ => throw new NotSupportedException()
        };
    }

    public static DockModule CreateByModuleType(this JsonObject reader)
    {
        return reader["Type"]!.GetValue<string>() switch
        {
            Document => new Document(),
            DocumentGroup => new DocumentGroup(),
            LayoutPanel => new LayoutPanel(),
            _ => throw new NotSupportedException()
        };
    }

    public static void WriteDockModuleProperties(this JsonObject writer, DockModule module)
    {
        writer[nameof(DockModule.DockMinWidth)] = module.DockMinWidth;
        writer[nameof(DockModule.DockMaxWidth)] = module.DockMaxWidth;
        writer[nameof(DockModule.DockWidth)] = module.DockWidth;
        writer[nameof(DockModule.DockMinHeight)] = module.DockMinHeight;
        writer[nameof(DockModule.DockMaxHeight)] = module.DockMaxHeight;
        writer[nameof(DockModule.DockHeight)] = module.DockHeight;
    }

    public static void ReadDockModuleProperties(this JsonObject reader, DockModule module)
    {
        module.DockMinWidth = reader[nameof(DockModule.DockMinWidth)]!.GetValue<double>();
        module.DockMaxWidth = reader[nameof(DockModule.DockMaxWidth)]!.GetValue<double>();
        module.DockWidth = reader[nameof(DockModule.DockWidth)]!.GetValue<double>();
        module.DockMinHeight = reader[nameof(DockModule.DockMinHeight)]!.GetValue<double>();
        module.DockMaxHeight = reader[nameof(DockModule.DockMaxHeight)]!.GetValue<double>();
        module.DockHeight = reader[nameof(DockModule.DockHeight)]!.GetValue<double>();
    }

    public static void WriteDockContainerChildren(this JsonObject writer, DockContainer container)
    {
        JsonArray children = [];

        foreach (DockModule child in container.Children)
        {
            JsonObject childWriter = [];

            child.SaveLayout(childWriter);

            children.Add(childWriter);
        }

        writer[nameof(DockContainer.Children)] = children;
    }

    public static void ReadDockContainerChildren(this JsonObject reader, DockContainer container)
    {
        container.Children.Clear();

        foreach (JsonObject childReader in reader[nameof(DockContainer.Children)]!.AsArray().Cast<JsonObject>())
        {
            DockModule child = CreateByModuleType(childReader);

            child.LoadLayout(childReader);

            container.Children.Add(child);
        }
    }
}
