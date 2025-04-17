using System.Text.Json.Nodes;

namespace Jumoo.Json.Tests
{
    public class JsonPropertyExtensionsTests
    {
        [Fact]
        public void TryGetPropertyAsJsonObject_ReturnsTrue_ForValidJsonObjectProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":{\"nestedKey\":\"nestedValue\"}}")!.AsObject();

            // Act
            var result = jsonObject.TryGetPropertyAsJsonObject("key", out var propertyObject);

            // Assert
            Assert.True(result);
            Assert.NotNull(propertyObject);
            Assert.Equal("nestedValue", propertyObject!["nestedKey"]!.ToString());
        }

        [Fact]
        public void TryGetPropertyAsJsonObject_ReturnsFalse_ForNonExistentProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();

            // Act
            var result = jsonObject.TryGetPropertyAsJsonObject("nonExistentKey", out var propertyObject);

            // Assert
            Assert.False(result);
            Assert.Null(propertyObject);
        }

        [Fact]
        public void GetPropertyAsString_ReturnsStringValue_ForValidProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyAsString("key");

            // Assert
            Assert.Equal("value", result);
        }

        [Fact]
        public void GetPropertyAsString_ReturnsNull_ForNonExistentProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyAsString("nonExistentKey");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetPropertyAsBool_ReturnsTrue_ForValidBooleanProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":true}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyAsBool("key");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetPropertyAsBool_ReturnsFalse_ForInvalidOrNonExistentProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"notABoolean\"}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyAsBool("key");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetPropertyValueOrDefault_ReturnsValue_ForValidProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":42}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyValueOrDefault("key", 0);

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void GetPropertyValueOrDefault_ReturnsDefaultValue_ForNonExistentProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":42}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyValueOrDefault("nonExistentKey", 0);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void TryGetPropertyAsArray_ReturnsTrue_ForValidJsonArrayProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":[1,2,3]}")!.AsObject();

            // Act
            var result = jsonObject.TryGetPropertyAsArray("key", out var jsonArray);

            // Assert
            Assert.True(result);
            Assert.NotNull(jsonArray);
            Assert.Equal(3, jsonArray!.Count);
        }

        [Fact]
        public void TryGetPropertyAsArray_ReturnsFalse_ForNonArrayProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();

            // Act
            var result = jsonObject.TryGetPropertyAsArray("key", out var jsonArray);

            // Assert
            Assert.False(result);
            Assert.Null(jsonArray);
        }

        [Fact]
        public void GetPropertyAsArray_ReturnsArray_ForValidJsonArrayProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":[1,2,3]}")!.AsObject();

            // Act
            var jsonArray = jsonObject.GetPropertyAsArray("key");

            // Assert
            Assert.NotNull(jsonArray);
            Assert.Equal(3, jsonArray.Count);
        }

        [Fact]
        public void GetPropertyAsArray_ReturnsEmptyArray_ForNonExistentOrInvalidProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();

            // Act
            var jsonArray = jsonObject.GetPropertyAsArray("nonExistentKey");

            // Assert
            Assert.NotNull(jsonArray);
            Assert.Empty(jsonArray);
        }

        [Fact]
        public void GetPropertyAsJsonObject_ReturnsJsonObject_ForValidProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":{\"nestedKey\":\"nestedValue\"}}")!.AsObject();

            // Act
            var propertyObject = jsonObject.GetPropertyAsJsonObject("key");

            // Assert
            Assert.NotNull(propertyObject);
            Assert.Equal("nestedValue", propertyObject!["nestedKey"]!.ToString());
        }

        [Fact]
        public void GetPropertyAsJsonObject_ReturnsNull_ForNonExistentOrInvalidProperty()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();

            // Act
            var propertyObject = jsonObject.GetPropertyAsJsonObject("nonExistentKey");

            // Assert
            Assert.Null(propertyObject);
        }
    }
}
