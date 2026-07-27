using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jumoo.Json;

/// <summary>
///  Extensions for getting <see cref="JsonObject"/> values out of strings and objects.
/// </summary>
public static class JsonObjectExtensions
{
    /// <summary>
    /// Parses a string to a JSON object.
    /// </summary>
    public static bool TryParseToJsonObject(this string? value, [NotNullWhen(true)] out JsonObject? node)
    {
        node = default;
        if (value.TryParseToJsonNode(out var jsonNode) is false) return false;
        if (jsonNode.GetValueKind() is not JsonValueKind.Object) return false;

        node = jsonNode.AsObject();
        return node is not null;
    }

    /// <summary>
    /// Tries to convert an object to a JSON object.
    /// </summary>
    public static bool TryConvertToJsonObject(this object? value, [NotNullWhen(true)] out JsonObject? result)
    {
        result = default;
        if (value is null || value.TryConvertToJsonNode(out var node) is false) return false;
        if (node.GetValueKind() is not JsonValueKind.Object) return false;

        result = node.AsObject();
        return result is not null;
    }

    /// <summary>
    /// Converts a string to a JSON object.
    /// </summary>
    public static JsonObject? ToJsonObject(this string? value)
        => value.TryParseToJsonObject(out var node) is true ? node : default;

    /// <summary>
    /// Converts an object to a JSON object.
    /// </summary>
    public static JsonObject? ConvertToJsonObject(this object? value)
        => value.TryConvertToJsonObject(out var node) is true ? node : default;
}
