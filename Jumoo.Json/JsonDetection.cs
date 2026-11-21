using System.Diagnostics.CodeAnalysis;

namespace Jumoo.Json;

/// <summary>
///  Cheap pre-checks used to keep obvious non-json away from the parser.
/// </summary>
internal static class JsonDetection
{
    /// <summary>
    ///  Does the value have the shape of a json object or array?
    /// </summary>
    /// <remarks>
    /// <para>
    ///  This is the same shape check as Umbraco's <c>DetectIsJson</c>, but the trim happens
    ///  over a span. <c>DetectIsJson</c> calls <c>string.Trim()</c>, which copies the entire
    ///  payload - so a 200kb value allocated 400kb before we had even looked at it.
    /// </para>
    /// <para>
    ///  Like <c>DetectIsJson</c> this only looks at the first and last characters, so it is a
    ///  filter rather than a validity check - callers still have to handle a parse failure.
    /// </para>
    /// </remarks>
    public static bool LooksLikeJson([NotNullWhen(true)] this string? value)
    {
        var span = value.AsSpan().Trim();

        // length > 1 so a lone "{" doesn't match itself at both ends.
        return span.Length > 1
            && ((span[0] is '{' && span[^1] is '}') || (span[0] is '[' && span[^1] is ']'));
    }

    /// <summary>
    ///  <see cref="LooksLikeJson(string?)"/> for utf8 content.
    /// </summary>
    public static bool LooksLikeJson(this ReadOnlySpan<byte> utf8)
    {
        var span = utf8.Trim(" \t\r\n"u8);

        return span.Length > 1
            && ((span[0] is (byte)'{' && span[^1] is (byte)'}')
                || (span[0] is (byte)'[' && span[^1] is (byte)']'));
    }
}
