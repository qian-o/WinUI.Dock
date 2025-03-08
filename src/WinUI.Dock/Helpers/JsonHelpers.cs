using System.Text.Json.Nodes;

namespace WinUI.Dock.Helpers;

public static class JsonHelpers
{
    public static T? Get<T>(this JsonObject json, string propertyName)
    {
        return json.TryGetPropertyValue(propertyName, out JsonNode? value) ? value!.GetValue<T>() : default;
    }
}
