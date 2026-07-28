using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jumoo.Json.Tests
{
    public class JsonTextOptionsTests
    {
        [Fact]
        public void GetOptions_Indented_ReturnsOptionsWithWriteIndentedTrue()
        {
            var options = JsonTextOptions.GetOptions(indent: true);

            Assert.NotNull(options);
            Assert.True(options.WriteIndented);
        }

        [Fact]
        public void GetOptions_Flat_ReturnsOptionsWithWriteIndentedFalse()
        {
            var options = JsonTextOptions.GetOptions(indent: false);

            Assert.NotNull(options);
            Assert.False(options.WriteIndented);
        }

        [Fact]
        public void GetOptions_DefaultParameter_ReturnsIndentedOptions()
        {
            var options = JsonTextOptions.GetOptions();

            Assert.True(options.WriteIndented);
        }

        [Fact]
        public void GetNodeOptions_ReturnsCaseInsensitiveOptions()
        {
            var nodeOptions = JsonTextOptions.GetNodeOptions();

            Assert.True(nodeOptions.PropertyNameCaseInsensitive);
        }

        [Fact]
        public void GetOptions_ContainsExpectedBuiltInConverters()
        {
            var options = JsonTextOptions.GetOptions();

            var converterTypes = options.Converters.Select(c => c.GetType()).ToList();
            Assert.Contains(typeof(JsonStringEnumConverter), converterTypes);
        }

        // --- AddConverter / RemoveConverter -------------------------------------
        //
        // These mutate global state, so each one removes what it added. The converter
        // deliberately targets a type only these tests use - registering one for a
        // common type like string would change how every other test serializes.

        /// <summary>
        ///  The regression test. JsonSerializerOptions makes itself read-only the first
        ///  time it is used, so the old in-place Converters.Add threw here.
        /// </summary>
        [Fact]
        public void AddConverter_Succeeds_AfterOptionsHaveAlreadyBeenUsed()
        {
            // Arrange - force a serialize so the shared options lock themselves
            _ = new Marker { Value = "warm up" }.SerializeJsonString(false);
            Assert.True(JsonTextOptions.GetOptions().IsReadOnly);

            var converter = new MarkerConverter();

            try
            {
                // Act
                JsonTextOptions.AddConverter(converter);

                // Assert
                Assert.Contains(converter, JsonTextOptions.GetOptions().Converters);
            }
            finally
            {
                JsonTextOptions.RemoveConverter(converter);
            }
        }

        [Fact]
        public void AddConverter_AppliesToBothIndentedAndFlatOptions()
        {
            var converter = new MarkerConverter();

            try
            {
                JsonTextOptions.AddConverter(converter);

                Assert.Contains(converter, JsonTextOptions.GetOptions(indent: true).Converters);
                Assert.Contains(converter, JsonTextOptions.GetOptions(indent: false).Converters);
            }
            finally
            {
                JsonTextOptions.RemoveConverter(converter);
            }
        }

        [Fact]
        public void AddConverter_IsUsedWhenSerializing()
        {
            var converter = new MarkerConverter();
            var value = new Marker { Value = "hello" };

            try
            {
                JsonTextOptions.AddConverter(converter);

                Assert.Equal("\"marker:hello\"", value.SerializeJsonString(false));
            }
            finally
            {
                JsonTextOptions.RemoveConverter(converter);
            }

            // and once removed, back to normal object serialization
            Assert.Equal("{\"value\":\"hello\"}", value.SerializeJsonString(false));
        }

        [Fact]
        public void AddConverter_DoesNotAddTheSameConverterTwice()
        {
            var converter = new MarkerConverter();

            try
            {
                JsonTextOptions.AddConverter(converter);
                JsonTextOptions.AddConverter(converter);

                Assert.Equal(1, JsonTextOptions.GetOptions().Converters.Count(c => ReferenceEquals(c, converter)));
            }
            finally
            {
                JsonTextOptions.RemoveConverter(converter);
            }
        }

        [Fact]
        public void AddConverter_PreservesExistingConfiguration()
        {
            var before = JsonTextOptions.GetOptions();
            var builtInCount = before.Converters.Count;
            var converter = new MarkerConverter();

            try
            {
                JsonTextOptions.AddConverter(converter);
                var after = JsonTextOptions.GetOptions();

                Assert.True(after.WriteIndented);
                Assert.True(after.PropertyNameCaseInsensitive);
                Assert.Equal(before.PropertyNamingPolicy, after.PropertyNamingPolicy);
                Assert.Equal(before.NumberHandling, after.NumberHandling);
                Assert.Equal(before.MaxDepth, after.MaxDepth);
                Assert.Same(before.TypeInfoResolver, after.TypeInfoResolver);
                Assert.Equal(builtInCount + 1, after.Converters.Count);
                Assert.False(JsonTextOptions.GetOptions(indent: false).WriteIndented);
            }
            finally
            {
                JsonTextOptions.RemoveConverter(converter);
            }
        }

        /// <summary>
        ///  Property ordering is the reason the resolver exists, so make sure rebuilding
        ///  the options doesn't quietly drop it.
        /// </summary>
        [Fact]
        public void AddConverter_KeepsPropertiesOrdered()
        {
            var converter = new MarkerConverter();

            try
            {
                JsonTextOptions.AddConverter(converter);

                var json = new { Zebra = 1, Apple = 2 }.SerializeJsonString(false);

                Assert.Equal("{\"apple\":2,\"zebra\":1}", json);
            }
            finally
            {
                JsonTextOptions.RemoveConverter(converter);
            }
        }

        [Fact]
        public void RemoveConverter_RemovesFromBothOptions()
        {
            var converter = new MarkerConverter();

            JsonTextOptions.AddConverter(converter);
            JsonTextOptions.RemoveConverter(converter);

            Assert.DoesNotContain(converter, JsonTextOptions.GetOptions(indent: true).Converters);
            Assert.DoesNotContain(converter, JsonTextOptions.GetOptions(indent: false).Converters);
        }

        [Fact]
        public void RemoveConverter_IsNoOp_ForAConverterThatWasNeverAdded()
        {
            var before = JsonTextOptions.GetOptions();

            JsonTextOptions.RemoveConverter(new MarkerConverter());

            // untouched, so the same instance is still in play
            Assert.Same(before, JsonTextOptions.GetOptions());
        }

        [Fact]
        public void AddConverter_ThrowsForNull()
            => Assert.Throws<ArgumentNullException>(() => JsonTextOptions.AddConverter(null!));

        [Fact]
        public void RemoveConverter_ThrowsForNull()
            => Assert.Throws<ArgumentNullException>(() => JsonTextOptions.RemoveConverter(null!));

        private sealed class Marker
        {
            public string Value { get; set; } = string.Empty;
        }

        private sealed class MarkerConverter : JsonConverter<Marker>
        {
            public override Marker? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => new() { Value = reader.GetString() ?? string.Empty };

            public override void Write(Utf8JsonWriter writer, Marker value, JsonSerializerOptions options)
                => writer.WriteStringValue($"marker:{value.Value}");
        }
    }
}
