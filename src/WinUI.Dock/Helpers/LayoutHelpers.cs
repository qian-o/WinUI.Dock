using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using WinUI.Dock.Abstracts;

namespace WinUI.Dock.Helpers;

internal static class LayoutHelpers
{
    public const string Document = "Document";
    public const string DocumentGroup = "DocumentGroup";
    public const string LayoutPanel = "LayoutPanel";

    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public static string GetFullPath(this DockModule module)
    {
        string name = module is Document document ? document.Title : module.GetType().Name;

        if (module.Owner is null)
        {
            return name;
        }
        else
        {
            string path = module.Owner.GetFullPath();

            if (module.Owner is DockContainer container)
            {
                path += $"[{container.Children.IndexOf(module)}]";
            }

            path += $".{name}";

            return path;
        }
    }

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
        return reader["Type"].Deserialize<string>(SerializerOptions) switch
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
        module.DockMinWidth = reader[nameof(DockModule.DockMinWidth)].Deserialize<double>(SerializerOptions);
        module.DockMaxWidth = reader[nameof(DockModule.DockMaxWidth)].Deserialize<double>(SerializerOptions);
        module.DockWidth = reader[nameof(DockModule.DockWidth)].Deserialize<double>(SerializerOptions);
        module.DockMinHeight = reader[nameof(DockModule.DockMinHeight)].Deserialize<double>(SerializerOptions);
        module.DockMaxHeight = reader[nameof(DockModule.DockMaxHeight)].Deserialize<double>(SerializerOptions);
        module.DockHeight = reader[nameof(DockModule.DockHeight)].Deserialize<double>(SerializerOptions);
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
        foreach (JsonObject childReader in reader[nameof(DockContainer.Children)]!.AsArray().Cast<JsonObject>())
        {
            DockModule child = CreateByModuleType(childReader);
            child.LoadLayout(childReader);

            container.Children.Add(child);
        }
    }

    public static void WriteSideDocuments(this JsonObject writer, string sideName, ObservableCollection<Document> side)
    {
        JsonArray documents = [];

        foreach (Document document in side)
        {
            JsonObject documentWriter = [];
            document.SaveLayout(documentWriter);

            documents.Add(documentWriter);
        }

        writer[sideName] = documents;
    }

    public static void ReadSideDocuments(this JsonObject reader, string sideName, ObservableCollection<Document> side)
    {
        foreach (JsonObject documentReader in reader[sideName]!.AsArray().Cast<JsonObject>())
        {
            Document document = new();
            document.LoadLayout(documentReader);

            side.Add(document);
        }
    }
}
