using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jumoo.Json;

/// <summary>
///  Extensions for getting <see cref="JsonArray"/> values out of strings and nodes.
/// </summary>
public static class JsonArrayExtensions
{
    /// <summary>
    ///  Tries to parse a string to a JsonArray.
    /// </summary>
    public static bool TryParseToJsonArray(this string? value, [NotNullWhen(true)] out JsonArray? node)
    {
        node = default;
        if (value.TryParseToJsonNode(out var jsonNode) is false) return false;
        if (jsonNode.GetValueKind() is not JsonValueKind.Array) return false;

        node = jsonNode.AsArray();
        return node is not null;
    }

    /// <summary>
    /// Tries to parse a string to a JsonArray. will always return an array (although it might be empty)
    /// </summary>
    public static JsonArray ToJsonArray(this string? value)
        => value.TryParseToJsonArray(out var node) is true ? node ?? [] : [];

    /// <summary>
    ///  returns array as a list of JsonObjects 
    /// </summary>
    public static IList<JsonObject> AsListOfJsonObjects(this JsonArray? array)
    {
        if (array is null) return [];

        var items = new List<JsonObject>(array.Count);

        foreach (var item in array)
        {
            if (item is not null) items.Add(item.AsObject());
        }

        return items;
    }
}
