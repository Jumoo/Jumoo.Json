using System.Text;

namespace Jumoo.Json.Tests
{
    /// <summary>
    ///  The utf8 / stream overloads, which exist so callers with a file or response body
    ///  don't have to materialise it as a string first.
    /// </summary>
    public class JsonStreamSerializationTests
    {
        private sealed class Person
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
        }

        private const string PersonJson = "{\"name\":\"John\",\"age\":30}";

        private static MemoryStream StreamOf(string value)
            => new(Encoding.UTF8.GetBytes(value));

        // --- utf8 span ---------------------------------------------------------

        [Fact]
        public void TryDeserialize_Utf8_ReturnsObject_ForValidJson()
        {
            // Arrange
            var utf8 = Encoding.UTF8.GetBytes(PersonJson);

            // Act
            var result = utf8.AsSpan().TryDeserialize<Person>(out var person);

            // Assert
            Assert.True(result);
            Assert.Equal("John", person!.Name);
            Assert.Equal(30, person.Age);
        }

        [Fact]
        public void TryDeserialize_Utf8_MatchesStringOverload()
        {
            // Arrange
            var utf8 = Encoding.UTF8.GetBytes(PersonJson);

            // Act
            var fromSpan = utf8.AsSpan().TryDeserialize<Person>(out var fromSpanResult);
            var fromString = PersonJson.TryDeserialize<Person>(out var fromStringResult);

            // Assert
            Assert.Equal(fromString, fromSpan);
            Assert.Equal(fromStringResult!.Name, fromSpanResult!.Name);
            Assert.Equal(fromStringResult.Age, fromSpanResult.Age);
        }

        [Fact]
        public void TryDeserialize_Utf8_HandlesSurroundingWhitespace()
        {
            // Arrange
            var utf8 = Encoding.UTF8.GetBytes("\r\n  " + PersonJson + "  \t");

            // Act & Assert
            Assert.True(utf8.AsSpan().TryDeserialize<Person>(out _));
        }

        [Fact]
        public void TryDeserialize_Utf8_ReturnsFalse_ForNonJson()
        {
            // Arrange
            var utf8 = Encoding.UTF8.GetBytes("not json");

            // Act
            var result = utf8.AsSpan().TryDeserialize<Person>(out var person);

            // Assert
            Assert.False(result);
            Assert.Null(person);
        }

        [Fact]
        public void TryDeserialize_Utf8_ReturnsFalse_ForEmpty()
        {
            // Act & Assert
            Assert.False(ReadOnlySpan<byte>.Empty.TryDeserialize<Person>(out _));
        }

        [Fact]
        public void TryDeserialize_Utf8_HandlesMultiByteCharacters()
        {
            // Arrange
            var utf8 = Encoding.UTF8.GetBytes("{\"name\":\"café\",\"age\":1}");

            // Act
            var result = utf8.AsSpan().TryDeserialize<Person>(out var person);

            // Assert
            Assert.True(result);
            Assert.Equal("café", person!.Name);
        }

        // --- stream ------------------------------------------------------------

        [Fact]
        public void TryDeserialize_Stream_ReturnsObject_ForValidJson()
        {
            // Arrange
            using var stream = StreamOf(PersonJson);

            // Act
            var result = stream.TryDeserialize<Person>(out var person);

            // Assert
            Assert.True(result);
            Assert.Equal("John", person!.Name);
        }

        [Fact]
        public void TryDeserialize_Stream_ReturnsFalse_ForNonJson()
        {
            // Arrange
            using var stream = StreamOf("not json");

            // Act
            var result = stream.TryDeserialize<Person>(out var person);

            // Assert
            Assert.False(result);
            Assert.Null(person);
        }

        [Fact]
        public async Task DeserializeJsonAsync_ReturnsObject_ForValidJson()
        {
            // Arrange
            using var stream = StreamOf(PersonJson);

            // Act
            var person = await stream.DeserializeJsonAsync<Person>();

            // Assert
            Assert.NotNull(person);
            Assert.Equal(30, person!.Age);
        }

        [Fact]
        public async Task DeserializeJsonAsync_ReturnsNull_ForNonJson()
        {
            // Arrange
            using var stream = StreamOf("not json");

            // Act
            var person = await stream.DeserializeJsonAsync<Person>();

            // Assert
            Assert.Null(person);
        }

        // --- serializing to a stream -------------------------------------------

        [Fact]
        public void TrySerializeToStream_WritesSameJsonAsStringOverload()
        {
            // Arrange
            var person = new Person { Name = "John", Age = 30 };
            using var stream = new MemoryStream();

            // Act
            var result = person.TrySerializeToStream(stream, indent: false);

            // Assert
            Assert.True(result);
            Assert.Equal(person.SerializeJsonString(false), Encoding.UTF8.GetString(stream.ToArray()));
        }

        [Fact]
        public void TrySerializeToStream_HonoursIndentSetting()
        {
            // Arrange
            var person = new Person { Name = "John", Age = 30 };
            using var indented = new MemoryStream();
            using var flat = new MemoryStream();

            // Act
            person.TrySerializeToStream(indented, indent: true);
            person.TrySerializeToStream(flat, indent: false);

            // Assert
            Assert.True(indented.Length > flat.Length);
        }

        [Fact]
        public void TrySerializeToStream_ReturnsFalse_ForNullValue()
        {
            // Arrange
            object? value = null;
            using var stream = new MemoryStream();

            // Act & Assert
            Assert.False(value.TrySerializeToStream(stream));
        }

        [Fact]
        public async Task TrySerializeToStreamAsync_WritesSameJsonAsSyncOverload()
        {
            // Arrange
            var person = new Person { Name = "John", Age = 30 };
            using var async = new MemoryStream();
            using var sync = new MemoryStream();

            // Act
            var result = await person.TrySerializeToStreamAsync(async, indent: false);
            person.TrySerializeToStream(sync, indent: false);

            // Assert
            Assert.True(result);
            Assert.Equal(sync.ToArray(), async.ToArray());
        }

        // --- round trip ---------------------------------------------------------

        [Fact]
        public void SerializeToStream_And_DeserializeStream_RoundTrip()
        {
            // Arrange
            var person = new Person { Name = "Jane", Age = 25 };
            using var stream = new MemoryStream();

            // Act
            Assert.True(person.TrySerializeToStream(stream));
            stream.Position = 0;
            var result = stream.TryDeserialize<Person>(out var roundTripped);

            // Assert
            Assert.True(result);
            Assert.Equal(person.Name, roundTripped!.Name);
            Assert.Equal(person.Age, roundTripped.Age);
        }
    }
}
