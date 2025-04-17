using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

using Umbraco.Extensions;

namespace Jumoo.Json;
public static class JsonNodeExtensions
{
    /// <summary>
    /// Checks if the string is a valid JSON string.
    /// </summary>
    public static bool IsValidJsonString(this string? value)
        => value?.TryParseToJsonNode(out _) is true;

    /// <summary>
    /// Tries to parse a string to a JSON node.
    /// </summary>
    public static bool TryParseToJsonNode(this string? value, [NotNullWhen(true)] out JsonNode? node)
    {
        node = default;
        if (string.IsNullOrEmpty(value) || value.DetectIsJson() is false) return false;

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
    public static bool TrySerializeJsonNode(this JsonNode node, [NotNullWhen(true)] out string? result, bool indent = true)
    {
        try
        {
            result = node.ToJsonString(JsonTextOptions.GetOptions(indent));
            return true;
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
        node = default;

        if (value.TryParseToJsonNode(out node) is true)
            return node is not null;

        try
        {
            node = JsonNode.Parse($"\"{value}\"", JsonTextOptions.GetNodeOptions());
            return node is not null;
        }
        catch
        {
            return false;
        }
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

}