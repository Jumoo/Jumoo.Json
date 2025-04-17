using System.Text.Json.Nodes;

namespace Jumoo.Json.Tests
{
    public class JsonSerializationTests
    {
        [Fact]
        public void TryDeserialize_Generic_ReturnsTrue_ForValidJson()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var result = jsonString.TryDeserialize<Dictionary<string, string>>(out var deserializedObject);

            // Assert
            Assert.True(result);
            Assert.NotNull(deserializedObject);
            Assert.Equal("value", deserializedObject!["key"]);
        }

        [Fact]
        public void TryDeserialize_Generic_ReturnsFalse_ForInvalidJson()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var result = jsonString.TryDeserialize<Dictionary<string, string>>(out var deserializedObject);

            // Assert
            Assert.False(result);
            Assert.Null(deserializedObject);
        }

        [Fact]
        public void TryDeserialize_Type_ReturnsTrue_ForValidJson()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var result = jsonString.TryDeserialize(typeof(Dictionary<string, string>), out var deserializedObject);

            // Assert
            Assert.True(result);
            Assert.NotNull(deserializedObject);
            Assert.IsType<Dictionary<string, string>>(deserializedObject);
            Assert.Equal("value", ((Dictionary<string, string>)deserializedObject!)["key"]);
        }

        [Fact]
        public void TryDeserialize_Type_ReturnsFalse_ForInvalidJson()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var result = jsonString.TryDeserialize(typeof(Dictionary<string, string>), out var deserializedObject);

            // Assert
            Assert.False(result);
            Assert.Null(deserializedObject);
        }

        [Fact]
        public void DeserializeJson_Generic_ReturnsObject_ForValidJson()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var deserializedObject = jsonString.DeserializeJson<Dictionary<string, string>>();

            // Assert
            Assert.NotNull(deserializedObject);
            Assert.Equal("value", deserializedObject!["key"]);
        }

        [Fact]
        public void DeserializeJson_Generic_ReturnsNull_ForInvalidJson()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var deserializedObject = jsonString.DeserializeJson<Dictionary<string, string>>();

            // Assert
            Assert.Null(deserializedObject);
        }

        [Fact]
        public void DeserializeJson_Type_ReturnsObject_ForValidJson()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var deserializedObject = jsonString.DeserializeJson(typeof(Dictionary<string, string>));

            // Assert
            Assert.NotNull(deserializedObject);
            Assert.IsType<Dictionary<string, string>>(deserializedObject);
            Assert.Equal("value", ((Dictionary<string, string>)deserializedObject!)["key"]);
        }

        [Fact]
        public void DeserializeJson_Type_ReturnsNull_ForInvalidJson()
        {
            // Arrange
            var jsonString = "invalid json";

            // Act
            var deserializedObject = jsonString.DeserializeJson(typeof(Dictionary<string, string>));

            // Assert
            Assert.Null(deserializedObject);
        }

        [Fact]
        public void TrySerializeJsonString_ReturnsTrue_ForValidObject()
        {
            // Arrange
            var obj = new { key = "value" };

            // Act
            var result = obj.TrySerializeJsonString(out var jsonString);

            // Assert
            Assert.True(result);
            Assert.NotNull(jsonString);
            Assert.Contains("\"key\": \"value\"", jsonString);
        }

        [Fact]
        public void TrySerializeJsonString_ReturnsFalse_ForNullObject()
        {
            // Arrange
            object? obj = null;

            // Act
            var result = obj.TrySerializeJsonString(out var jsonString);

            // Assert
            Assert.False(result);
            Assert.Null(jsonString);
        }

        [Fact]
        public void SerializeJsonString_ReturnsJsonString_ForValidObject()
        {
            // Arrange
            var obj = new { key = "value" };

            // Act
            var jsonString = obj.SerializeJsonString();

            // Assert
            Assert.NotNull(jsonString);
            Assert.Contains("\"key\": \"value\"", jsonString);
        }

        [Fact]
        public void SerializeJsonString_ReturnsNull_ForNullObject()
        {
            // Arrange
            object? obj = null;

            // Act
            var jsonString = obj.SerializeJsonString();

            // Assert
            Assert.Null(jsonString);
        }

        [Fact]
        public void SerializeJsonNode_ReturnsJsonString_ForValidJsonNode()
        {
            // Arrange
            var jsonNode = JsonNode.Parse("{\"key\":\"value\"}");

            // Act
            var jsonString = jsonNode!.SerializeJsonNode();

            // Assert
            Assert.NotNull(jsonString);
            Assert.Contains("\"key\": \"value\"", jsonString);
        }

        [Fact]
        public void SerializeJsonNode_ReturnsNull_ForNullJsonNode()
        {
            // Arrange
            JsonNode? jsonNode = null;

            // Act
            var jsonString = jsonNode.SerializeJsonNode();

            // Assert
            Assert.Null(jsonString);
        }

        [Fact]
        public void TryGetValueAs_ReturnsTrue_ForConvertibleValue()
        {
            // Arrange
            object value = "42";

            // Act
            var result = value.TryGetValueAs<int>(out var convertedValue);

            // Assert
            Assert.True(result);
            Assert.Equal(42, convertedValue);
        }

        [Fact]
        public void TryGetValueAs_ReturnsFalse_ForNonConvertibleValue()
        {
            // Arrange
            object value = "notAnInt";

            // Act
            var result = value.TryGetValueAs<int>(out var convertedValue);

            // Assert
            Assert.False(result);
            Assert.Equal(0, convertedValue);
        }
    }
}
