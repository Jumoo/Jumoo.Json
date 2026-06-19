using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jumoo.Json.Tests
{
    public class JsonTextOptionsTests
    {
        [Fact]
        public void GetOptions_Indented_ReturnsOptionsWithWriteIndentedTrue()
        {
            var options = JsonTextOptions.GetOptions(indent: true);

            Assert.NotNull(options);
            Assert.True(options.WriteIndented);
        }

        [Fact]
        public void GetOptions_Flat_ReturnsOptionsWithWriteIndentedFalse()
        {
            var options = JsonTextOptions.GetOptions(indent: false);

            Assert.NotNull(options);
            Assert.False(options.WriteIndented);
        }

        [Fact]
        public void GetOptions_DefaultParameter_ReturnsIndentedOptions()
        {
            var options = JsonTextOptions.GetOptions();

            Assert.True(options.WriteIndented);
        }

        [Fact]
        public void GetNodeOptions_ReturnsCaseInsensitiveOptions()
        {
            var nodeOptions = JsonTextOptions.GetNodeOptions();

            Assert.True(nodeOptions.PropertyNameCaseInsensitive);
        }

        [Fact]
        public void GetOptions_ContainsExpectedBuiltInConverters()
        {
            var options = JsonTextOptions.GetOptions();

            var converterTypes = options.Converters.Select(c => c.GetType()).ToList();
            Assert.Contains(typeof(JsonStringEnumConverter), converterTypes);
        }

        private class TestJsonConverter : JsonConverter<string>
        {
            public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => reader.GetString();

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
                => writer.WriteStringValue(value);
        }
    }
}
