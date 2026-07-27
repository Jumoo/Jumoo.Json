using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

using Umbraco.Extensions;

namespace Jumoo.Json;

/// <summary>
///  Serializing and deserializing objects using the shared <see cref="JsonTextOptions"/>.
/// </summary>
public static class JsonSerialization
{
    /// <summary>
    /// Tries to deserialize a JSON string to an object of type TObject.
    /// </summary>
    public static bool TryDeserialize<TObject>(this string? value, [NotNullWhen(true)] out TObject? result)
    {
        result = default;
        if (value.LooksLikeJson() is false) return false;

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
        if (value.LooksLikeJson() is false) return false;

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
    /// Tries to deserialize utf8 JSON bytes to an object of type TObject.
    /// </summary>
    /// <remarks>
    ///  Prefer this over the string overloads when the source is already utf8 (a file, a
    ///  response body). The string form has to be transcoded back to utf8 to be parsed,
    ///  so the string itself is pure overhead.
    /// </remarks>
    public static bool TryDeserialize<TObject>(this ReadOnlySpan<byte> utf8, [NotNullWhen(true)] out TObject? result)
    {
        result = default;
        if (utf8.LooksLikeJson() is false) return false;

        try
        {
            result = JsonSerializer.Deserialize<TObject>(utf8, JsonTextOptions.GetOptions());
            return result is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to deserialize a stream of utf8 JSON to an object of type TObject.
    /// </summary>
    public static bool TryDeserialize<TObject>(this Stream stream, [NotNullWhen(true)] out TObject? result)
    {
        result = default;
        if (stream is null) return false;

        try
        {
            result = JsonSerializer.Deserialize<TObject>(stream, JsonTextOptions.GetOptions());
            return result is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deserializes a stream of utf8 JSON to an object of type TObject.
    /// </summary>
    public static async Task<TObject?> DeserializeJsonAsync<TObject>(this Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream is null) return default;

        try
        {
            return await JsonSerializer.DeserializeAsync<TObject>(
                stream, JsonTextOptions.GetOptions(), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Serializes an object as utf8 JSON straight into a stream.
    /// </summary>
    /// <remarks>
    ///  Avoids building the whole document as a string first, which for large values meant a
    ///  large object heap allocation just to hand the bytes straight back out again.
    /// </remarks>
    public static bool TrySerializeToStream(this object? value, Stream stream, bool indent = true)
    {
        if (value is null || stream is null) return false;

        try
        {
            JsonSerializer.Serialize(stream, value, JsonTextOptions.GetOptions(indent));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes an object as utf8 JSON straight into a stream.
    /// </summary>
    public static async Task<bool> TrySerializeToStreamAsync(this object? value, Stream stream, bool indent = true, CancellationToken cancellationToken = default)
    {
        if (value is null || stream is null) return false;

        try
        {
            await JsonSerializer.SerializeAsync(
                stream, value, JsonTextOptions.GetOptions(indent), cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

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
        if (value is null) return false;

        // already the type we want - no need to go near a converter.
        if (value is TObject direct)
        {
            result = direct;
            return true;
        }

        // Umbraco's TryConvertTo turns a JsonElement into a string cleanly, but throws
        // (and swallows) an InvalidCastException for JsonElement -> value type. Do the
        // value-type conversion with System.Text.Json first to avoid that noise; string
        // and anything STJ can't handle fall through to TryConvertTo below.
        if (value is JsonElement element && typeof(TObject) != typeof(string))
        {
            try
            {
                result = element.Deserialize<TObject>(JsonTextOptions.GetOptions());
                if (result is not null) return true;
            }
            catch
            {
                // not something STJ could convert directly - fall back to TryConvertTo below.
            }
        }

        var attempt = value.TryConvertTo<TObject>();
        if (attempt.Success is false || attempt.Result is null) return false;

        result = attempt.Result;
        return true;
    }
}
