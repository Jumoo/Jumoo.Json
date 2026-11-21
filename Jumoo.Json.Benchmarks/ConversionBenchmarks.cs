using BenchmarkDotNet.Attributes;

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Jumoo.Json.Benchmarks;

/// <summary>
///  The object overloads of TryConvertToJsonNode / TryParseToJsonNode, with the value
///  types callers actually pass in: a string, an existing node, and a JsonElement.
/// </summary>
/// <remarks>
///  Without a fast path, a node or element goes through ToString() first - an indented
///  UTF-16 copy of the whole payload - before being parsed back.
/// </remarks>
[ShortRunJob]
[MemoryDiagnoser]
public class ConversionBenchmarks
{
    private object _string = null!;
    private object _node = null!;
    private object _element = null!;
    private JsonDocument _document = null!;

    [GlobalSetup]
    public void Setup()
    {
        _string = JsonFixtures.LargePlain;
        _node = JsonFixtures.LargePlain.ToJsonNode()!;
        _document = JsonDocument.Parse(JsonFixtures.LargePlain);
        _element = _document.RootElement;
    }

    [GlobalCleanup]
    public void Cleanup() => _document.Dispose();

    [Benchmark(Baseline = true)]
    public JsonNode? TryConvertToJsonNode_FromString_Large()
        => _string.TryConvertToJsonNode(out var node) ? node : null;

    [Benchmark]
    public JsonNode? TryConvertToJsonNode_FromJsonNode_Large()
        => _node.TryConvertToJsonNode(out var node) ? node : null;

    [Benchmark]
    public JsonNode? TryConvertToJsonNode_FromJsonElement_Large()
        => _element.TryConvertToJsonNode(out var node) ? node : null;

    [Benchmark]
    public JsonNode? TryParseToJsonNode_FromJsonNode_Large()
        => _node.TryParseToJsonNode(out var node) ? node : null;
}
