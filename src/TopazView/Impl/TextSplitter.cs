#define DEBUG_TEXT_PARTS

using System;
using System.Runtime.CompilerServices;
using Tenray.TopazView.Exceptions;

namespace Tenray.TopazView.Impl;

internal static class TextSplitter
{
    const char SplitChar = '@';

    const string LayoutStr = "@layout";

    const string SectionStr = "section ";

    const char OpenBracket = '{';

    const char CloseBracket = '}';

    const char OpenParenthesis = '(';

    const char CloseParenthesis = ')';

    const char OpenSquareBracket = '[';

    const char CloseSquareBracket = ']';

    const char DoubleQuote = '"';

    const char SingleQuote = '\'';

    const char EscapeChar = '\\';

    public static TextSplitterResult SplitText(string text)
    {
        var result = SplitInternal(text);
        return result;
    }

    public static string GetLayout(string textStr)
    {
        var len = textStr.Length;
        var i = 0;
        var text = textStr.AsSpan();
        SkipWhitespace(ref i, text, len);
        if (text[i..].StartsWith(LayoutStr, StringComparison.OrdinalIgnoreCase))
            return GetLayout(ref i, text, len);
        return null;
    }

    static TextSplitterResult SplitInternal(string textStr)
    {
        var len = textStr.Length;
        var list = new List<TextPart>();
        var result = new TextSplitterResult
        {
            Text = textStr,
            TextParts = list
        };
        var i = 0;
        var text = textStr.AsSpan();
        SkipWhitespace(ref i, text, len);
        if (text[i..].StartsWith(LayoutStr, StringComparison.OrdinalIgnoreCase))
        {
            result.Layout = GetLayout(ref i, text, len);
            SkipSingleNewLine(ref i, text, len);
        }
        else
        {
            i = 0;
        }
        var textPart = new TextPart
#if DEBUG_TEXT_PARTS
            (textStr)
#endif
        {
            Start = i,
            Length = len - i
        };

        var lastCharWasSplitChar = false;
        var expectElseBlock = false;
        for (; i < len; ++i)
        {
            if (lastCharWasSplitChar)
            {
                --i;
                lastCharWasSplitChar = false;
            }
            else if (text[i] != SplitChar)
                continue;


            if (i + 1 == len)
                continue;

            var nextChar = text[i + 1];
            if (nextChar == SplitChar)
            {
                // add new page part to get rid of double @ in output!
                ++i;
                textPart.Length = i - textPart.Start;
                if (i == len)
                    continue;
                AddTextPartToList(list, textPart);
                textPart = new TextPart
#if DEBUG_TEXT_PARTS
            (textStr)
#endif
                {
                    Start = i + 1,
                    Length = len - i - 1
                };
                continue;
            }

            // found script start
            textPart.Length = i - textPart.Start;
            AddTextPartToList(list, textPart);
            textPart = new TextPart(
#if DEBUG_TEXT_PARTS
            textStr
#endif
        );
            if (expectElseBlock)
                --i;
            expectElseBlock = false;
            textPart.Start = i;
            textPart.Length = len - i;
            var remaining = text[(i + 1)..];
            if (remaining.StartsWith(SectionStr, StringComparison.Ordinal))
            {
                i += 9;
                textPart.Type = TextPartType.Section;
                SkipSection(list, ref textPart, ref i, text, len);
                lastCharWasSplitChar = i + 1 < len && text[i] == SplitChar && text[i + 1] != SplitChar;
                continue;
            }

            if (remaining.StartsWith("if", StringComparison.Ordinal))
            {
                i += 3;
                textPart.Type = TextPartType.IfStatement;
                SkipWhitespace(ref i, text, len);
                SkipScriptStatement(list, ref textPart, ref i, text, len);
                SkipWhitespace(ref i, text, len);
                if (i >= len || text[i] != OpenBracket)
                    throw new ViewSyntaxException("Invalid if statement.");
                textPart.Start = i;
                textPart.Length = len - i;
                textPart.Type = TextPartType.IfBlock;
                nextChar = OpenBracket;
                --i;
            }

            if (remaining.StartsWith("else ", StringComparison.Ordinal))
            {
                i += 5;
                SkipWhitespace(ref i, text, len);
                if (i < len && text[i..].StartsWith("if", StringComparison.Ordinal))
                {
                    i += 2;
                    textPart.Type = TextPartType.ElseIfStatement;
                    SkipWhitespace(ref i, text, len);
                    textPart.Start = i;
                    textPart.Length = len - i;
                    SkipScriptStatement(list, ref textPart, ref i, text, len);
                    SkipWhitespace(ref i, text, len);
                    if (i >= len || text[i] != OpenBracket)
                        throw new ViewSyntaxException("Invalid else if statement.");
                    textPart.Start = i;
                    textPart.Length = len - i;
                    textPart.Type = TextPartType.ElseIfBlock;
                    nextChar = OpenBracket;
                    --i;
                }
                else
                {
                    textPart.Type = TextPartType.ElseBlock;
                    SkipWhitespace(ref i, text, len);
                    if (i >= len || text[i] != OpenBracket)
                        throw new ViewSyntaxException("Invalid else statement.");
                    textPart.Start = i;
                    textPart.Length = len - i;
                    textPart.Type = TextPartType.ElseBlock;
                    nextChar = OpenBracket;
                    --i;
                }
            }

            if (nextChar == OpenBracket)
            {
                i += 2;
                var isIfBlock = textPart.IsIfBlock || textPart.IsElseIfBlock;
                if (!textPart.IsIfElseElseIfBlock)
                    textPart.Type = TextPartType.ScriptBlock;
                SkipBlock(list, ref textPart, ref i, text, len);
                SkipWhitespace(ref i, text, len);
                lastCharWasSplitChar = i + 1 < len && text[i] == SplitChar && text[i + 1] != SplitChar;
                if (isIfBlock && i < len && text[i..].StartsWith("else "))
                {
                    expectElseBlock = true;
                    lastCharWasSplitChar = true;
                }
                continue;
            }

            ++i;
            textPart.Type = TextPartType.ScriptStatement;
            textPart.Start = i;
            textPart.Length = len - i;
            SkipScriptStatement(list, ref textPart, ref i, text, len);
            lastCharWasSplitChar = i + 1 < len && text[i] == SplitChar && text[i + 1] != SplitChar;
        }
        AddTextPartToList(list, textPart);
        return result;
    }

