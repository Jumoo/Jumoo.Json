using System.Text.Json;

namespace Jumoo.Json;
public static class JsonComparisons
{
    /// <summary>
    /// Compares two objects to see if they are equal in JSON format.
    /// </summary>
    /// <remarks>
    ///  Compares the utf8 bytes rather than materialising two strings - same result, but
    ///  half the memory and no large object heap traffic for big values.
    /// </remarks>
    public static bool IsJsonEqual(this object? currentObject, object? newObject)
    {
        if (ReferenceEquals(currentObject, newObject)) return true;
        if (currentObject is null || newObject is null) return false;

        using var current = new PooledByteBufferWriter();
        using var updated = new PooledByteBufferWriter();

        try
        {
            WriteJson(current, currentObject);
            WriteJson(updated, newObject);
        }
        catch
        {
            return false;
        }

        return current.WrittenSpan.SequenceEqual(updated.WrittenSpan);

        static void WriteJson(PooledByteBufferWriter buffer, object value)
        {
            using var writer = new Utf8JsonWriter(buffer);
            JsonSerializer.Serialize(writer, value, JsonTextOptions.GetOptions(false));
        }
    }
}
