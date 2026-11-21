using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Jumoo.Json.Converters;

/// <summary>
/// A JSON converter for <see cref="XElement"/>.
/// </summary>
public class JsonXElementConverter : JsonConverter<XElement>
{
    /// <summary>
    ///  Reads an <see cref="XElement"/> from its string representation.
    /// </summary>
    public override XElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return XElement.Parse(reader.GetString() ?? string.Empty);
    }

    /// <summary>
    ///  Writes an <see cref="XElement"/> out as a string value.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, XElement value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