    static void AddTextPartToList(List<TextPart> list, TextPart textPart)
    {
        if (textPart.Length <= 0)
            return;
        list.Add(textPart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SkipScriptStatement(
        List<TextPart> list,
        ref TextPart textPart,
        ref int i,
        ReadOnlySpan<char> text,
        int len)
    {
        var openParenthesis = 0;
        var startsWithParenthesis = text[i] == OpenParenthesis;
        for (; i < len; ++i)
        {
            var c = text[i];
            if (c == OpenParenthesis)
            {
                ++openParenthesis;
                continue;
            }

            if (c == CloseParenthesis)
            {
                // edge case: (@page.raw('test'))
                if (openParenthesis == 0)
                {
                    textPart.Length = i - textPart.Start;
                    AddTextPartToList(list, textPart);
                    textPart = new TextPart
#if DEBUG_TEXT_PARTS
            (text.ToString())
#endif
                    {
                        Start = i,
                        Length = len - i
                    };
                    return;
                }
                --openParenthesis;
                if (startsWithParenthesis && openParenthesis == 0)
                {
                    ++i;
                    textPart.Length = i - textPart.Start;
                    AddTextPartToList(list, textPart);
                    textPart = new TextPart
#if DEBUG_TEXT_PARTS
            (text.ToString())
#endif
                    {
                        Start = i,
                        Length = len - i
                    };
                    return;
                }
                continue;
            }
            if (c == OpenSquareBracket)
            {
                SkipSquareBrackets(ref i, text, len);
                continue;
            }
            if (openParenthesis > 0 && c == DoubleQuote)
            {
                SkipDoubleQuote(ref i, text, len);
                continue;
            }
            if (openParenthesis > 0 && c == SingleQuote)
            {
                SkipSingleQuote(ref i, text, len);
                continue;
            }
            if (openParenthesis > 0)
                continue;
            // single line script statement termination characters
            if (c == ' ' ||
                c == '<' ||
                c == DoubleQuote ||
                c == SingleQuote ||
                c == ';' ||
                char.IsWhiteSpace(c))
            {
                textPart.Length = i - textPart.Start;
                AddTextPartToList(list, textPart);
                SkipSingleNewLine(ref i, text, len);
                textPart = new TextPart
#if DEBUG_TEXT_PARTS
            (text.ToString())
#endif
                {
                    Start = i,
                    Length = len - i
                };
                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SkipBlock(
        List<TextPart> list,
        ref TextPart textPart,
        ref int i,
        ReadOnlySpan<char> text,
        int len)
    {
        SkipSingleNewLine(ref i, text, len);
        textPart.Start = i;
        textPart.Length = len - i;
        int openedBrackets = 1;
        for (; i < len; ++i)
        {
            var c = text[i];
            if (c == '/' && i + 1 < len)
            {
                // skip comments.
                var p = text[i + 1];
                if (p == '/')
                {
                    SkipLine(ref i, text, len);
                    continue;
                }
                else if (p == '*')
                {
                    SkipBlockComment(ref i, text, len);
                    continue;
                }
            }
            if (c == DoubleQuote)
            {
                SkipDoubleQuote(ref i, text, len);
                continue;
            }
            if (c == SingleQuote)
            {
                SkipSingleQuote(ref i, text, len);
                continue;
            }

            if (c == OpenBracket)
            {
                ++openedBrackets;
                continue;
            }
            if (c == CloseBracket)
            {
                if (openedBrackets == 1)
                {
                    ++i;
                    textPart.Length = i - textPart.Start;
                    --textPart.Length;
                    AddTextPartToList(list, textPart);
                    SkipSingleNewLine(ref i, text, len);
                    textPart = new TextPart
#if DEBUG_TEXT_PARTS
            (text.ToString())
#endif
                    {
                        Start = i,
                        Length = len - i
                    };
                    return;
                }
                --openedBrackets;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SkipSquareBrackets(ref int i, ReadOnlySpan<char> text, int len)
    {
        ++i;
        for (; i < len; ++i)
        {
            var c = text[i];
            if (c == SingleQuote)
            {
                SkipSingleQuote(ref i, text, len);
                continue;
            }
            if (c == DoubleQuote)
            {
                SkipDoubleQuote(ref i, text, len);
                continue;
            }
            if (c != CloseSquareBracket)
                continue;
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SkipDoubleQuote(ref int i, ReadOnlySpan<char> text, int len)
    {
        ++i;
        for (; i < len; ++i)
        {
            var c = text[i];
            if (c != DoubleQuote)
                continue;
            var previous = text[i - 1];
            if (previous == EscapeChar)
                continue;
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SkipSingleQuote(ref int i, ReadOnlySpan<char> text, int len)
    {
        ++i;
        for (; i < len; ++i)
        {
            var c = text[i];
            if (c != SingleQuote)
                continue;
            var previous = text[i - 1];
            if (previous == EscapeChar)
                continue;
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string GetLayout(ref int i, ReadOnlySpan<char> text, int len)
    {
        var fixLen = LayoutStr.Length;
        var start = i + fixLen + 1;
        i += fixLen;
        SkipWhitespace(ref i, text, len);
        for (; i < len; ++i)
        {
            var c = text[i];
            if (c == '=')
            {
                start = i + 1;
                ++i;
                SkipWhitespace(ref i, text, len);
                --i;
                continue;
            }
            if (c == DoubleQuote)
            {
                start = i + 1;
                SkipDoubleQuote(ref i, text, len);
                continue;
            }
            if (c == SingleQuote)
            {
                start = i + 1;
                SkipSingleQuote(ref i, text, len);
                continue;
            }
            if (char.IsWhiteSpace(c) || i == len - 1)
            {
                return
                    text[start..i]
                    .Trim()
                    .Trim(DoubleQuote)
                    .Trim(SingleQuote)
                    .ToString();
            }
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SkipSingleNewLine(ref int i, ReadOnlySpan<char> text, int len)
    {
        if (i < len && text[i] == '\r')
            ++i;
        if (i < len && text[i] == '\n')
            ++i;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SkipLine(ref int i, ReadOnlySpan<char> text, int len)
    {
        while (i < len && text[i] != '\n')
            ++i;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SkipBlockComment(ref int i, ReadOnlySpan<char> text, int len)
    {
        while (i + 1 < len && !(text[i] == '*' && text[i + 1] == '/'))
            ++i;
        ++i;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SkipWhitespace(ref int i, ReadOnlySpan<char> text, int len)
    {
        for (; i < len; ++i)
        {
            var c = text[i];
            if (char.IsWhiteSpace(c))
                continue;
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SkipSection(
        List<TextPart> list,
        ref TextPart textPart,
        ref int i,
        ReadOnlySpan<char> text,
        int len)
    {
        SkipWhitespace(ref i, text, len);
        if (i >= len)
            throw new ViewSyntaxException("Invalid section in the end of the file.");
        if (text[i] == SplitChar)
        {
            ++i;
            if (i >= len)
                throw new ViewSyntaxException("Invalid section in the end of the file.");
            textPart.Type = TextPartType.ScriptSection;
        }
        var endOfScriptDefinition = text[i..].IndexOf(OpenBracket);
        if (endOfScriptDefinition < 0)
            throw new ViewSyntaxException("Invalid section block. Section block must have name and brackets");
        var sectionName = text.Slice(i, endOfScriptDefinition);
        var sectionParameterBeginIndex = sectionName.IndexOf(OpenParenthesis);
        var sectionParameterEndIndex = sectionName.IndexOf(CloseParenthesis);
        if (sectionParameterBeginIndex != -1 && sectionParameterEndIndex != -1)
        {
            textPart.Parameters = $",{sectionName.Slice(sectionParameterBeginIndex + 1, sectionParameterEndIndex - sectionParameterBeginIndex - 1)}";
            sectionName = sectionName.Slice(0, sectionParameterBeginIndex);
        }
        if (sectionName.IsWhiteSpace())
            throw new ViewSyntaxException("Invalid section. The section name is missing.");
        textPart.SectionName = sectionName.Trim().ToString();

        i += endOfScriptDefinition;
        textPart.Start = i;
        textPart.Length = len - i;
        ++i;
        SkipBlock(list, ref textPart, ref i, text, len);
    }
}
