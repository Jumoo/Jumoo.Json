using BenchmarkDotNet.Attributes;

using System.Text.Json.Nodes;

namespace Jumoo.Json.Benchmarks;

/// <summary>
///  Expansion walks the whole node graph, so it is sensitive to both the
///  unconditional DeepClone and the per-node allocations inside ExpandNode.
/// </summary>
/// <remarks>
///  The Plain / Embedded split matters: "Plain" is the majority case where there is
///  nothing to expand and all the work is wasted.
/// </remarks>
[ShortRunJob]
[MemoryDiagnoser]
public class ExpansionBenchmarks
{
    private JsonNode _smallPlain = null!;
    private JsonNode _smallEmbedded = null!;
    private JsonNode _largePlain = null!;
    private JsonNode _largeEmbedded = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallPlain = JsonFixtures.SmallPlain.ToJsonNode()!;
        _smallEmbedded = JsonFixtures.SmallEmbedded.ToJsonNode()!;
        _largePlain = JsonFixtures.LargePlain.ToJsonNode()!;
        _largeEmbedded = JsonFixtures.LargeEmbedded.ToJsonNode()!;
    }

    [Benchmark]
    public JsonNode Expand_Small_Plain() => _smallPlain.ExpandAllJsonInToken();

    [Benchmark]
    public JsonNode Expand_Small_Embedded() => _smallEmbedded.ExpandAllJsonInToken();

    [Benchmark]
    public JsonNode Expand_Large_Plain() => _largePlain.ExpandAllJsonInToken();

    [Benchmark]
    public JsonNode Expand_Large_Embedded() => _largeEmbedded.ExpandAllJsonInToken();

    [Benchmark]
    public string ConvertStringToExpandedJsonString_Large_Embedded()
        => JsonFixtures.LargeEmbedded.ConvertStringToExpandedJsonString();

    [Benchmark]
    public string GetEscapedJsonValue_Large()
        => JsonFixtures.LargePlain.GetEscapedJsonValue();
}
