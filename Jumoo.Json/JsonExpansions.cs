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
    /// <remarks>
    ///  When there is nothing to expand the original node is handed back rather than a clone,
    ///  so don't mutate the result unless you know something was expanded.
    /// </remarks>
    public static bool TryExpandJsonNodeValue(this JsonNode value, [NotNullWhen(true)] out JsonNode? node)
    {
        node = default;
        if (value is null) return false;

        try
        {
            // expansion only ever replaces string values holding json. If the graph has none,
            // there is nothing to do - and finding that out is far cheaper than the clone.
            if (HasExpandableValue(value) is false)
            {
                node = value;
                return true;
            }

            node = ExpandNode(value.DeepClone());
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///  Read-only walk looking for any string value that holds json.
    /// </summary>
    private static bool HasExpandableValue(JsonNode node)
    {
        switch (node.GetValueKind())
        {
            case JsonValueKind.String:
                return node.ToString().LooksLikeJson();
            case JsonValueKind.Object:
                foreach (var property in node.AsObject())
                {
                    if (property.Value is not null && HasExpandableValue(property.Value))
                        return true;
                }
                return false;
            case JsonValueKind.Array:
                foreach (var child in node.AsArray())
                {
                    if (child is not null && HasExpandableValue(child))
                        return true;
                }
                return false;
            default:
                return false;
        }
    }

    private static JsonNode ExpandNode(JsonNode node)
    {
        switch (node.GetValueKind())
        {
            case JsonValueKind.String:
                // only swap the node out if the string actually parses as json. Anything else
                // is already the value we want, so there is no need to rebuild it.
                return node.ToString().TryParseToJsonNode(out var converted) ? converted : node;
            case JsonValueKind.Object:
                var jsonObject = node.AsObject();

                // the collection can't be modified while it is being enumerated, but expansion
                // is the rare case - so only build a list once we actually have a change.
                List<KeyValuePair<string, JsonNode>>? changes = null;

                foreach (var property in jsonObject)
                {
                    if (property.Value is null) continue;

                    var expanded = ExpandNode(property.Value);
                    if (ReferenceEquals(expanded, property.Value)) continue;

                    (changes ??= []).Add(new(property.Key, expanded));
                }

                if (changes is not null)
                {
                    foreach (var change in changes)
                        jsonObject[change.Key] = change.Value;
                }

                return jsonObject;
            case JsonValueKind.Array:
                var jsonArray = node.AsArray();
                for (int n = 0; n < jsonArray.Count; n++)
                {
                    var child = jsonArray[n];
                    if (child is not null)
                    {
                        var expanded = ExpandNode(child);
                        if (!ReferenceEquals(expanded, child))
                            jsonArray[n] = expanded;
                    }
                }
                return jsonArray;
            default:
                return node;
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
