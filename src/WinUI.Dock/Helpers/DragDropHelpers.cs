namespace WinUI.Dock.Helpers;

internal static class DragDropHelpers
{
    private static readonly Dictionary<string, Document> cache = [];
    private static readonly Lock @lock = new();

    public const string DocumentId = "Dock.Document";

    public static string GetDocumentKey(Document document)
    {
        using Lock.Scope scope = @lock.EnterScope();

        string documentKey = Guid.NewGuid().ToString();

        cache[documentKey] = document;

        return documentKey;
    }

    public static Document? GetDocument(string documentKey)
    {
        using Lock.Scope scope = @lock.EnterScope();

        cache.TryGetValue(documentKey, out Document? document);

        return document;
    }

    public static void RemoveDocumentKey(string documentKey)
    {
        using Lock.Scope scope = @lock.EnterScope();

        cache.Remove(documentKey);
    }
}
