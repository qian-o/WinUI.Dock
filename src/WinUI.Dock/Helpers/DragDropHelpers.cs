namespace WinUI.Dock.Helpers;

internal static class DragDropHelpers
{
    private static readonly Dictionary<string, DockManager> dockManagers = [];
    private static readonly Dictionary<string, Document> documents = [];
    private static readonly Lock @lock = new();

    public const string DockManagerId = "Dock.DockManager";
    public const string DocumentId = "Dock.Document";

    public static string GetDockManagerKey(DockManager dockManager)
    {
        using Lock.Scope scope = @lock.EnterScope();

        string dockManagerKey = Guid.NewGuid().ToString();

        dockManagers[dockManagerKey] = dockManager;

        return dockManagerKey;
    }

    public static DockManager? GetDockManager(string dockManagerKey)
    {
        using Lock.Scope scope = @lock.EnterScope();

        dockManagers.TryGetValue(dockManagerKey, out DockManager? dockManager);

        return dockManager;
    }

    public static void RemoveDockManagerKey(string dockManagerKey)
    {
        using Lock.Scope scope = @lock.EnterScope();

        dockManagers.Remove(dockManagerKey);
    }

    public static string GetDocumentKey(Document document)
    {
        using Lock.Scope scope = @lock.EnterScope();

        string documentKey = Guid.NewGuid().ToString();

        documents[documentKey] = document;

        return documentKey;
    }

    public static Document? GetDocument(string documentKey)
    {
        using Lock.Scope scope = @lock.EnterScope();

        documents.TryGetValue(documentKey, out Document? document);

        return document;
    }

    public static void RemoveDocumentKey(string documentKey)
    {
        using Lock.Scope scope = @lock.EnterScope();

        documents.Remove(documentKey);
    }
}
