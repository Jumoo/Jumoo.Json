using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jumoo.Json;

/// <summary>
///  Parsing, validating and serializing <see cref="JsonNode"/> values.
/// </summary>
public static class JsonNodeExtensions
{
    /// <summary>
    /// Checks if the string is a valid JSON string.
    /// </summary>
    /// <remarks>
    ///  Validates with <see cref="JsonDocument"/> rather than parsing to a node graph -
    ///  it reads from pooled buffers and doesn't allocate an object per value, which is
    ///  wasted work when all we want back is a bool.
    /// </remarks>
    public static bool IsValidJsonString(this string? value)
    {
        if (value.LooksLikeJson() is false) return false;

        try
        {
            using var document = JsonDocument.Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to parse a string to a JSON node.
    /// </summary>
    public static bool TryParseToJsonNode(this string? value, [NotNullWhen(true)] out JsonNode? node)
    {
        node = default;
        if (value.LooksLikeJson() is false) return false;

        try
        {
            node = JsonNode.Parse(value, JsonTextOptions.GetNodeOptions());
            return node is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to parse an object to a JSON node.
    /// </summary>
    public static bool TryParseToJsonNode(this object? value, [NotNullWhen(true)] out JsonNode? node)
    {
        node = default;

        if (value?.TryGetValueAs<string>(out var stringValue) is true)
            return stringValue.TryParseToJsonNode(out node);

        return false;
    }

    /// <summary>
    /// Serializes a JSON node to a string.
    /// </summary>
    public static bool TrySerializeJsonNode(this JsonNode? node, [NotNullWhen(true)] out string? result, bool indent = true)
    {
        try
        {
            result = node?.ToJsonString(JsonTextOptions.GetOptions(indent)) ?? default;
            return result is not null;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Converts a string to a JSON node.
    /// </summary>
    public static bool TryConvertToJsonNode(this string value, [NotNullWhen(true)] out JsonNode? node)
    {
        if (value.TryParseToJsonNode(out node) is true)
            return node is not null;

        // not json, so it is just a string value. Creating the node directly leaves the
        // escaping to the writer - quoting it into a string and re-parsing meant anything
        // containing a quote, backslash or control character failed to convert at all.
        node = JsonValue.Create(value);
        return node is not null;
    }

    /// <summary>
    /// Converts an object to a JSON node.
    /// </summary>
    public static bool TryConvertToJsonNode(this object value, [NotNullWhen(true)] out JsonNode? node)
    {
        node = default;
        if (value.TryGetValueAs<string>(out var stringValue) is true)
            return stringValue.TryConvertToJsonNode(out node);

        return false;
    }

    /// <summary>
    /// Converts a string to a JSON node.
    /// </summary>
    public static JsonNode? ToJsonNode(this string? value)
        => TryParseToJsonNode(value, out var node) ? node : default;

    /// <summary>
    /// Converts a string to a JSON node, or null if it cannot be converted.
    /// </summary>
    /// <remarks>
    ///  Unlike <see cref="ToJsonNode(string?)"/> a value that isn't json comes back as a string
    ///  node rather than null.
    /// </remarks>
    public static JsonNode? ConvertToJsonNode(this string value)
        => TryConvertToJsonNode(value, out var node) ? node : default;

    /// <summary>
    /// Converts an object to a JSON node, or null if it cannot be converted.
    /// </summary>
    public static JsonNode? ConvertToJsonNode(this object value)
        => TryConvertToJsonNode(value, out var node) ? node : default;

    /// <summary>
    ///  Checks if the value is a non-string JSON value (array, object, number, boolean).
    /// </summary>
    public static bool IsNonStringJsonValue(this object? value)
        => value is JsonElement { ValueKind: not JsonValueKind.String and not JsonValueKind.Undefined }
           || value is JsonArray or JsonObject;
}