#define DEBUG_TEXT_PARTS


#if DEBUG_TEXT_PARTS
using System.Diagnostics;
#endif

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

    public string Parameters { get; set; } = string.Empty;

    public bool IsSection => Type == TextPartType.Section;

    public bool IsScriptSection => Type == TextPartType.ScriptSection;

    public bool IsScript => IsScriptStatement || IsScriptBlock || IsIfStatement || IsElseIfStatement;

    public bool IsScriptStatement => Type == TextPartType.ScriptStatement;

    public bool IsScriptBlock => Type == TextPartType.ScriptBlock;

    public bool IsIfStatement => Type == TextPartType.IfStatement;

    public bool IsElseIfStatement => Type == TextPartType.ElseIfStatement;

    public bool IsIfBlock => Type == TextPartType.IfBlock;

    public bool IsElseIfBlock => Type == TextPartType.ElseIfBlock;

    public bool IsElseBlock => Type == TextPartType.ElseBlock;

    public bool IsIfElseElseIfBlock => IsIfBlock || IsElseBlock || IsElseIfBlock;

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
            TextPartType.ElseIfStatement =>
                $"return !!{text.Substring(Start, Length)}",
            _ => text.Substring(Start, Length)
        };
    }
}
