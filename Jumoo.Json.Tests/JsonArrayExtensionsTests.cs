using System.Text.Json.Nodes;

namespace Jumoo.Json.Tests
{
    public class JsonArrayExtensionsTests
    {
        [Fact]
        public void TryParseToJsonArray_ReturnsTrue_ForValidJsonArrayString()
        {
            // Arrange
            string json = "[{\"Name\":\"John\"}, {\"Name\":\"Jane\"}]";

            // Act
            var result = json.TryParseToJsonArray(out var jsonArray);

            // Assert
            Assert.True(result);
            Assert.NotNull(jsonArray);
            Assert.Equal(2, jsonArray.Count);
        }

        [Fact]
        public void TryParseToJsonArray_ReturnsFalse_ForInvalidJsonString()
        {
            // Arrange
            string json = "{\"Name\":\"John\"}"; // Not an array

            // Act
            var result = json.TryParseToJsonArray(out var jsonArray);

            // Assert
            Assert.False(result);
            Assert.Null(jsonArray);
        }

        [Fact]
        public void TryParseToJsonArray_ReturnsFalse_ForNullString()
        {
            // Arrange
            string? json = null;

            // Act
            var result = json.TryParseToJsonArray(out var jsonArray);

            // Assert
            Assert.False(result);
            Assert.Null(jsonArray);
        }

        [Fact]
        public void ToJsonArray_ReturnsJsonArray_ForValidJsonArrayString()
        {
            // Arrange
            string json = "[{\"Name\":\"John\"}, {\"Name\":\"Jane\"}]";

            // Act
            var jsonArray = json.ToJsonArray();

            // Assert
            Assert.NotNull(jsonArray);
            Assert.Equal(2, jsonArray.Count);
        }

        [Fact]
        public void ToJsonArray_ReturnsEmptyArray_ForInvalidJsonString()
        {
            // Arrange
            string json = "{\"Name\":\"John\"}"; // Not an array

            // Act
            var jsonArray = json.ToJsonArray();

            // Assert
            Assert.NotNull(jsonArray);
            Assert.Empty(jsonArray);
        }

        [Fact]
        public void ToJsonArray_ReturnsEmptyArray_ForNullString()
        {
            // Arrange
            string? json = null;

            // Act
            var jsonArray = json.ToJsonArray();

            // Assert
            Assert.NotNull(jsonArray);
            Assert.Empty(jsonArray);
        }

        [Fact]
        public void AsListOfJsonObjects_ReturnsListOfJsonObjects_ForValidJsonArray()
        {
            // Arrange
            var jsonArray = new JsonArray(
                JsonNode.Parse("{\"Name\":\"John\"}")!,
                JsonNode.Parse("{\"Name\":\"Jane\"}")!
            );

            // Act
            var list = jsonArray.AsListOfJsonObjects();

            // Assert
            Assert.NotNull(list);
            Assert.Equal(2, list.Count);
            Assert.Equal("John", list[0]["Name"]!.ToString());
            Assert.Equal("Jane", list[1]["Name"]!.ToString());
        }

        [Fact]
        public void AsListOfJsonObjects_ReturnsEmptyList_ForNullArray()
        {
            // Arrange
            JsonArray? jsonArray = null;

            // Act
            var list = jsonArray.AsListOfJsonObjects();

            // Assert
            Assert.NotNull(list);
            Assert.Empty(list);
        }

        [Fact]
        public void AsListOfJsonObjects_IgnoresNullElements_InJsonArray()
        {
            // Arrange
            var jsonArray = new JsonArray(
                JsonNode.Parse("{\"Name\":\"John\"}")!,
                null,
                JsonNode.Parse("{\"Name\":\"Jane\"}")!
            );

            // Act
            var list = jsonArray.AsListOfJsonObjects();

            // Assert
            Assert.NotNull(list);
            Assert.Equal(2, list.Count);
            Assert.Equal("John", list[0]["Name"]!.ToString());
            Assert.Equal("Jane", list[1]["Name"]!.ToString());
        }
    }
}
