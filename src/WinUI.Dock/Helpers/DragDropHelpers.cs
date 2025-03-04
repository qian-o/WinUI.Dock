namespace WinUI.Dock.Helpers;

internal static class DragDropHelpers
{
    private static readonly Dictionary<string, Document> cache = [];
    private static readonly Lock @lock = new();

    public static string GetText(Document document)
    {
        using Lock.Scope scope = @lock.EnterScope();

        string? text = cache.FirstOrDefault(x => x.Value == document).Key;

        if (text is null)
        {
            text = Guid.NewGuid().ToString();

            cache.Add(text, document);
        }

        return text;
    }

    public static Document? GetDocument(string text)
    {
        using Lock.Scope scope = @lock.EnterScope();

        cache.TryGetValue(text, out Document? document);

        return document;
    }
}
