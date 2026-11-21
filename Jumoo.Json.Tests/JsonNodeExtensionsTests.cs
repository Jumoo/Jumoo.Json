using System.Text.Json.Nodes;

namespace Jumoo.Json.Tests
{
    public class JsonNodeExtensionsTests
    {
        [Fact]
        public void IsValidJsonString_ReturnsTrue_ForValidJsonString()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var result = jsonString.IsValidJsonString();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidJsonString_ReturnsFalse_ForInvalidJsonString()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var result = jsonString.IsValidJsonString();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryParseToJsonNode_ReturnsTrue_ForValidJsonString()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var result = jsonString.TryParseToJsonNode(out var node);

            // Assert
            Assert.True(result);
            Assert.NotNull(node);
            Assert.Equal("value", node!["key"]!.ToString());
        }

        [Fact]
        public void TryParseToJsonNode_ReturnsFalse_ForInvalidJsonString()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var result = jsonString.TryParseToJsonNode(out var node);

            // Assert
            Assert.False(result);
            Assert.Null(node);
        }

        [Fact]
        public void TryParseToJsonNode_ReturnsTrue_ForValidObject()
        {
            // Arrange
            var jsonObject = "{\"key\":\"value\"}";

            // Act
            var result = jsonObject.TryParseToJsonNode(out var node);

            // Assert
            Assert.True(result);
            Assert.NotNull(node);
            Assert.Equal("value", node!["key"]!.ToString());
        }

        [Fact]
        public void TryParseToJsonNode_ReturnsFalse_ForInvalidObject()
        {
            // Arrange
            var invalidObject = 12345;

            // Act
            var result = invalidObject.TryParseToJsonNode(out var node);

            // Assert
            Assert.False(result);
            Assert.Null(node);
        }

        [Fact]
        public void TrySerializeJsonNode_ReturnsTrue_ForValidJsonNode()
        {
            // Arrange
            var jsonNode = JsonNode.Parse("{\"key\":\"value\"}");

            // Act
            var result = jsonNode!.TrySerializeJsonNode(out var serializedString);

            // Assert
            Assert.True(result);
            Assert.NotNull(serializedString);
            Assert.Contains("\"key\": \"value\"", serializedString);
        }

        [Fact]
        public void TrySerializeJsonNode_ReturnsFalse_ForInvalidJsonNode()
        {
            // Arrange
            JsonNode? jsonNode = null;

            string? serializedString = null;
            // Act
            var result = jsonNode.TrySerializeJsonNode(out serializedString);

            // Assert
            Assert.False(result);
            Assert.Null(serializedString);
        }

        [Fact]
        public void TryConvertToJsonNode_ReturnsTrue_ForValidJsonString()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var result = jsonString.TryConvertToJsonNode(out var node);

            // Assert
            Assert.True(result);
            Assert.NotNull(node);
            Assert.Equal("value", node!["key"]!.ToString());
        }

        [Fact]
        public void TryConvertToJsonNode_ReturnsTrue_ForNonJsonString()
        {
            // Arrange
            var nonJsonString = "simple string";

            // Act
            var result = nonJsonString.TryConvertToJsonNode(out var node);

            // Assert
            Assert.True(result);
            Assert.NotNull(node);
            Assert.Equal("simple string", node!.ToString());
        }

        [Fact]
        public void TryConvertToJsonNode_ReturnsJsonNode_ForString()
        {
            // Arrange
            var invalidString = "invalid json";

            // Act
            var result = invalidString.TryConvertToJsonNode(out var node);

            // Assert
            Assert.True(result);
            Assert.Equal(invalidString, node?.ToString());
        }

        [Fact]
        public void ToJsonNode_ReturnsJsonNode_ForValidJsonString()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var node = jsonString.ToJsonNode();

            // Assert
            Assert.NotNull(node);
            Assert.Equal("value", node!["key"]!.ToString());
        }

        [Fact]
        public void ToJsonNode_ReturnsNull_ForInvalidJsonString()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var node = jsonString.ToJsonNode();

            // Assert
            Assert.Null(node);
        }

        [Theory]
        [InlineData("He said \"hello\"")]
        [InlineData("c:\\temp\\file.txt")]
        [InlineData("line one\nline two")]
        public void TryConvertToJsonNode_ReturnsTrue_ForStringNeedingEscaping(string value)
        {
            // Act
            var result = value.TryConvertToJsonNode(out var node);

            // Assert
            Assert.True(result);
            Assert.Equal(value, node?.ToString());
        }

        [Fact]
        public void ConvertToJsonNode_ReturnsJsonNode_ForValidJsonString()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var node = jsonString.ConvertToJsonNode();

            // Assert
            Assert.NotNull(node);
            Assert.Equal("value", node!["key"]!.ToString());
        }

        [Fact]
        public void ConvertToJsonNode_ReturnsStringNode_ForNonJsonString()
        {
            // Arrange
            var value = "simple string";

            // Act
            var node = value.ConvertToJsonNode();

            // Assert
            Assert.NotNull(node);
            Assert.Equal(value, node!.ToString());
        }

        [Fact]
        public void ConvertToJsonNode_ReturnsJsonNode_ForObjectHoldingJson()
        {
            // Arrange
            object value = "{\"key\":\"value\"}";

            // Act
            var node = value.ConvertToJsonNode();

            // Assert
            Assert.NotNull(node);
            Assert.Equal("value", node!["key"]!.ToString());
        }

        [Fact]
        public void ConvertToJsonNode_ReturnsStringNode_ForObjectThatIsNotJson()
        {
            // Arrange - anything TryConvertTo can turn into a string ends up as a string node.
            object value = 12345;

            // Act
            var node = value.ConvertToJsonNode();

            // Assert
            Assert.NotNull(node);
            Assert.Equal("12345", node!.ToString());
        }

        [Fact]
        public void IsNonStringJsonValue_ReturnsTrue_ForJsonObjectAndArray()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();
            var jsonArray = JsonNode.Parse("[1,2,3]")!.AsArray();

            // Assert
            Assert.True(jsonObject.IsNonStringJsonValue());
            Assert.True(jsonArray.IsNonStringJsonValue());
        }

        [Theory]
        [InlineData("{\"key\":\"value\"}", true)]
        [InlineData("[1,2,3]", true)]
        [InlineData("12", true)]
        [InlineData("true", true)]
        [InlineData("\"a string\"", false)]
        public void IsNonStringJsonValue_ChecksTheValueKind_ForJsonElement(string json, bool expected)
        {
            // Arrange
            var element = System.Text.Json.JsonDocument.Parse(json).RootElement;

            // Act
            var result = ((object)element).IsNonStringJsonValue();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsNonStringJsonValue_ReturnsFalse_ForNullAndPlainValues()
        {
            // Assert
            Assert.False(((object?)null).IsNonStringJsonValue());
            Assert.False("a string".IsNonStringJsonValue());
            Assert.False(12345.IsNonStringJsonValue());
        }
    }
}
