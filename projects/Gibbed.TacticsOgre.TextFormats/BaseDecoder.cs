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
using System.IO;
using System.Text;
using Gibbed.IO;
using static Gibbed.TacticsOgre.Extensions.InvariantShorthand;

namespace Gibbed.TacticsOgre.TextFormats;

public abstract class BaseDecoder : IDecoder
{
    protected abstract Encoding Encoding { get; }
    internal abstract Dialect Dialect { get; }

    // PSP: FF
    // Reborn: U+F8FF (EF A3 BF)
    protected abstract byte[] MacroPrefix { get; }

    protected enum CodepointType
    {
        Null,
        RawBytes,
        Gaiji, // https://en.wiktionary.org/wiki/gaiji

        // macro control codes
        Span, // size header
        ParameterFD,
        ParameterFE,
        Macro,
    }

    protected abstract CodepointType Decode(Stream input, Endian endian, byte[] bytes, out int length, out uint codepoint);

    private sealed class State
    {
        public readonly StringBuilder Output = new();
        public int CharacterCount;
        public int NewlineCount;
        public int ControlCodeCount;
        public readonly HashSet<long> BranchPositions = [];
    }

    public string Decode(Stream input, Endian endian)
    {
        ArgumentNullException.ThrowIfNull(input);

        var encoding = this.Encoding;

        var pendingBytes = new byte[128];
        var pendingLength = pendingBytes.Length;
        int pendingIndex = 0;

        State state = new();
        var output = state.Output;

        void FlushPending()
        {
            if (pendingIndex < 1)
            {
                return;
            }
            var value = encoding.GetString(pendingBytes, 0, pendingIndex);
            output.Append(value.Replace("{", "{{").Replace("}", "}}"));
            pendingIndex = 0;
        }

        byte[] header = null;
        List<byte[]> records = [];

        var bytes = new byte[4];
        for (bool first = true; ; first = false)
        {
            var type = this.Decode(input, endian, bytes, out var length, out var codepoint);
            if (type == CodepointType.Null)
            {
                break;
            }

            if (type != CodepointType.RawBytes || pendingIndex + length > pendingLength)
            {
                FlushPending();
            }

            switch (type)
            {
                case CodepointType.RawBytes:
                {
                    Array.Copy(bytes, 0, pendingBytes, pendingIndex, length);
                    pendingIndex += length;
                    state.CharacterCount++;
                    continue;
                }

                case CodepointType.Gaiji:
                {
                    output.Append($"{{gaiji {GaijiInfo.GetLabel(this.Dialect, codepoint)}}}");
                    state.CharacterCount++;
                    continue;
                }

                case CodepointType.Span:
                {
                    if (first == false)
                    {
                        throw new FormatException("size header can only start a string");
                    }

                    header = ReadBytes(input, 5);
                    if ((header[0] & 1) != 0)
                    {
                        byte[] record;
                        do
                        {
                            record = ReadBytes(input, 6);
                            records.Add(record);
                        }
                        while ((record[0] & 1) != 0);
                    }

                    continue;
                }

                case CodepointType.ParameterFD:
                {
                    var value = input.ReadValueU8();
                    output.Append(_($"{{param {value - 1}}}"));
                    continue;
                }

                case CodepointType.ParameterFE:
                {
                    var value = input.ReadValueU8();
                    // FD FF and FE 01 both mean parameter 254
                    output.Append(value == 1 ? "{param 254 FE}" : _($"{{param {value + 0xFD}}}"));
                    continue;
                }

                case CodepointType.Macro:
                {
                    state.ControlCodeCount++;
                    this.DecodeMacro(input, state);
                    continue;
                }
            }

            throw new NotSupportedException();
        }

        FlushPending();

        if (header != null)
        {
            output.Insert(0, FormatHeader(header, records, state));
        }

        return output.ToString();
    }

    private static byte[] ReadBytes(Stream input, int count)
    {
        var bytes = new byte[count];
        if (input.Read(bytes, 0, count) != count)
        {
            throw new EndOfStreamException();
        }
        return bytes;
    }

