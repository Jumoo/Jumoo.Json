using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Jumoo.Json.Converters;

namespace Jumoo.Json.Tests
{
    public class JsonXElementConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public JsonXElementConverterTests()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new JsonXElementConverter());
        }

        [Fact]
        public void Write_SerializesXElementAsString()
        {
            var element = XElement.Parse("<root><child>value</child></root>");

            var json = JsonSerializer.Serialize(element, _options);

            Assert.NotNull(json);
            Assert.Contains("root", json);
            Assert.Contains("child", json);
            Assert.Contains("value", json);
        }

        [Fact]
        public void Read_DeserializesXElementFromString()
        {
            var xmlString = "<root><child>value</child></root>";
            var json = $"\"{xmlString.Replace("\"", "\\\"")}\"";

            var element = JsonSerializer.Deserialize<XElement>(json, _options);

            Assert.NotNull(element);
            Assert.Equal("root", element!.Name.LocalName);
            Assert.Equal("value", element.Element("child")?.Value);
        }

        [Fact]
        public void RoundTrip_PreservesXElement()
        {
            var original = XElement.Parse("<root attr=\"test\"><child>value</child></root>");

            var json = JsonSerializer.Serialize(original, _options);
            var restored = JsonSerializer.Deserialize<XElement>(json, _options);

            Assert.NotNull(restored);
            Assert.Equal(original.ToString(), restored!.ToString());
        }
    }
}
