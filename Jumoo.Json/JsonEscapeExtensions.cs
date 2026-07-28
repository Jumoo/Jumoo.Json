namespace Jumoo.Json;

/// <summary>
///  Normalising a JSON string into a safely escaped, flat form.
/// </summary>
public static class JsonEscapeExtensions
{
    /// <summary>
    /// Gets a safely escaped version of a string of a json object.
    /// </summary>
    public static string GetEscapedJsonValue(this string value)
    {
        try
        {
            if (value.TryParseToJsonNode(out var jsonValue) is false) return value;
            return jsonValue.SerializeJsonNode(false) ?? value;
        }
        catch
        {
            return value;
        }
    }
}
