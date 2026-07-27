using System.Text.Json.Nodes;

namespace Jumoo.Json.Tests
{
    /// <summary>
    ///  Behaviour that has to hold while the internals get optimised.
    /// </summary>
    /// <remarks>
    ///  Each of these covers an edge case that the faster implementations could
    ///  plausibly get wrong (or, in the escaping cases, that the original
    ///  implementation got wrong).
    /// </remarks>
    public class JsonPerformanceRegressionTests
    {
        // --- TryConvertToJsonNode: escaping ------------------------------------

        [Theory]
        [InlineData(@"he said ""hello""")]
        [InlineData(@"a backslash \ and a quote """)]
        [InlineData("a tab\tand a newline\n")]
        [InlineData(@"c:\temp\file.json")]
        public void TryConvertToJsonNode_HandlesStringsNeedingEscaping(string value)
        {
            // Act
            var result = value.TryConvertToJsonNode(out var node);

            // Assert
            Assert.True(result);
            Assert.NotNull(node);
            Assert.Equal(value, node!.GetValue<string>());
        }

        [Fact]
        public void TryConvertToJsonNode_RoundTripsThroughSerialization()
        {
            // Arrange
            var value = @"he said ""hello"" \ goodbye";

            // Act
            Assert.True(value.TryConvertToJsonNode(out var node));
            var json = node!.SerializeJsonNode(false);

            // Assert
            Assert.NotNull(json);
            Assert.Equal(value, JsonNode.Parse(json!)!.GetValue<string>());
        }

        [Fact]
        public void TryConvertToJsonNode_ReturnsTrue_ForEmptyString()
        {
            // Act
            var result = string.Empty.TryConvertToJsonNode(out var node);

            // Assert - an empty string is still a legitimate json string value
            Assert.True(result);
            Assert.Equal(string.Empty, node!.GetValue<string>());
        }

        // --- DetectIsJson replacement -----------------------------------------

        [Theory]
        [InlineData("  {\"key\":\"value\"}  ")]
        [InlineData("\r\n[1,2,3]\t")]
        public void TryParseToJsonNode_IgnoresSurroundingWhitespace(string value)
        {
            // Act
            var result = value.TryParseToJsonNode(out var node);

            // Assert
            Assert.True(result);
            Assert.NotNull(node);
        }

        [Theory]
        [InlineData("{")]
        [InlineData("[")]
        [InlineData("}")]
        [InlineData("not json at all")]
        [InlineData("{ unbalanced")]
        [InlineData("")]
        [InlineData("   ")]
        public void TryParseToJsonNode_ReturnsFalse_ForNonJson(string value)
        {
            // Act
            var result = value.TryParseToJsonNode(out var node);

            // Assert
            Assert.False(result);
            Assert.Null(node);
        }

        // --- GetPropertyValueOrDefault ----------------------------------------

        [Fact]
        public void GetPropertyValueOrDefault_ReturnsDefault_ForTypeMismatch()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"not a number\"}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyValueOrDefault("key", -1);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetPropertyValueOrDefault_ReturnsDefault_ForObjectValue()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":{\"nested\":1}}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyValueOrDefault("key", -1);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetPropertyValueOrDefault_ReturnsDefault_ForNullValue()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":null}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyValueOrDefault("key", -1);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetPropertyValueOrDefault_ReadsStringValue()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyValueOrDefault("key", "fallback");

            // Assert
            Assert.Equal("value", result);
        }

