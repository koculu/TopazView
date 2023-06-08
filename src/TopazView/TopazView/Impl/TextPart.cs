#undef DEBUG_TEXT_PARTS


namespace Tenray.TopazView.Impl;

#if DEBUG_TEXT_PARTS
[DebuggerDisplay("{Text}")]
#endif
internal sealed class TextPart
{
#if DEBUG_TEXT_PARTS
    string Source;

    public string Text => Source.Substring(Start, Length);
#endif

    public int Start;

    public int Length;

    public TextPartType Type;

    public string SectionName;

    public string FunctionName { get; set; }

    public bool IsSection => Type == TextPartType.Section;

    public bool IsScriptSection => Type == TextPartType.ScriptSection;

    public bool IsScript => IsScriptStatement || IsScriptBlock || IsIfStatement;

    public bool IsScriptStatement => Type == TextPartType.ScriptStatement;

    public bool IsScriptBlock => Type == TextPartType.ScriptBlock;

    public bool IsIfStatement => Type == TextPartType.IfStatement;

    public bool IsIfBlock => Type == TextPartType.IfBlock;

    public bool IsElseBlock => Type == TextPartType.ElseBlock;

    public bool IsIfElseBlock => IsIfBlock || IsElseBlock;

    public ICompiledViewInternal CompiledView { get; set; }

#if DEBUG_TEXT_PARTS
    public TextPart(string source)
    {
        Source = source;
    }
#endif

    public string GetBody(string text)
    {
        return Type switch
        {
            TextPartType.ScriptStatement =>
                $"return {text.Substring(Start, Length)}",
            TextPartType.IfStatement =>
                $"return !!{text.Substring(Start + 3, Length - 3)}",
            _ => text.Substring(Start, Length)
        };
    }
}
