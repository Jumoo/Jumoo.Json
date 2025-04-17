using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jumoo.Json;
public static class JsonExpansions
{
    /// <summary>
    /// Expands all JSON in the token.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static JsonNode ExpandAllJsonInToken(this JsonNode value)
        => TryExpandJsonNodeValue(value, out var node) is true ? node : value;

    /// <summary>
    /// Tries to expand the JSON node value.
    /// </summary>
    public static bool TryExpandJsonNodeValue(this JsonNode value, [NotNullWhen(true)] out JsonNode? node)
    {
        node = default;
        try
        {
            node = value?.DeepClone() ?? null;
            if (node is null) return false;

            switch (node.GetValueKind())
            {
                case JsonValueKind.String:
                    return node.ToString().TryConvertToJsonNode(out node);
                case JsonValueKind.Object:
                    var jsonObject = node.AsObject();
                    foreach (var property in jsonObject.ToList())
                    {
                        if (property.Value?.TryExpandJsonNodeValue(out var innerNode) is true)
                        {
                            jsonObject[property.Key] = innerNode;
                        }
                    }
                    node = jsonObject;
                    return true;
                case JsonValueKind.Array:
                    var jsonArray = node.AsArray();
                    for (int n = 0; n < jsonArray.Count; n++)
                    {
                        if (jsonArray[n]?.TryExpandJsonNodeValue(out var innerNode) is true)
                        {
                            jsonArray[n] = innerNode;
                        }
                    }
                    node = jsonArray;
                    return true;
                default:
                    return true;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a string to an expanded JSON node.
    /// </summary>
    public static JsonNode? ConvertStringToExpandedJson(this string value)
    {
        if (value.TryParseToJsonNode(out var node) is false) return default;
        return node?.ExpandAllJsonInToken();
    }

    /// <summary>
    /// Converts a string to an expanded JSON string.
    /// </summary>
    public static string ConvertStringToExpandedJsonString(this string value, bool indented = true)
    {
        var json = value.ConvertStringToExpandedJson();
        if (json is null) return value;

        return json.SerializeJsonNode(indented) ?? value;
    }
}
