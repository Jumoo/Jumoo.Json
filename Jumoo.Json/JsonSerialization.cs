using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

using Umbraco.Extensions;

namespace Jumoo.Json;
public static class JsonSerialization
{
    /// <summary>
    /// Tries to deserialize a JSON string to an object of type TObject.
    /// </summary>
    public static bool TryDeserialize<TObject>(this string? value, [NotNullWhen(true)] out TObject? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        try
        {
            result = JsonSerializer.Deserialize<TObject>(value, JsonTextOptions.GetOptions());
            return result is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to deserialize a JSON string to an object of type Type.
    /// </summary>
    public static bool TryDeserialize(this string? value, Type type, [NotNullWhen(true)] out object? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        try
        {
            result = JsonSerializer.Deserialize(value, type, JsonTextOptions.GetOptions());
            return result is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type Type
    /// </summary>
    public static object? DeserializeJson(this string? value, Type type)
        => TryDeserialize(value, type, out var result) ? result : default;

    /// <summary>
    /// Deserializes a JSON string to an object of type TObject.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static TObject? DeserializeJson<TObject>(this string? value)
        => TryDeserialize<TObject>(value, out var result) ? result : default;


    /// <summary>
    /// Tries to serializes an object to a JSON string.
    /// </summary>
    public static bool TrySerializeJsonString(this object? value, [NotNullWhen(true)] out string? result, bool indent = true)
    {
        result = default;
        if (value is null) return false;
        try
        {
            result = JsonSerializer.Serialize(value, JsonTextOptions.GetOptions(indent));
            return result is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    public static string? SerializeJsonString(this object? value, bool indent = true)
        => TrySerializeJsonString(value, out var result, indent) ? result : default;

    /// <summary>
    /// Serializes a JSON node to a string.
    /// </summary>
    public static string? SerializeJsonNode(this JsonNode? node, bool indent = true)
    {
        if (node is null) return null;
        node.TrySerializeJsonNode(out var result, indent);
        return result ?? node.ToString();
    }

    /// <summary>
    /// Tries to convert an object to a TObject item.
    /// </summary>
    public static bool TryGetValueAs<TObject>(this object value, [NotNullWhen(true)] out TObject? result)
    {
        result = default;
        if (value == null) return false;
        var attempt = value.TryConvertTo<TObject>();
        if (attempt.Success is false || attempt.Result is null)
        {
            result = default;
            return attempt.Success;
        }

        result = attempt.Result;
        return true;
    }
}
