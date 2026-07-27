using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jumoo.Json;

/// <summary>
///  Reading properties off a <see cref="JsonObject"/> as specific types, without throwing.
/// </summary>
public static class JsonPropertyExtensions
{
    /// <summary>
    /// Tries to get a property value as a JSON object.
    /// </summary>
    public static bool TryGetPropertyAsJsonObject(this JsonObject json, string propertyName, [NotNullWhen(true)] out JsonObject? result)
    {
        result = default;

        if (json.TryGetPropertyValue(propertyName, out var propertyNode) is false || propertyNode is null) return false;

        try
        {
            result = propertyNode.GetValueKind() switch
            {
                JsonValueKind.String => new JsonObject
                {
                    { propertyName, propertyNode.ToString() }
                },
                JsonValueKind.Object => propertyNode.AsObject(),
                _ => null
            };

            return result is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to get a property value as a String Value.
    /// </summary>
    public static string? GetPropertyAsString(this JsonObject? obj, string propertyName)
    {
        if (obj?.TryGetPropertyValue(propertyName, out var value) is true)
            return value?.ToString() ?? null;

        return null;
    }

    /// <summary>
    /// Tries to get a property value as a Bool Value
    /// </summary>
    /// <remarks>
    ///  Checks the value kind first. ToString() on anything that isn't a string serialises
    ///  the whole subtree through an indented writer, which is a lot of work to do before
    ///  handing the result to bool.TryParse.
    /// </remarks>
    public static bool GetPropertyAsBool(this JsonObject? obj, string propertyName)
    {
        if (obj?.TryGetPropertyValue(propertyName, out var value) is not true || value is null)
            return false;

        return value.GetValueKind() switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(value.ToString(), out var result) && result,
            _ => false
        };
    }

    /// <summary>
    /// Tries to get a property value as a TResult Value
    /// </summary>
    public static TResult GetPropertyValueOrDefault<TResult>(this JsonObject obj, string propertyName, TResult defaultValue)
    {
        if (obj.TryGetPropertyValue(propertyName, out var value) is false || value is null)
            return defaultValue;

        // TryGetValue is the non-throwing form of GetValue. A type mismatch is an ordinary
        // outcome here (we hand back the default), so it shouldn't cost an exception.
        if (value is JsonValue jsonValue
            && jsonValue.TryGetValue<TResult>(out var result)
            && result is not null)
        {
            return result;
        }

        return defaultValue;
    }


    /// <summary>
    /// Tries to get a property value as a JSON array.
    /// </summary>
    public static bool TryGetPropertyAsArray(this JsonObject obj, string propertyName, [NotNullWhen(true)] out JsonArray? array)
    {
        array = default;

        if (obj.TryGetPropertyValue(propertyName, out var value) is false || value is null)
            return false;

        if (value.GetValueKind() is not JsonValueKind.Array)
            return false;

        try
        {
            array = value.AsArray();
            return array is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get an array from a property
    /// </summary>
    public static JsonArray GetPropertyAsArray(this JsonObject obj, string propertyName)
        => obj.TryGetPropertyAsArray(propertyName, out var value) ? value ?? [] : [];

    /// <summary>
    ///  Gets a Json object from a property.
    /// </summary>
    public static JsonObject? GetPropertyAsJsonObject(this JsonObject obj, string propertyName)
        => obj.TryGetPropertyAsJsonObject(propertyName, out var value) ? value : null;
}
