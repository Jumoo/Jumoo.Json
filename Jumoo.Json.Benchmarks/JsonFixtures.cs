using System.Text;

namespace Jumoo.Json.Benchmarks;

/// <summary>
///  Deterministic, Umbraco-shaped JSON payloads used across the benchmarks.
/// </summary>
/// <remarks>
///  Four axes matter for the paths we are measuring:
///  <list type="bullet">
///   <item>size - small (~1kb) vs large (~200kb)</item>
///   <item>embedded json - whether string values contain escaped json that expansion will unpack</item>
///  </list>
///  Everything is generated from a fixed seed so runs are comparable.
/// </remarks>
internal static class JsonFixtures
{
    /// <summary>~1kb, no embedded json - the "nothing to expand" case.</summary>
    public static readonly string SmallPlain = BuildDocument(itemCount: 2, embedJson: false);

    /// <summary>~1kb, string values containing escaped json.</summary>
    public static readonly string SmallEmbedded = BuildDocument(itemCount: 2, embedJson: true);

    /// <summary>~200kb, no embedded json.</summary>
    public static readonly string LargePlain = BuildDocument(itemCount: 320, embedJson: false);

    /// <summary>~200kb, string values containing escaped json.</summary>
    public static readonly string LargeEmbedded = BuildDocument(itemCount: 260, embedJson: true);

    /// <summary>
    ///  <see cref="LargePlain"/> with surrounding whitespace, which is what makes
    ///  <c>DetectIsJson</c>'s <c>Trim()</c> allocate a full copy of the payload.
    /// </summary>
    public static readonly string LargePlainPadded = "\r\n  " + LargePlain + "  \r\n";

    /// <summary>A plain, non-json string - the fallback path in TryConvertToJsonNode.</summary>
    public const string PlainString = "a perfectly ordinary property value";

    /// <summary>A non-json string containing characters that need escaping.</summary>
    public const string PlainStringWithQuotes = @"he said ""hello"" and left a \ behind";

    private static string BuildDocument(int itemCount, bool embedJson)
    {
        var random = new Random(20260727);
        var builder = new StringBuilder(itemCount * 800);

        builder.Append('[');

        for (var i = 0; i < itemCount; i++)
        {
            if (i > 0) builder.Append(',');
            AppendItem(builder, random, i, embedJson);
        }

        builder.Append(']');
        return builder.ToString();
    }

    private static void AppendItem(StringBuilder builder, Random random, int index, bool embedJson)
    {
        builder.Append("{\"key\":\"").Append(DeterministicGuid(random)).Append('"')
            .Append(",\"alias\":\"contentItem").Append(index).Append('"')
            .Append(",\"name\":\"Content Item ").Append(index).Append('"')
            .Append(",\"level\":").Append(random.Next(1, 6))
            .Append(",\"sortOrder\":").Append(index)
            .Append(",\"published\":").Append(random.Next(2) == 0 ? "true" : "false")
            .Append(",\"properties\":{");

        for (var p = 0; p < 6; p++)
        {
            if (p > 0) builder.Append(',');
            builder.Append("\"property").Append(p).Append("\":\"");

            // every third property carries escaped json when we want the embedded variant,
            // so expansion has real work to do but most values are still plain text.
            if (embedJson && p % 3 == 0)
                builder.Append(EscapedNestedJson(random, p));
            else
                builder.Append(Sentence(random));

            builder.Append('"');
        }

        builder.Append("}}");
    }

    private static string EscapedNestedJson(Random random, int seed)
        => "{\\\"udi\\\":\\\"umb://document/" + DeterministicGuid(random).Replace("-", string.Empty)
            + "\\\",\\\"culture\\\":\\\"en-us\\\",\\\"segment\\\":null,\\\"value\\\":\\\""
            + Sentence(random) + "\\\",\\\"index\\\":" + seed + "}";

    private static string Sentence(Random random)
    {
        string[] words =
        [
            "content", "document", "property", "editor", "value", "member", "media",
            "template", "culture", "variant", "published", "schedule", "workflow"
        ];

        var builder = new StringBuilder(64);
        var count = random.Next(6, 12);

        for (var i = 0; i < count; i++)
        {
            if (i > 0) builder.Append(' ');
            builder.Append(words[random.Next(words.Length)]);
        }

        return builder.ToString();
    }

    private static string DeterministicGuid(Random random)
    {
        var bytes = new byte[16];
        random.NextBytes(bytes);
        return new Guid(bytes).ToString();
    }
}

/// <summary>
///  Deserialisation target matching the generated fixtures.
/// </summary>
public sealed class ContentItem
{
    public Guid Key { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public bool Published { get; set; }
    public Dictionary<string, string> Properties { get; set; } = [];
}
