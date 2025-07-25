namespace WinUI.Dock;

public static class DocumentHelpers
{
    public static Document? GetDocument(object content)
    {
        if (content is FrameworkElement element)
        {
            if (element.Parent is Document document)
            {
                return document;
            }
            else
            {
                return GetDocument(element.Parent);
            }
        }

        return null;
    }
}
