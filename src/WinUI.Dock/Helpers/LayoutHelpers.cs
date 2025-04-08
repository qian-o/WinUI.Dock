using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using WinUI.Dock.Abstracts;

namespace WinUI.Dock.Helpers;

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(string))]
internal partial class SourceGenerationContext : JsonSerializerContext;

internal static class LayoutHelpers
{
    public const string Document = "Document";
    public const string DocumentGroup = "DocumentGroup";
    public const string LayoutPanel = "LayoutPanel";

    public static readonly SourceGenerationContext SerializerContext = new(new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    });

    public static T Deserialize<T>(this JsonNode? node)
    {
        if (node is null)
        {
            return default!;
        }

        return (T)node.Deserialize(typeof(T), SerializerContext)!;
    }

    public static string Path(this DockModule module)
    {
        string name = module is Document document ? document.Title : module.GetType().Name;

        if (module.Owner is null)
        {
            return name;
        }
        else
        {
            string path = module.Owner.Path();

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
        return reader["Type"].Deserialize<string>() switch
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
        module.DockMinWidth = reader[nameof(DockModule.DockMinWidth)].Deserialize<double>();
        module.DockMaxWidth = reader[nameof(DockModule.DockMaxWidth)].Deserialize<double>();
        module.DockWidth = reader[nameof(DockModule.DockWidth)].Deserialize<double>();
        module.DockMinHeight = reader[nameof(DockModule.DockMinHeight)].Deserialize<double>();
        module.DockMaxHeight = reader[nameof(DockModule.DockMaxHeight)].Deserialize<double>();
        module.DockHeight = reader[nameof(DockModule.DockHeight)].Deserialize<double>();
    }

    public static void WriteDockContainerChildren(this JsonObject writer, DockContainer container)
    {
        writer[nameof(DockContainer.Children)] = new JsonArray([.. container.Children.Select(static item =>
        {
            JsonObject itemWriter = [];
            item.SaveLayout(itemWriter);

            return itemWriter;
        })]);
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
        writer[sideName] = new JsonArray([.. side.Select(static item =>
        {
            JsonObject itemWriter = [];
            item.SaveLayout(itemWriter);

            return itemWriter;
        })]);
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
