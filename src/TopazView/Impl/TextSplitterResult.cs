#undef DEBUG_TEXT_PARTS


namespace Tenray.TopazView.Impl;

internal sealed class TextSplitterResult
{
    public string Text { get; set; }

    public string Layout { get; set; }

    public List<TextPart> TextParts { get; set; }

#if DEBUG_TEXT_PARTS
    public string GetContent(bool markBlocks)
    {
        var sb = new StringBuilder();
        if (Layout != null)
        {
            sb.Append("@Layout = '");
            sb.Append(Layout);
            sb.AppendLine("'");
        }
        foreach (var part in TextParts)
        {
            var type = part.Type.ToString();
            if (markBlocks)
                sb.AppendLine($"<!-- {type} -->");
            sb.AppendLine(Text.Substring(part.Start, part.Length));
            if (markBlocks)
                sb.AppendLine($"<!-- {type} -->");
        }
        return sb.ToString();
    }
#endif
}