    private static string FormatHeader(byte[] bytes, List<byte[]> records, State state)
    {
        StringBuilder sb = new();

        var header = SizeHeader.Unpack(bytes);
        var repacked = new byte[5];
        header.Pack(repacked);

        if (repacked.AsSpan().SequenceEqual(bytes) == true &&
            header.CharacterCount == state.CharacterCount &&
            header.LineCount == state.NewlineCount + 1 &&
            header.HasControlCodes == (state.ControlCodeCount > 0))
        {
            sb.Append(_($"{{size {header.Width}"));
            if (header.A != 0)
            {
                sb.Append(_($" a={header.A}"));
            }
            if (header.Flag7 == true)
            {
                sb.Append(" flag7");
            }
            sb.Append('}');
        }
        else
        {
            sb.Append($"{{size raw {Convert.ToHexString(bytes)}}}");
        }
        foreach (var record in records)
        {
            var copy = (byte[])record.Clone();
            copy[0] &= 0xFE;
            sb.Append($"{{record {Convert.ToHexString(copy)}}}");
        }

        return sb.ToString();
    }

    private void DecodeMacro(Stream input, State state)
    {
        var output = state.Output;
        var codePosition = input.Position;
        var code = (MacroCode)input.ReadValueU8();
        var size = code.GetSize(this.Dialect);
        if (size == 0)
        {
            throw new FormatException(_($"control code {(byte)code:X2} does not exist in {this.Dialect} text"));
        }

        var args = ReadBytes(input, size - 1);
        var bodyPosition = input.Position;

        bool AllOnes()
        {
            foreach (var b in args)
            {
                if (b != 1)
                {
                    return false;
                }
            }
            return true;
        }

        switch (code)
        {
            case MacroCode.InsertNewline:
            {
                state.NewlineCount++;
                output.Append('\n');
                return;
            }

            case MacroCode.IndicateWrapArea:
            {
                if (AllOnes() == true)
                {
                    output.Append("{wrap}");
                    return;
                }
                break;
            }

            case MacroCode.SetTextColor:
            {
                output.Append(_($"{{#{args[0] - 1:X2}{args[1] - 1:X2}{args[2] - 1:X2}{args[3] - 1:X2}}}"));
                return;
            }

            case MacroCode.InsertStringVariable:
            {
                if (args[3] != 0xF1 || args[4] != 0xF0)
                {
                    break;
                }

                output.Append(_($"{{str {args[0] - 1}"));

                if (args[1] != 1)
                {
                    output.Append(_($" value={args[1] - 1}"));
                }

                if (args[2] != 0xFF)
                {
                    output.Append(_($" count={args[2] - 1}"));
                }

                output.Append('}');
                return;
            }

            case MacroCode.InsertNumberVariable:
            {
                if (args[2] != 1)
                {
                    break;
                }

                output.Append(_($"{{num {args[0] - 1}"));

                if (args[1] != 0xFF)
                {
                    output.Append(_($" word={args[1] - 1}"));
                }

                if (args.Length > 3 && args[3] != 1)
                {
                    output.Append(_($" format={args[3] - 1}"));
                }

                output.Append('}');
                return;
            }

            case MacroCode.SetTextSpeed:
            {
                output.Append(_($"{{speed {MacroCodeHelpers.DecodeU16(args[0], args[1])}}}"));
                return;
            }

            case MacroCode.InsertPageBreak:
            {
                output.Append("{pp}\n");
                return;
            }

            case MacroCode.InsertPixelSpacing:
            {
                // signed, not biased
                output.Append(_($"{{space {(sbyte)args[0]}}}"));
                return;
            }

            case MacroCode.Pause:
            {
                output.Append(_($"{{pause {args[0] - 1}}}"));
                return;
            }

            case MacroCode.InsertIcon:
            {
                output.Append(_($"{{icon {args[0]}}}"));
                return;
            }

            case MacroCode.InsertSelect:
            {
                output.Append(args[0] == 1
                    ? "{select}\n"
                    : _($"{{select {args[0]}}}\n"));
                return;
            }

            case MacroCode.Center:
            {
                // half-width (filled in by layout), then 02 = start
                if (args[0] == 1 && args[1] == 2)
                {
                    output.Append("{center}");
                    return;
                }
                break;
            }

            case MacroCode.InsertHeroName:
            {
                output.Append("{hero name}");
                return;
            }

            case MacroCode.InsertOrderName:
            {
                output.Append("{order name}");
                return;
            }

            case MacroCode.Conditional:
            {
                // length counts from here through the closing 96
                var length = MacroCodeHelpers.DecodeU16(args[1], args[2]);
                var endPosition = bodyPosition + length - 1;
                if (endPosition >= bodyPosition + this.MacroPrefix.Length &&
                    this.IsMacroAt(input, endPosition, MacroCode.ConditionalEnd) == true)
                {
                    state.BranchPositions.Add(endPosition);
                    output.Append(_($"{{cond {args[0] - 1}}}"));
                    return;
                }
                break;
            }

            case MacroCode.ConditionalEnd:
            {
                if (state.BranchPositions.Contains(codePosition) == true)
                {
                    output.Append("{/cond}");
                    return;
                }
                break;
            }

            case MacroCode.AutoAdvanceSeconds:
            {
                output.Append(_($"{{auto {MacroCodeHelpers.DecodeU16(args[0], args[1])}s}}"));
                return;
            }

            case MacroCode.AutoAdvanceFrames:
            {
                output.Append(_($"{{auto {MacroCodeHelpers.DecodeU16(args[0], args[1])}}}"));
                return;
            }

            case MacroCode.ResetTextColor:
            {
                output.Append("{color reset}");
                return;
            }

            case MacroCode.SetFontPalette:
            {
                output.Append(_($"{{pal {args[0]}}}"));
                return;
            }

            case MacroCode.ResetFontPalette:
            {
                output.Append("{pal reset}");
                return;
            }

            case MacroCode.UppercaseFollowingCharacter:
            {
                output.Append("{uc}");
                return;
            }

            case MacroCode.EnableWordWrap:
            {
                output.Append("{ww}");
                return;
            }

            case MacroCode.Emphasis:
            {
                if (args[0] == 1 || args[0] == 2)
                {
                    output.Append(args[0] == 2 ? "{em}" : "{/em}");
                    return;
                }
                break;
            }

            case MacroCode.IfSingular:
            case MacroCode.IfMasculine:
            case MacroCode.IfOrderNameStartsWithVowel:
            case MacroCode.IfHeroNameStartsWithVowel:
            {
                var length = MacroCodeHelpers.DecodeU16(args[0], args[1]);
                if (this.ValidateChain(input, bodyPosition + length, state) == true)
                {
                    output.Append(code switch
                    {
                        MacroCode.IfSingular => "{if singular}",
                        MacroCode.IfMasculine => "{if masc}",
                        MacroCode.IfOrderNameStartsWithVowel => "{if vowel order}",
                        _ => "{if vowel hero}",
                    });
                    return;
                }
                break;
            }

            case MacroCode.ElseIfFeminine:
            case MacroCode.Else:
            case MacroCode.EndIf:
            {
                if (state.BranchPositions.Contains(codePosition) == true)
                {
                    output.Append(code switch
                    {
                        MacroCode.ElseIfFeminine => "{elif fem}",
                        MacroCode.Else => "{else}",
                        _ => "{end}",
                    });
                    return;
                }
                break;
            }

            case MacroCode.InsertSymbol:
            case MacroCode.InsertButton:
            {
                var name = code == MacroCode.InsertSymbol ? "symbol" : "button";
                output.Append(_($"{{{name} {args[0] - 1} {args[1] - 1} {args[2] - 1}}}"));
                return;
            }
        }

        output.Append(_($"{{unk#{(byte)code:X2}"));
        foreach (var b in args)
        {
            output.Append(_($" {b}"));
        }
        output.Append('}');
    }

