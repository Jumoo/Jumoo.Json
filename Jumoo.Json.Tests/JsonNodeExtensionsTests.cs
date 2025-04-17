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
    }
}
