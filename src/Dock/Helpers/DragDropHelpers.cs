namespace Dock.Helpers;

internal static class DragDropHelpers
{
    private static readonly Dictionary<string, object> datas = [];

    public static string AddData(object data)
    {
        string key = Guid.NewGuid().ToString();

        datas.Add(key, data);

        return key;
    }

    public static object GetData(string key)
    {
        return datas[key];
    }

    public static void RemoveData(string key)
    {
        datas.Remove(key);
    }
}