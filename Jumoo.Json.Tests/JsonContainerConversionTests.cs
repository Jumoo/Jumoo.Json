using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jumoo.Json.Tests;

/// <summary>
///  The object overloads take a fast path for JsonObject / JsonArray / JsonElement values. These
///  check the results are the same as the string route they replace, plus expansion of a parsed
///  string matching expansion of the node.
/// </summary>
public class JsonContainerConversionTests
{
    private static JsonObject BuildDocument()
    {
        var inner = new JsonObject
        {
            ["title"] = "inner \"quoted\" title",
            ["path"] = "C:\\temp\\file.txt",
            ["unicode"] = "caf\u00e9 \u4e2d <b>",
            ["count"] = 3,
            ["price"] = 12.5m,
            ["flag"] = true,
            ["nothing"] = null,
            ["deeper"] = new JsonObject { ["a"] = "[1,2,3]", ["b"] = "{ broken json" }.ToJsonString(),
            ["list"] = new JsonArray("x", "{\"y\":1}", 2, null, new JsonArray("[\"z\"]")),
        };

        return new JsonObject
        {
            ["alias"] = "home",
            ["value"] = inner.ToJsonString(),
            ["values"] = new JsonArray(inner.ToJsonString(), "plain", 12.5),
            ["object"] = new JsonObject { ["nested"] = inner.ToJsonString(), ["empty"] = "" },
            ["emptyArray"] = new JsonArray(),
            ["emptyObject"] = new JsonObject(),
        };
    }

    // what the object overloads did before the fast path: go via the value's string form.
    private static JsonNode? ViaString(object value)
        => value.ToString()!.TryConvertToJsonNode(out var node) ? node : null;

    [Fact]
    public void TryConvertToJsonNode_FromJsonObject_MatchesStringRoute()
    {
        object source = BuildDocument();

        Assert.True(source.TryConvertToJsonNode(out var actual));
        Assert.Equal(ViaString(source)!.ToJsonString(), actual.ToJsonString());
    }

    [Fact]
    public void TryConvertToJsonObject_FromJsonObject_ReturnsNewParentlessNode()
    {
        var source = BuildDocument();
        var parent = new JsonObject { ["child"] = source };

        Assert.True(((object)source).TryConvertToJsonObject(out var actual));

        Assert.NotSame(source, actual);
        Assert.Null(actual.Parent);
        Assert.Same(parent, source.Parent);

        // the result can be added to another tree, which a node with a parent can't.
        var other = new JsonObject { ["copy"] = actual };
        Assert.Same(other, actual.Parent);
    }

    [Fact]
    public void TryConvertToJsonObject_FromJsonObject_UsesLibraryNodeOptions()
    {
        // built without options, so case sensitive - the parsed result ignores case.
        object source = new JsonObject { ["Alias"] = "home" };

        Assert.True(source.TryConvertToJsonObject(out var actual));
        Assert.Equal("home", actual["ALIAS"]?.ToString());
    }

    [Fact]
    public void TryConvertToJsonObject_FromJsonObject_DoesNotChangeSource()
    {
        var source = BuildDocument();
        var before = source.ToJsonString();

        Assert.True(((object)source).TryConvertToJsonObject(out var actual));
        actual["alias"] = "changed";

        Assert.Equal(before, source.ToJsonString());
    }

    [Fact]
    public void TryConvertToJsonObject_FromJsonArray_IsFalse()
    {
        object source = new JsonArray(1, 2);
        Assert.False(source.TryConvertToJsonObject(out _));
    }

    [Fact]
    public void TryParseToJsonNode_FromJsonArray_MatchesStringRoute()
    {
        object source = new JsonArray(new JsonObject { ["a"] = 1 }, "b", null);

        Assert.True(source.TryParseToJsonNode(out var actual));
        Assert.IsType<JsonArray>(actual);
        Assert.Equal(ViaString(source)!.ToJsonString(), actual.ToJsonString());
    }

    [Fact]
    public void TryConvertToJsonObject_FromJsonElement_MatchesStringRoute()
    {
        using var document = JsonDocument.Parse(BuildDocument().ToJsonString());
        object element = document.RootElement;

        Assert.True(element.TryConvertToJsonObject(out var actual));
        Assert.Equal(ViaString(element)!.ToJsonString(), actual.ToJsonString());
        Assert.Equal("home", actual["ALIAS"]?.ToString());
    }

    [Fact]
    public void TryConvertToJsonObject_FromJsonElement_OutlivesTheDocument()
    {
        object element;
        using (var document = JsonDocument.Parse("{\"alias\":\"home\",\"list\":[1,2]}"))
        {
            element = document.RootElement;
            Assert.True(element.TryConvertToJsonObject(out var actual));

            document.Dispose();
            Assert.Equal("{\"alias\":\"home\",\"list\":[1,2]}", actual.ToJsonString());
        }
    }

    [Fact]
    public void TryParseToJsonNode_FromStringJsonElement_StillParsesTheString()
    {
        // a JsonElement holding a string of json goes through the string route, as before.
        using var document = JsonDocument.Parse("\"{\\\"a\\\":1}\"");
        object element = document.RootElement;

        Assert.True(element.TryParseToJsonNode(out var actual));
        Assert.Equal("{\"a\":1}", actual.ToJsonString());
    }

    [Fact]
    public void TryParseToJsonNode_FromNumberJsonElement_IsFalse()
    {
        using var document = JsonDocument.Parse("42");
        object element = document.RootElement;

        Assert.False(element.TryParseToJsonNode(out _));
    }

    [Fact]
    public void TryConvertToJsonNode_FromDisposedJsonElement_IsFalse()
    {
        var document = JsonDocument.Parse("{\"a\":1}");
        object element = document.RootElement;
        document.Dispose();

        Assert.False(element.TryConvertToJsonNode(out _));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConvertStringToExpandedJsonString_MatchesExpandAllJsonInToken(bool indented)
    {
        var json = BuildDocument().ToJsonString();

        var expected = json.ToJsonNode()!.ExpandAllJsonInToken().SerializeJsonNode(indented);

        Assert.Equal(expected, json.ConvertStringToExpandedJsonString(indented));
    }

    [Fact]
    public void ConvertStringToExpandedJson_ExpandsNestedStrings()
    {
        var expanded = BuildDocument().ToJsonString().ConvertStringToExpandedJson();

        Assert.NotNull(expanded);
        Assert.IsType<JsonObject>(expanded["value"]);
        Assert.IsType<JsonObject>(expanded["object"]!["nested"]);
        Assert.IsType<JsonObject>(expanded["values"]![0]);
        Assert.Equal("plain", expanded["values"]![1]!.ToString());
    }

    [Fact]
    public void ConvertStringToExpandedJson_NothingToExpand_ReturnsParsedJson()
    {
        const string json = "{\"a\":\"plain\",\"b\":[1,2,{\"c\":true}]}";

        var expanded = json.ConvertStringToExpandedJson();

        Assert.NotNull(expanded);
        Assert.Equal(json, expanded.ToJsonString());
    }
}
