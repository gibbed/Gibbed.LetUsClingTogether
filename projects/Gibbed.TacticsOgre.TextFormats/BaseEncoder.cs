/* Copyright (c) 2026 Rick (rick 'at' gibbed 'dot' us)
*
* This software is provided 'as-is', without any express or implied
* warranty. In no event will the authors be held liable for any damages
* arising from the use of this software.
*
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
*
* 1. The origin of this software must not be misrepresented; you must not
*    claim that you wrote the original software. If you use this software
*    in a product, an acknowledgment in the product documentation would
*    be appreciated but is not required.
*
* 2. Altered source versions must be plainly marked as such, and must not
*    be misrepresented as being the original software.
*
* 3. This notice may not be removed or altered from any source
*    distribution.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.TacticsOgre.TextFormats;

public abstract class BaseEncoder : IEncoder
{
    protected abstract void WriteText(string text, Stream output);
    protected abstract void WriteMacroPrefix(Stream output);
    protected abstract void WriteEscape(byte escape, Stream output);
    protected abstract void WriteGaiji(uint codepoint, Stream output);

    internal abstract Dialect Dialect { get; }

    protected bool IsReborn => this.Dialect == Dialect.Reborn;

    private enum FrameKind
    {
        Conditional,
        Chain,
    }

    private sealed class Frame
    {
        public FrameKind Kind;
        public long PatchPosition = -1;
    }

    private sealed class State
    {
        public readonly MemoryStream Body = new();
        public readonly Stack<Frame> Frames = new();
        public byte[] Header;
        public SizeHeader? ComputedHeader;
        public readonly List<byte[]> Records = [];
        public bool HasHeader;
        public int CharacterCount;
        public int NewlineCount;
        public int ControlCodeCount;
    }

    public void Encode(string text, Stream output, Endian endian)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(output);

        State state = new();
        var body = state.Body;

        StringBuilder pending = new();
        void FlushPending()
        {
            if (pending.Length > 0)
            {
                var value = pending.ToString();
                this.WriteText(value, body);
                foreach (var _ in value.EnumerateRunes())
                {
                    state.CharacterCount++;
                }
                pending.Clear();
            }
        }

        int i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            if (c == '{' && i + 1 < text.Length && text[i + 1] == '{')
            {
                pending.Append('{');
                i += 2;
                continue;
            }
            if (c == '}' && i + 1 < text.Length && text[i + 1] == '}')
            {
                pending.Append('}');
                i += 2;
                continue;
            }
            if (c == '}')
            {
                throw new FormatException($"unmatched '}}' at {i}");
            }
            if (c == '\r')
            {
                i++;
                continue;
            }
            if (c == '\n')
            {
                FlushPending();
                this.WriteMacro(MacroCode.InsertNewline, state);
                i++;
                continue;
            }
            if (c != '{')
            {
                pending.Append(c);
                i++;
                continue;
            }

            var end = text.IndexOf('}', i + 1);
            if (end < 0)
            {
                throw new FormatException($"unclosed tag at {i}");
            }
            FlushPending();
            var tag = text.Substring(i + 1, end - i - 1);
            i = end + 1;
            
            if (this.WriteTag(tag, state) == true && i < text.Length && text[i] == '\n')
            {
                i++;
            }
        }
        FlushPending();

        if (state.Frames.Count > 0)
        {
            throw new FormatException(state.Frames.Peek().Kind == FrameKind.Conditional
                ? "{cond} without {/cond}"
                : "{if} without {end}");
        }

        if (state.HasHeader == true)
        {
            byte[] header;
            if (state.ComputedHeader.HasValue == true)
            {
                var computed = state.ComputedHeader.Value;
                computed.CharacterCount = state.CharacterCount;
                computed.LineCount = state.NewlineCount + 1;
                computed.HasControlCodes = state.ControlCodeCount > 0;
                computed.HasRecords = state.Records.Count > 0;
                header = new byte[5];
                computed.Pack(header);
            }
            else
            {
                header = (byte[])state.Header.Clone();
                header[0] = (byte)((header[0] & 0xFE) | (state.Records.Count > 0 ? 1 : 0));
            }
            this.WriteEscape(0xFC, output);
            output.Write(header, 0, header.Length);
            for (int j = 0; j < state.Records.Count; j++)
            {
                var record = (byte[])state.Records[j].Clone();
                record[0] = (byte)((record[0] & 0xFE) | (j + 1 < state.Records.Count ? 1 : 0));
                output.Write(record, 0, record.Length);
            }
        }

        body.Position = 0;
        body.CopyTo(output);
        output.WriteValueU8(0);
    }

    private void WriteMacro(MacroCode code, State state, params byte[] args)
    {
        var size = code.GetSize(this.Dialect);
        if (size == 0)
        {
            throw new FormatException($"control code {(byte)code:X2} does not exist in {this.Dialect} text");
        }
        if (args.Length != size - 1)
        {
            throw new FormatException($"{code} takes {size - 1} argument byte(s) in {this.Dialect} text, got {args.Length}");
        }
        foreach (var arg in args)
        {
            if (arg == 0)
            {
                throw new FormatException($"{code} argument bytes can't be zero");
            }
        }
        this.WriteMacroPrefix(state.Body);
        state.Body.WriteValueU8((byte)code);
        state.Body.Write(args, 0, args.Length);
        state.ControlCodeCount++;
        if (code == MacroCode.InsertNewline)
        {
            state.NewlineCount++;
        }
    }

    private long WriteMacroWithLength(MacroCode code, State state, params byte[] args)
    {
        var allArgs = new byte[args.Length + 2];
        args.CopyTo(allArgs, 0);
        allArgs[args.Length] = 1;
        allArgs[args.Length + 1] = 1;
        this.WriteMacro(code, state, allArgs);
        return state.Body.Position - 2;
    }

    private static void PatchLength(State state, long patchPosition)
    {
        if (patchPosition < 0)
        {
            return;
        }
        var body = state.Body;
        var length = body.Position - (patchPosition + 2);
        if (length > (254 * 255) + 254)
        {
            throw new FormatException("conditional text is too long");
        }
        var (a, b) = MacroCodeHelpers.EncodeU16((int)length);
        var position = body.Position;
        body.Position = patchPosition;
        body.WriteValueU8(a);
        body.WriteValueU8(b);
        body.Position = position;
    }

    private bool WriteTag(string tag, State state)
    {
        var parts = tag.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            throw new FormatException("empty tag {}");
        }

        int Number(string text, int min, int max)
        {
            if (int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value) == false)
            {
                throw new FormatException($"bad number '{text}' in tag {{{tag}}}");
            }
            if (value < min || value > max)
            {
                throw new FormatException($"'{text}' in tag {{{tag}}} goes from {min} to {max}");
            }
            return value;
        }

        byte Arg(int index, int bias = 0)
        {
            if (index >= parts.Length)
            {
                throw new FormatException($"tag {{{tag}}} is missing an argument");
            }
            return (byte)(Number(parts[index], bias == 0 ? 1 : 0, 255 - bias) + bias);
        }

        void NoMoreArgs(int count)
        {
            if (parts.Length != count)
            {
                throw new FormatException($"tag {{{tag}}} takes {count - 1} argument(s)");
            }
        }

        Dictionary<string, string> Options(int start, params string[] names)
        {
            Dictionary<string, string> options = new();
            for (int j = start; j < parts.Length; j++)
            {
                var equals = parts[j].IndexOf('=');
                var key = equals < 0 ? parts[j] : parts[j].Substring(0, equals);
                if (Array.IndexOf(names, key) < 0 || options.ContainsKey(key) == true)
                {
                    throw new FormatException($"unexpected '{parts[j]}' in tag {{{tag}}}");
                }
                options[key] = equals < 0 ? null : parts[j].Substring(equals + 1);
            }
            return options;
        }

        byte BiasedOption(Dictionary<string, string> options, string key, byte fallback)
        {
            return options.TryGetValue(key, out var value) == false
                ? fallback
                : (byte)(Number(value, 0, 254) + 1);
        }

        void Body(string what)
        {
            if (state.Body.Length > 0)
            {
                throw new FormatException($"{what} must come before any text");
            }
        }

        var name = parts[0];
        if (name.Length == 9 && name[0] == '#')
        {
            NoMoreArgs(1);
            if (uint.TryParse(name.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgba) == false)
            {
                throw new FormatException($"bad color in tag {{{tag}}}");
            }
            if (((rgba >> 24) & 0xFFu) == 0xFFu ||
                ((rgba >> 16) & 0xFFu) == 0xFFu ||
                ((rgba >> 8) & 0xFFu) == 0xFFu ||
                ((rgba >> 0) & 0xFFu) == 0xFFu)
            {
                throw new FormatException($"color components go up to FE in {{{tag}}}");
            }
            this.WriteMacro(
                MacroCode.SetTextColor,
                state,
                (byte)((rgba >> 24) + 1),
                (byte)(((rgba >> 16) & 0xFFu) + 1),
                (byte)(((rgba >> 8) & 0xFFu) + 1),
                (byte)((rgba & 0xFFu) + 1));
            return false;
        }

        if (name.StartsWith("unk#", StringComparison.Ordinal) == true)
        {
            if (byte.TryParse(name.AsSpan(4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code) == false)
            {
                throw new FormatException($"bad macro code in tag {{{tag}}}");
            }
            var args = new byte[parts.Length - 1];
            for (int j = 0; j < args.Length; j++)
            {
                args[j] = Arg(j + 1);
            }
            this.WriteMacro((MacroCode)code, state, args);
            return false;
        }

        switch (name)
        {
            case "pp":
            {
                NoMoreArgs(1);
                this.WriteMacro(MacroCode.InsertPageBreak, state);
                return true;
            }
            case "ww":
            case "wordwrap": // deprecated
            case "np": // deprecated
            {
                NoMoreArgs(1);
                this.WriteMacro(MacroCode.EnableWordWrap, state);
                return false;
            }
            case "wrap":
            {
                NoMoreArgs(1);
                var args = new byte[MacroCode.IndicateWrapArea.GetSize(this.Dialect) - 1];
                Array.Fill(args, (byte)1);
                this.WriteMacro(MacroCode.IndicateWrapArea, state, args);
                return false;
            }
            case "uc":
            {
                NoMoreArgs(1);
                this.WriteMacro(MacroCode.UppercaseFollowingCharacter, state);
                return false;
            }
            case "space":
            {
                NoMoreArgs(2);
                var value = Number(parts[1], -128, 127);
                if (value == 0)
                {
                    throw new FormatException("{space 0} can't be stored");
                }
                this.WriteMacro(MacroCode.InsertPixelSpacing, state, (byte)(sbyte)value);
                return false;
            }
            case "icon":
            {
                NoMoreArgs(2);
                this.WriteMacro(MacroCode.InsertIcon, state, Arg(1));
                return false;
            }
            case "select":
            {
                this.WriteMacro(MacroCode.InsertSelect, state, parts.Length == 1 ? (byte)1 : Arg(1));
                return true;
            }
            case "pal":
            {
                NoMoreArgs(2);
                if (parts[1] == "reset")
                {
                    this.WriteMacro(MacroCode.ResetFontPalette, state);
                }
                else
                {
                    this.WriteMacro(MacroCode.SetFontPalette, state, Arg(1));
                }
                return false;
            }
            case "color":
            {
                if (parts.Length != 2 || parts[1] != "reset")
                {
                    break;
                }
                this.WriteMacro(MacroCode.ResetTextColor, state);
                return false;
            }
            case "hero":
            case "order":
            {
                if (parts.Length != 2 || parts[1] != "name")
                {
                    break;
                }
                this.WriteMacro(name == "hero"
                        ? MacroCode.InsertHeroName
                        : MacroCode.InsertOrderName
                    , state
                );
                return false;
            }
            case "str":
            {
                var options = Options(2, "value", "count");
                this.WriteMacro(
                    MacroCode.InsertStringVariable,
                    state,
                    Arg(1, 1),
                    BiasedOption(options, "value", 1),
                    BiasedOption(options, "count", 0xFF),
                    0xF1,
                    0xF0
                );
                return false;
            }
            case "num":
            {
                var options = Options(2, "word", "format");
                if (this.IsReborn == true)
                {
                    this.WriteMacro(
                        MacroCode.InsertNumberVariable,
                        state,
                        Arg(1, 1),
                        BiasedOption(options, "word", 0xFF),
                        1,
                        BiasedOption(options, "format", 1)
                    );
                }
                else
                {
                    if (options.ContainsKey("format") == true)
                    {
                        throw new FormatException("format= is Reborn only");
                    }
                    this.WriteMacro(
                        MacroCode.InsertNumberVariable,
                        state,
                        Arg(1, 1),
                        BiasedOption(options, "word", 0xFF),
                        1
                    );
                }
                return false;
            }
            case "speed":
            case "auto":
            {
                NoMoreArgs(2);
                var value = parts[1];
                var code = MacroCode.SetTextSpeed;
                if (name == "auto")
                {
                    var seconds = value.EndsWith("s", StringComparison.Ordinal);
                    code = seconds == true
                        ? MacroCode.AutoAdvanceSeconds
                        : MacroCode.AutoAdvanceFrames;
                    if (seconds == true)
                    {
                        value = value.Substring(0, value.Length - 1);
                    }
                }
                var (a, b) = MacroCodeHelpers.EncodeU16(Number(value, 0, (254 * 255) + 254));
                this.WriteMacro(code, state, a, b);
                return false;
            }
            case "pause":
            {
                NoMoreArgs(2);
                this.WriteMacro(MacroCode.Pause, state, Arg(1, 1));
                return false;
            }
            case "title":
            {
                NoMoreArgs(2);
                var title = Array.IndexOf(MacroCodeHelpers.WindowTitles, parts[1]);
                this.WriteMacro(
                    MacroCode.SetWindowTitle,
                    state,
                    title >= 0
                        ? (byte)(title + 1)
                        : Arg(1, 1)
                );
                return false;
            }
            case "vbreak":
            {
                NoMoreArgs(2);
                this.WriteMacro(
                    parts[1] switch
                    {
                        "ja" => MacroCode.VoiceBreakJapanese,
                        "en" => MacroCode.VoiceBreakEnglish,
                        _ => throw new FormatException($"unknown voice language in {{{tag}}}"),
                    },
                    state
                );
                return false;
            }
            case "center":
            {
                NoMoreArgs(1);
                this.WriteMacro(MacroCode.Center, state, 1, 2);
                return false;
            }
            case "em":
            case "/em":
            {
                NoMoreArgs(1);
                this.WriteMacro(
                    MacroCode.Emphasis,
                    state,
                    name == "em"
                        ? (byte)2
                        : (byte)1
                );
                return false;
            }
            case "symbol":
            case "button":
            {
                if (this.IsReborn == false)
                {
                    break;
                }
                NoMoreArgs(4);
                // Stored plus one, like colors.
                this.WriteMacro(
                    name == "symbol"
                        ? MacroCode.InsertSymbol
                        : MacroCode.InsertButton,
                    state,
                    Arg(1, 1),
                    Arg(2, 1),
                    Arg(3, 1)
                );
                return false;
            }
            case "param":
            {
                if (parts.Length < 2)
                {
                    throw new FormatException("{param} needs an index");
                }
                var index = Number(parts[1], 0, 255 + 0xFD);
                bool fe;
                if (parts.Length == 3 && parts[2] == "FE" && index == 254)
                {
                    fe = true;
                }
                else
                {
                    NoMoreArgs(2);
                    fe = index > 254;
                }
                this.WriteEscape(
                    fe == true
                        ? (byte)0xFE
                        : (byte)0xFD,
                    state.Body
                );
                state.Body.WriteValueU8(
                    fe == true
                        ? (byte)(index - 0xFD)
                        : (byte)(index + 1)
                );
                return false;
            }
            case "cond":
            case "zombie": // older name
            {
                NoMoreArgs(2);
                var condition = parts[1] == "undead"
                    ? (byte)(MacroCodeHelpers.Conditions.Undead + 1)
                    : Arg(1, 1);
                var patch = this.WriteMacroWithLength(MacroCode.Conditional, state, condition);
                state.Frames.Push(
                    new()
                    {
                        Kind = FrameKind.Conditional,
                        PatchPosition = patch,
                    }
                );
                return false;
            }
            case "/cond":
            case "/zombie":
            {
                NoMoreArgs(1);
                if (state.Frames.Count == 0 || state.Frames.Peek().Kind != FrameKind.Conditional)
                {
                    throw new FormatException("{/cond} without {cond}");
                }
                this.WriteMacro(MacroCode.ConditionalEnd, state);
                PatchLength(state, state.Frames.Pop().PatchPosition);
                return false;
            }
            case "if":
            {
                var code = string.Join(' ', parts, 1, parts.Length - 1) switch
                {
                    "singular" => MacroCode.IfSingular,
                    "masc" => MacroCode.IfMasculine,
                    "vowel order" => MacroCode.IfOrderNameStartsWithVowel,
                    "vowel hero" => MacroCode.IfHeroNameStartsWithVowel,
                    _ => throw new FormatException($"unknown condition in {{{tag}}}"),
                };
                var patch = this.WriteMacroWithLength(code, state);
                state.Frames.Push(
                    new()
                    {
                        Kind = FrameKind.Chain,
                        PatchPosition = patch,
                    }
                );
                return false;
            }
            case "elif":
            case "else":
            {
                if (name == "elif" && (parts.Length != 2 || parts[1] != "fem"))
                {
                    throw new FormatException($"unknown condition in {{{tag}}}");
                }
                if (name == "else")
                {
                    NoMoreArgs(1);
                }
                if (state.Frames.Count == 0 || state.Frames.Peek().Kind != FrameKind.Chain)
                {
                    throw new FormatException($"{{{tag}}} without {{if}}");
                }
                var frame = state.Frames.Peek();
                PatchLength(state, frame.PatchPosition);
                frame.PatchPosition = this.WriteMacroWithLength(
                    name == "elif"
                        ? MacroCode.ElseIfFeminine
                        : MacroCode.Else,
                    state
                );
                return false;
            }
            case "end":
            {
                NoMoreArgs(1);
                if (state.Frames.Count == 0 || state.Frames.Peek().Kind != FrameKind.Chain)
                {
                    throw new FormatException("{end} without {if}");
                }
                PatchLength(state, state.Frames.Pop().PatchPosition);
                this.WriteMacro(MacroCode.EndIf, state);
                return false;
            }
            case "size":
            {
                Body("{size}");
                if (state.HasHeader == true)
                {
                    throw new FormatException("only one {size} per string");
                }
                state.HasHeader = true;
                if (parts.Length == 3 && parts[1] == "raw")
                {
                    state.Header = ParseHex(parts[2], 5, tag, true);
                    return false;
                }
                if (parts.Length < 2)
                {
                    throw new FormatException("{size} needs a width");
                }
                var options = Options(2, "a", "flag7");
                SizeHeader header = default;
                header.Width = Number(parts[1], -128, (127 * 127) + 127 - 128);
                header.A = options.TryGetValue("a", out var a) == true
                    ? Number(a, 0, 31)
                    : 0;
                header.Flag7 = options.ContainsKey("flag7");
                state.ComputedHeader = header;
                return false;
            }
            case "record":
            {
                Body("{record}");
                NoMoreArgs(2);
                if (state.HasHeader == false)
                {
                    throw new FormatException("{record} must follow {size}");
                }
                state.Records.Add(ParseHex(parts[1], 6, tag, false));
                return false;
            }
            case "gaiji":
            {
                NoMoreArgs(2);
                uint codepoint;
                if (parts[1].StartsWith("0x", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (uint.TryParse(parts[1].AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codepoint) == false)
                    {
                        throw new FormatException($"bad codepoint in tag {{{tag}}}");
                    }
                }
                else if (GaijiInfo.TryGetCodepoint(this.Dialect, parts[1], out codepoint) == false)
                {
                    throw new FormatException($"unknown gaiji '{parts[1]}'");
                }
                this.WriteGaiji(codepoint, state.Body);
                state.CharacterCount++;
                return false;
            }
        }

        throw new FormatException($"tag {{{tag}}} can't be encoded");
    }

    private static byte[] ParseHex(string text, int count, string tag, bool checkFirst)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromHexString(text);
        }
        catch (FormatException)
        {
            throw new FormatException($"bad hex in tag {{{tag}}}");
        }
        if (bytes.Length != count)
        {
            throw new FormatException($"tag {{{tag}}} needs {count} bytes");
        }
        for (int i = checkFirst == true ? 0 : 1; i < bytes.Length; i++)
        {
            if (bytes[i] == 0)
            {
                throw new FormatException($"tag {{{tag}}} can't contain a zero byte");
            }
        }
        return bytes;
    }
}
