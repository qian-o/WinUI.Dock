namespace WinUI.Dock.Helpers;

internal static class DragDropHelpers
{
    private static readonly Dictionary<string, Document> cache = [];
    private static readonly Lock @lock = new();

    public const string FormatId = "Dock.Document";

    public static string GetDragKey(Document document)
    {
        using Lock.Scope scope = @lock.EnterScope();

        string dragKey = Guid.NewGuid().ToString();

        cache[dragKey] = document;

        return dragKey;
    }

    public static Document? GetDocument(string dragKey)
    {
        using Lock.Scope scope = @lock.EnterScope();

        cache.TryGetValue(dragKey, out Document? document);

        return document;
    }

    public static void RemoveDragKey(string dragKey)
    {
        using Lock.Scope scope = @lock.EnterScope();

        cache.Remove(dragKey);
    }
}
