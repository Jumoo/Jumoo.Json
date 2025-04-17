using System;
using Xunit;

namespace Jumoo.Json.Tests
{
    public class JsonEscapeExtensionsTests
    {
        [Fact]
        public void GetEscapedJsonValue_ReturnsEscapedJson_ForValidJsonString()
        {
            // Arrange
            var jsonString = "{\"key\":\"value\"}";

            // Act
            var escapedValue = jsonString.GetEscapedJsonValue();

            // Assert
            Assert.NotNull(escapedValue);
            Assert.Equal("{\"key\":\"value\"}", escapedValue);
        }

        [Fact]
        public void GetEscapedJsonValue_ReturnsOriginalString_ForInvalidJsonString()
        {
            // Arrange
            var invalidJsonString = "invalid json";

            // Act
            var escapedValue = invalidJsonString.GetEscapedJsonValue();

            // Assert
            Assert.NotNull(escapedValue);
            Assert.Equal(invalidJsonString, escapedValue);
        }

        [Fact]
        public void GetEscapedJsonValue_ReturnsOriginalString_ForEmptyString()
        {
            // Arrange
            var emptyString = "";

            // Act
            var escapedValue = emptyString.GetEscapedJsonValue();

            // Assert
            Assert.NotNull(escapedValue);
            Assert.Equal(emptyString, escapedValue);
        }

        [Fact]
        public void GetEscapedJsonValue_ReturnsOriginalString_ForNullString()
        {
            // Arrange
            string? nullString = null;

            // Act
            var escapedValue = nullString?.GetEscapedJsonValue();

            // Assert
            Assert.Null(escapedValue);
        }

        [Fact]
        public void GetEscapedJsonValue_ReturnsEscapedJson_ForNestedJsonString()
        {
            // Arrange
            var nestedJsonString = "{\"key\":{\"nestedKey\":\"nestedValue\"}}";

            // Act
            var escapedValue = nestedJsonString.GetEscapedJsonValue();

            // Assert
            Assert.NotNull(escapedValue);
            Assert.Equal("{\"key\":{\"nestedKey\":\"nestedValue\"}}", escapedValue);
        }
    }
}