        [Fact]
        public void GetPropertyValueOrDefault_ReadsBoolValue()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":true}")!.AsObject();

            // Act
            var result = jsonObject.GetPropertyValueOrDefault("key", false);

            // Assert
            Assert.True(result);
        }

        // --- IsJsonEqual -------------------------------------------------------

        [Fact]
        public void IsJsonEqual_ReturnsTrue_ForSameReference()
        {
            // Arrange
            var obj = new { Name = "John", Age = 30 };

            // Act
            var result = obj.IsJsonEqual(obj);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsJsonEqual_ReturnsFalse_WhenFirstObjectIsNull()
        {
            // Arrange
            object? first = null;
            var second = new { Name = "John" };

            // Act
            var result = first.IsJsonEqual(second);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsJsonEqual_HandlesNonAsciiValues()
        {
            // Arrange - utf8 byte comparison must not confuse different multi-byte content
            var first = new { Name = "café" };
            var second = new { Name = "cafe" };

            // Act & Assert
            Assert.False(first.IsJsonEqual(second));
            Assert.True(first.IsJsonEqual(new { Name = "café" }));
        }

        // --- Expansion ---------------------------------------------------------

        [Fact]
        public void ExpandAllJsonInToken_LeavesDocumentUnchanged_WhenNothingToExpand()
        {
            // Arrange
            var json = "{\"a\":\"plain\",\"b\":[1,2,3],\"c\":{\"d\":\"also plain\"},\"e\":null}";
            var node = JsonNode.Parse(json)!;

            // Act
            var expanded = node.ExpandAllJsonInToken();

            // Assert
            Assert.Equal(node.ToJsonString(), expanded.ToJsonString());
        }

        [Fact]
        public void ExpandAllJsonInToken_DoesNotMutateTheSourceNode()
        {
            // Arrange
            var json = "{\"key\":\"{\\\"nested\\\":\\\"value\\\"}\"}";
            var node = JsonNode.Parse(json)!;
            var before = node.ToJsonString();

            // Act
            _ = node.ExpandAllJsonInToken();

            // Assert
            Assert.Equal(before, node.ToJsonString());
        }

        [Fact]
        public void ExpandAllJsonInToken_ExpandsInsideArrays()
        {
            // Arrange
            var json = "[\"{\\\"nested\\\":\\\"value\\\"}\",\"plain\"]";
            var node = JsonNode.Parse(json)!;

            // Act
            var expanded = node.ExpandAllJsonInToken().AsArray();

            // Assert
            Assert.True(expanded[0] is JsonObject);
            Assert.Equal("value", expanded[0]!["nested"]!.GetValue<string>());
            Assert.Equal("plain", expanded[1]!.GetValue<string>());
        }

        [Fact]
        public void ExpandAllJsonInToken_PreservesStringsNeedingEscaping()
        {
            // Arrange
            var node = new JsonObject { ["key"] = @"he said ""hello""" };

            // Act
            var expanded = node.ExpandAllJsonInToken();

            // Assert
            Assert.Equal(@"he said ""hello""", expanded["key"]!.GetValue<string>());
        }

        // --- Property accessors ------------------------------------------------

        [Fact]
        public void GetPropertyAsBool_ReadsRealBooleans()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"yes\":true,\"no\":false}")!.AsObject();

            // Act & Assert
            Assert.True(jsonObject.GetPropertyAsBool("yes"));
            Assert.False(jsonObject.GetPropertyAsBool("no"));
        }

        [Fact]
        public void GetPropertyAsBool_ReadsBooleanStrings()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"yes\":\"true\",\"no\":\"false\"}")!.AsObject();

            // Act & Assert
            Assert.True(jsonObject.GetPropertyAsBool("yes"));
            Assert.False(jsonObject.GetPropertyAsBool("no"));
        }

        [Fact]
        public void GetPropertyAsBool_ReturnsFalse_ForNonBooleanValues()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"a\":42,\"b\":{\"c\":1},\"d\":null}")!.AsObject();

            // Act & Assert
            Assert.False(jsonObject.GetPropertyAsBool("a"));
            Assert.False(jsonObject.GetPropertyAsBool("b"));
            Assert.False(jsonObject.GetPropertyAsBool("d"));
            Assert.False(jsonObject.GetPropertyAsBool("missing"));
        }

        [Fact]
        public void GetPropertyAsString_ReturnsStringValueUnquoted()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"key\":\"value\"}")!.AsObject();

            // Act & Assert
            Assert.Equal("value", jsonObject.GetPropertyAsString("key"));
        }

        [Fact]
        public void GetPropertyAsString_ReturnsSerializedJson_ForNonStringValues()
        {
            // Arrange
            var jsonObject = JsonNode.Parse("{\"num\":42,\"obj\":{\"a\":1}}")!.AsObject();

            // Act & Assert
            Assert.Equal("42", jsonObject.GetPropertyAsString("num"));
            Assert.NotNull(jsonObject.GetPropertyAsString("obj"));
        }

        // --- Array helpers -----------------------------------------------------

        [Fact]
        public void AsListOfJsonObjects_SkipsNullEntries()
        {
            // Arrange
            var array = JsonNode.Parse("[{\"a\":1},null,{\"b\":2}]")!.AsArray();

            // Act
            var list = array.AsListOfJsonObjects();

            // Assert
            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0]["a"]!.GetValue<int>());
            Assert.Equal(2, list[1]["b"]!.GetValue<int>());
        }

        [Fact]
        public void AsListOfJsonObjects_ReturnsEmptyList_ForNullArray()
        {
            // Arrange
            JsonArray? array = null;

            // Act
            var list = array.AsListOfJsonObjects();

            // Assert
            Assert.Empty(list);
        }

        // --- Validation --------------------------------------------------------

        [Theory]
        [InlineData("{\"key\":\"value\"}", true)]
        [InlineData("[1,2,3]", true)]
        [InlineData("  {\"key\":1}  ", true)]
        [InlineData("{\"key\":}", false)]
        [InlineData("{unquoted:1}", false)]
        [InlineData("not json", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValidJsonString_MatchesParserBehaviour(string? value, bool expected)
        {
            // Act & Assert
            Assert.Equal(expected, value.IsValidJsonString());
        }

        // --- TryGetValueAs -----------------------------------------------------

        [Fact]
        public void TryGetValueAs_ReturnsExactTypeDirectly()
        {
            // Arrange
            var value = "a string";

            // Act
            var result = value.TryGetValueAs<string>(out var converted);

            // Assert
            Assert.True(result);
            Assert.Same(value, converted);
        }

        [Fact]
        public void TryGetValueAs_ConvertsBoxedValueTypes()
        {
            // Arrange
            object value = 42;

            // Act
            var result = value.TryGetValueAs<int>(out var converted);

            // Assert
            Assert.True(result);
            Assert.Equal(42, converted);
        }

        [Fact]
        public void TryGetValueAs_ConvertsNumericStrings()
        {
            // Arrange
            object value = "42";

            // Act
            var result = value.TryGetValueAs<int>(out var converted);

            // Assert
            Assert.True(result);
            Assert.Equal(42, converted);
        }
    }
}
