using System.Text.Json.Nodes;

namespace Jumoo.Json.Tests
{
    public class JsonExpansionsTests
    {
        [Fact]
        public void ExpandAllJsonInToken_ExpandsNestedJsonStrings()
        {
            // Arrange
            var jsonNode = JsonNode.Parse("{\"key\":\"{\\\"nestedKey\\\":\\\"nestedValue\\\"}\"}");

            // Act
            var expandedNode = jsonNode!.ExpandAllJsonInToken();

            // Assert
            Assert.NotNull(expandedNode);
            var expandedObject = expandedNode.AsObject();
            Assert.True(expandedObject["key"] is JsonObject);
            Assert.Equal("nestedValue", expandedObject["key"]!["nestedKey"]!.ToString());
        }

        [Fact]
        public void ExpandAllJsonInToken_ReturnsOriginalNode_ForNonExpandableJson()
        {
            // Arrange
            var jsonNode = JsonNode.Parse("{\"key\":\"value\"}");

            // Act
            var expandedNode = jsonNode!.ExpandAllJsonInToken();

            // Assert
            Assert.NotNull(expandedNode);
            Assert.Equal(jsonNode!.ToJsonString(), expandedNode.ToJsonString());
        }

        [Fact]
        public void TryExpandJsonNodeValue_ReturnsTrue_ForExpandableJsonString()
        {
            // Arrange
            var jsonNode = JsonValue.Create("{\"nestedKey\":\"nestedValue\"}");

            // Act
            var result = jsonNode!.TryExpandJsonNodeValue(out var expandedNode);

            // Assert
            Assert.True(result);
            Assert.NotNull(expandedNode);
            Assert.True(expandedNode is JsonObject);
            Assert.Equal("nestedValue", expandedNode!["nestedKey"]!.ToString());
        }

        [Fact]
        public void TryExpandJsonNodeValue_ReturnsTrue_ForNonExpandableJson()
        {
            // Arrange
            var jsonNode = JsonValue.Create("simple string");

            // Act
            var result = jsonNode!.TryExpandJsonNodeValue(out var expandedNode);

            // Assert
            Assert.True(result);
            Assert.Equal(jsonNode.ToString(), expandedNode?.ToString());
        }

        [Fact]
        public void ConvertStringToExpandedJson_ExpandsValidJsonString()
        {
            // Arrange
            var jsonString = "{\"key\":\"{\\\"nestedKey\\\":\\\"nestedValue\\\"}\"}";

            // Act
            var expandedNode = jsonString.ConvertStringToExpandedJson();

            // Assert
            Assert.NotNull(expandedNode);
            var expandedObject = expandedNode!.AsObject();
            Assert.True(expandedObject["key"] is JsonObject);
            Assert.Equal("nestedValue", expandedObject["key"]!["nestedKey"]!.ToString());
        }

        [Fact]
        public void ConvertStringToExpandedJson_ReturnsNull_ForInvalidJsonString()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var expandedNode = jsonString.ConvertStringToExpandedJson();

            // Assert
            Assert.Null(expandedNode);
        }

        [Fact]
        public void ConvertStringToExpandedJsonString_ExpandsAndSerializesJson()
        {
            // Arrange
            var jsonString = "{\"key\":\"{\\\"nestedKey\\\":\\\"nestedValue\\\"}\"}";

            // Act
            var expandedJsonString = jsonString.ConvertStringToExpandedJsonString();

            // Assert
            Assert.NotNull(expandedJsonString);
            Assert.Contains("\"nestedKey\": \"nestedValue\"", expandedJsonString);
        }

        [Fact]
        public void ConvertStringToExpandedJsonString_ReturnsOriginalString_ForInvalidJson()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var expandedJsonString = jsonString.ConvertStringToExpandedJsonString();

            // Assert
            Assert.Equal(jsonString, expandedJsonString);
        }
    }
}
