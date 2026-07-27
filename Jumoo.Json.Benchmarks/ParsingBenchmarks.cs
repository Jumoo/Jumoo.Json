using BenchmarkDotNet.Attributes;

using System.Text.Json.Nodes;

namespace Jumoo.Json.Benchmarks;

/// <summary>
///  Parsing and validation entry points - everything here passes through the
///  DetectIsJson gate before touching the parser.
/// </summary>
[ShortRunJob]
[MemoryDiagnoser]
public class ParsingBenchmarks
{
    [Benchmark]
    public JsonNode? TryParseToJsonNode_Small()
        => JsonFixtures.SmallPlain.TryParseToJsonNode(out var node) ? node : null;

    [Benchmark]
    public JsonNode? TryParseToJsonNode_Large()
        => JsonFixtures.LargePlain.TryParseToJsonNode(out var node) ? node : null;

    /// <summary>
    ///  Padded input is the case where DetectIsJson's Trim() copies the whole payload.
    /// </summary>
    [Benchmark]
    public JsonNode? TryParseToJsonNode_Large_Padded()
        => JsonFixtures.LargePlainPadded.TryParseToJsonNode(out var node) ? node : null;

    /// <summary>
    ///  The rejection path - never reaches the parser, so this is pure gate cost.
    /// </summary>
    [Benchmark]
    public bool TryParseToJsonNode_NonJson()
        => JsonFixtures.PlainString.TryParseToJsonNode(out _);

    [Benchmark]
    public bool IsValidJsonString_Small()
        => JsonFixtures.SmallPlain.IsValidJsonString();

    [Benchmark]
    public bool IsValidJsonString_Large()
        => JsonFixtures.LargePlain.IsValidJsonString();

    [Benchmark]
    public JsonArray ToJsonArray_Large()
        => JsonFixtures.LargePlain.ToJsonArray();

    [Benchmark]
    public bool TryConvertToJsonNode_PlainString()
        => JsonFixtures.PlainString.TryConvertToJsonNode(out _);

    [Benchmark]
    public bool TryConvertToJsonNode_Json()
        => JsonFixtures.SmallPlain.TryConvertToJsonNode(out _);
}
