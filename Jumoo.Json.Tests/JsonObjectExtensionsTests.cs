using System.Text.Json.Nodes;

namespace Jumoo.Json.Tests
{
    public class JsonObjectExtensionsTests
    {
        [Fact]
        public void TryParseToJsonObject_ReturnsTrue_ForValidJsonObjectString()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var result = jsonString.TryParseToJsonObject(out var jsonObject);

            // Assert
            Assert.True(result);
            Assert.NotNull(jsonObject);
            Assert.Equal("value", jsonObject!["key"]!.ToString());
        }

        [Fact]
        public void TryParseToJsonObject_ReturnsFalse_ForInvalidJsonString()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var result = jsonString.TryParseToJsonObject(out var jsonObject);

            // Assert
            Assert.False(result);
            Assert.Null(jsonObject);
        }

        [Fact]
        public void TryParseToJsonObject_ReturnsFalse_ForJsonArrayString()
        {
            // Arrange
            var jsonString = "[{\"key\":\"value\"}]";

            // Act
            var result = jsonString.TryParseToJsonObject(out var jsonObject);

            // Assert
            Assert.False(result);
            Assert.Null(jsonObject);
        }

        [Fact]
        public void TryConvertToJsonObject_ReturnsTrue_ForValidJsonObject()
        {
            // Arrange
            var jsonObject = "{\"key\":\"value\"}";

            // Act
            var result = jsonObject.TryConvertToJsonObject(out var resultObject);

            // Assert
            Assert.True(result);
            Assert.NotNull(resultObject);
            Assert.Equal("value", resultObject!["key"]!.ToString());
        }

        [Fact]
        public void TryConvertToJsonObject_ReturnsFalse_ForInvalidObject()
        {
            // Arrange
            var invalidObject = 12345;

            // Act
            var result = invalidObject.TryConvertToJsonObject(out var resultObject);

            // Assert
            Assert.False(result);
            Assert.Null(resultObject);
        }

        [Fact]
        public void TryConvertToJsonObject_ReturnsFalse_ForJsonArray()
        {
            // Arrange
            var jsonArray = "[{\"key\":\"value\"}]";

            // Act
            var result = jsonArray.TryConvertToJsonObject(out var resultObject);

            // Assert
            Assert.False(result);
            Assert.Null(resultObject);
        }

        [Fact]
        public void ToJsonObject_ReturnsJsonObject_ForValidJsonString()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var jsonObject = jsonString.ToJsonObject();

            // Assert
            Assert.NotNull(jsonObject);
            Assert.Equal("value", jsonObject!["key"]!.ToString());
        }

        [Fact]
        public void ToJsonObject_ReturnsNull_ForInvalidJsonString()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var jsonObject = jsonString.ToJsonObject();

            // Assert
            Assert.Null(jsonObject);
        }

        [Fact]
        public void ConvertToJsonObject_ReturnsJsonObject_ForValidObject()
        {
            // Arrange
            var jsonObject = "{\"key\":\"value\"}";

            // Act
            var resultObject = jsonObject.ConvertToJsonObject();

            // Assert
            Assert.NotNull(resultObject);
            Assert.Equal("value", resultObject!["key"]!.ToString());
        }

        [Fact]
        public void ConvertToJsonObject_ReturnsNull_ForInvalidObject()
        {
            // Arrange
            var invalidObject = 12345;

            // Act
            var resultObject = invalidObject.ConvertToJsonObject();

            // Assert
            Assert.Null(resultObject);
        }

        [Fact]
        public void AddOrRemoveIfNull_SetsTheProperty_WhenTheValueIsNotNull()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();

            // Act
            jsonObject.AddOrRemoveIfNull("added", JsonValue.Create("newValue"));

            // Assert
            Assert.Equal("newValue", jsonObject["added"]!.ToString());
        }

        [Fact]
        public void AddOrRemoveIfNull_RemovesTheProperty_WhenTheValueIsNull()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();

            // Act
            jsonObject.AddOrRemoveIfNull<JsonNode>("key", null);

            // Assert
            Assert.False(jsonObject.ContainsKey("key"));
        }

        [Fact]
        public void AddOrRemoveIfNull_DoesNothing_ForANullObject()
        {
            // Arrange
            JsonObject? jsonObject = null;

            // Act / Assert - the point is that this doesn't throw.
            jsonObject.AddOrRemoveIfNull("key", JsonValue.Create("value"));
        }
    }
}