    private bool IsMacroAt(Stream input, long position, MacroCode code)
    {
        var prefix = this.MacroPrefix;
        var start = position - prefix.Length;
        if (start < 0 || position >= input.Length)
        {
            return false;
        }

        var oldPosition = input.Position;
        input.Position = start;
        var bytes = new byte[prefix.Length + 1];
        var read = input.Read(bytes, 0, bytes.Length);
        input.Position = oldPosition;
        return read == bytes.Length &&
            bytes.AsSpan(0, prefix.Length).SequenceEqual(prefix) == true &&
            bytes[prefix.Length] == (byte)code;
    }

    private bool ValidateChain(Stream input, long branchEnd, State state)
    {
        var prefixLength = this.MacroPrefix.Length;
        List<long> positions = [];
        var oldPosition = input.Position;
        try
        {
            for (; ; )
            {
                var codePosition = branchEnd + prefixLength;

                if (this.IsMacroAt(input, codePosition, MacroCode.EndIf) == true)
                {
                    positions.Add(codePosition);
                    state.BranchPositions.UnionWith(positions);
                    return true;
                }

                if (this.IsMacroAt(input, codePosition, MacroCode.Else) == false &&
                    this.IsMacroAt(input, codePosition, MacroCode.ElseIfFeminine) == false)
                {
                    return false;
                }

                positions.Add(codePosition);
                input.Position = codePosition + 1;
                var a = input.ReadValueU8();
                var b = input.ReadValueU8();
                branchEnd = input.Position + MacroCodeHelpers.DecodeU16(a, b);
            }
        }
        catch (EndOfStreamException)
        {
            return false;
        }
        finally
        {
            input.Position = oldPosition;
        }
    }
}
