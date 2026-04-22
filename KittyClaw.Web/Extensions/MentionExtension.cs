using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using System.Globalization;

namespace KittyClaw.Web.Extensions;

/// <summary>
/// Markdig extension that transforms @username into a styled mention span.
/// Slugged names like @game-designer are displayed as "Game Designer".
/// </summary>
public class MentionExtension(HashSet<string> knownMembers) : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline) =>
        pipeline.InlineParsers.Insert(0, new MentionParser(knownMembers));

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}

public partial class MentionParser : InlineParser
{
    private readonly HashSet<string> _knownMembers;

    public MentionParser(HashSet<string> knownMembers)
    {
        OpeningCharacters = ['@'];
        _knownMembers = knownMembers;
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        // Don't match in the middle of a word
        var prev = slice.PeekCharExtra(-1);
        if (char.IsLetterOrDigit(prev) || prev == '_') return false;

        slice.NextChar(); // skip '@'

        var start = slice.Start;
        while (slice.CurrentChar is not ('\0' or ' ' or '\n' or '\r' or '\t' or ',' or '.' or ';' or ':' or '!' or '?' or ')' or ']' or '}' or '>' or '"' or '\''))
            slice.NextChar();

        var len = slice.Start - start;
        if (len == 0) return false;

        var handle = slice.Text.Substring(start, len);
        var displayName = Humanize(handle);
        var escapedHandle = System.Net.WebUtility.HtmlEncode(handle);
        var escapedDisplay = System.Net.WebUtility.HtmlEncode(displayName);

        var cssClass = _knownMembers.Contains(handle) ? "mention" : "mention mention-unknown";

        processor.Inline = new HtmlInline(
            $"<span class=\"{cssClass}\" title=\"@{escapedHandle}\">@{escapedDisplay}</span>");
        return true;
    }

    /// <summary>
    /// Converts a slugged handle like "game-designer" into "Game Designer".
    /// Simple names like "owner" become "Owner".
    /// </summary>
    private static string Humanize(string handle) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(handle.Replace('-', ' ').Replace('_', ' '));
}
