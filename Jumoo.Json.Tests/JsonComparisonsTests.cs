namespace Jumoo.Json.Tests
{
    public class JsonComparisonsTests
    {
        [Fact]
        public void IsJsonEqual_ReturnsTrue_ForEqualJsonObjects()
        {
            // Arrange
            var obj1 = new { Name = "John", Age = 30 };
            var obj2 = new { Name = "John", Age = 30 };

            // Act
            var result = obj1.IsJsonEqual(obj2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsJsonEqual_ReturnsFalse_ForDifferentJsonObjects()
        {
            // Arrange
            var obj1 = new { Name = "John", Age = 30 };
            var obj2 = new { Name = "Jane", Age = 25 };

            // Act
            var result = obj1.IsJsonEqual(obj2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsJsonEqual_ReturnsFalse_WhenOneObjectIsNull()
        {
            // Arrange
            var obj1 = new { Name = "John", Age = 30 };
            object obj2 = null;

            // Act
            var result = obj1.IsJsonEqual(obj2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsJsonEqual_ReturnsTrue_ForBothObjectsNull()
        {
            // Arrange
            object obj1 = null;
            object obj2 = null;

            // Act
            var result = obj1.IsJsonEqual(obj2);

            // Assert
            Assert.True(result);
        }
    }
}
