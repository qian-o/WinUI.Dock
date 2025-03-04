﻿namespace WinUI.Dock.Helpers;

internal static class DragDropHelpers
{
    private readonly static Dictionary<string, Document> cache = [];
    private readonly static Lock @lock = new();

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

    public static bool GetDocument(string text, out Document? document)
    {
        using Lock.Scope scope = @lock.EnterScope();

        return cache.TryGetValue(text, out document);
    }
}
