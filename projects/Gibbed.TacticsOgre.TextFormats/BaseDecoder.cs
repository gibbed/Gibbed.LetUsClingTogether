/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
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
using System.IO;
using System.Text;
using Gibbed.IO;
using static Gibbed.TacticsOgre.Extensions.InvariantShorthand;

namespace Gibbed.TacticsOgre.TextFormats
{
    public abstract class BaseDecoder : IDecoder
    {
        protected abstract Encoding Encoding { get; }

        protected enum CodepointType
        {
            Null,
            RawBytes,
            Gaiji,
            // macro control codes
            Span,
            UnknownFD,
            UnknownFE,
            Macro,
        }

        protected abstract CodepointType Decode(Stream input, Endian endian, byte[] bytes, out int length, out uint codepoint);

        public string Decode(Stream input, Endian endian)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var encoding = this.Encoding;

            var pendingBytes = new byte[128];
            var pendingLength = pendingBytes.Length;
            int pendingIndex = 0;

            StringBuilder output = new();

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

            var bytes = new byte[4];
            for (; ; )
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

                if (type == CodepointType.RawBytes)
                {
                    Array.Copy(bytes, 0, pendingBytes, pendingIndex, length);
                    pendingIndex += length;
                    continue;
                }

                if (type == CodepointType.Gaiji)
                {
                    // TODO(gibbed): move these to be per platform/encoding
                    string gaijiLabel = codepoint switch
                    {
                        // PSP

                        0x000391u => "enemy_1",
                        0x000392u => "enemy_2",
                        0x000393u => "enemy_3",
                        0x000394u => "enemy_4",
                        0x000395u => "leader_1",
                        0x000396u => "leader_2",
                        0x000397u => "leader_3",
                        0x000398u => "leader_4",
                        0x000399u => "guest_1",
                        0x00039Au => "guest_2",
                        0x00039Bu => "guest_3",
                        0x00039Cu => "guest_4",

                        0x002469u => "ai_1",
                        0x00246Au => "ai_2",

                        // Reborn

                        0x100000u => "heart",

                        0x10000Bu => "ai",

                        0x10000Du => "enemy",
                        0x10000Eu => "leader",
                        0x10000Fu => "guest",

                        _ => $"0x{codepoint:X}",
                    };
                    output.Append($"{{gaiji {gaijiLabel}}}");
                    continue;
                }

                if (this.Decode(type, input, endian, output) == true)
                {
                    break;
                }
            }

            FlushPending();

            return output.ToString();
        }

        protected bool Decode(CodepointType type, Stream input, Endian endian, StringBuilder output)
        {
            if (type == CodepointType.Span)
            {
                var flags = input.ReadValueU8();
                //var width = stream.ReadValueU16(endian);
                input.Seek(2, SeekOrigin.Current);
                var count = input.ReadValueU8();
                var unknown = input.ReadValueU8();

                if (unknown != 0 && unknown != 8 &&
                    unknown != 21 &&
                    unknown != 16 && unknown != 24 && unknown != 32 &&
                    unknown != 40 && unknown != 48 && unknown != 56 &&
                    unknown != 64 && unknown != 72 && unknown != 80 &&
                    unknown != 104 && unknown != 120 && unknown != 128 &&
                    unknown != 136 && unknown != 144 && unknown != 152)
                {
                    throw new InvalidOperationException();
                }

                if ((flags & 1) != 0)
                {
                    byte unknown2;
                    do
                    {
                        unknown2 = input.ReadValueU8();
                        var unknown3 = input.ReadValueU8();
                        var unknown4 = input.ReadString(4, true, Encoding.ASCII);
                        output.Append(_($"{{var ref {unknown2} {unknown3} {unknown4}}}"));
                    }
                    while ((unknown2 & 1) != 0);
                }

                output.Append(this.Decode(input, endian));
                return true;
            }
            else if (type == CodepointType.UnknownFD)
            {
                var unknown1 = input.ReadValueU8();
                output.Append(_($"{{unk#FD {unknown1}}}"));
                return false;
            }
            else if (type == CodepointType.Macro)
            {
                var opcode = (MacroCode)input.ReadValueU8();
                if (opcode == MacroCode.InsertNewline)
                {
                    output.Append("\n");
                }
                /*else if (includeTags == false)
                {
                    var size = opcode.GetSize() - 1;
                    if (size > 0)
                    {
                        input.Seek(size, SeekOrigin.Current);
                    }
                }*/
                else
                {
                    Decode(opcode, input, endian, output);
                }
                return false;
            }
            throw new NotImplementedException();
        }

        private void Decode(MacroCode code, Stream input, Endian endian, StringBuilder output)
        {
            switch (code)
            {
                case MacroCode.IndicateWrapArea:
                {
                    var width = input.ReadValueU8();
                    var unknown = input.ReadValueU8();
                    //output.Append(_($"{{wrap {width} {unknown}}}"));
                    return;
                }

                case MacroCode.SetTextColor:
                {
                    var r = input.ReadValueU8();
                    var g = input.ReadValueS8();
                    var b = input.ReadValueU8();
                    var a = input.ReadValueU8();
                    r--;
                    g--;
                    b--;
                    a--;
                    output.Append("{");
                    output.Append(_($"#{r:X2}{g:X2}{b:X2}{a:X2}"));
                    output.Append("}");
                    return;
                }

                case MacroCode.Unknown83:
                {
                    var unknown1 = input.ReadValueU8();
                    var unknown2 = input.ReadValueU8();
                    var unknown3 = input.ReadValueS8();
                    var unknown4 = input.ReadValueU8();
                    var unknown5 = input.ReadValueU8();
                    output.Append(_($"{{var str {unknown1} {unknown2} {unknown3} {unknown4} {unknown5}}}"));
                    return;
                }

                case MacroCode.Unknown84:
                {
                    var unknown1 = input.ReadValueU8();
                    var unknown2 = input.ReadValueS8();
                    var unknown3 = input.ReadValueU8();
                    if (this is RebornDecoder)
                    {
                        var unknown4 = input.ReadValueU8();
                        output.Append(_($"{{var num {unknown1} {unknown2} {unknown3} {unknown4}}}"));
                    }
                    else
                    {
                        output.Append(_($"{{var num {unknown1} {unknown2} {unknown3}}}"));
                    }
                    return;
                }

                case MacroCode.Unknown85:
                {
                    var unknown1 = input.ReadValueU8();
                    var unknown2 = input.ReadValueU8();
                    output.Append(_($"{{unk#85 {unknown1} {unknown2}}}"));
                    return;
                }

                case MacroCode.InsertPageBreak:
                {
                    output.Append("{pp}\n");
                    return;
                }

                case MacroCode.InsertPixelSpacing:
                {
                    var width = input.ReadValueU8();
                    output.Append(_($"{{space {width}}}"));
                    return;
                }

                case MacroCode.Unknown89:
                {
                    var unknown = input.ReadValueU8();
                    output.Append(_($"{{unk#89 {unknown}}}"));
                    return;
                }

                case MacroCode.InsertIcon:
                {
                    var index = input.ReadValueU8();
                    output.Append(_($"{{icon {index}}}"));
                    return;
                }

                case MacroCode.InsertSelect:
                {
                    var defaultOption = input.ReadValueU8();
                    output.Append(defaultOption == 1
                        ? "{select}\n"
                        : _($"{{select {defaultOption}}}\n"));
                    return;
                }

                case MacroCode.Unknown8D:
                {
                    var unknown1 = input.ReadValueU8();
                    var unknown2 = input.ReadValueU8();
                    output.Append(_($"{{unk#8D {unknown1} {unknown2}}}"));
                    return;
                }

                case MacroCode.Unknown92:
                {
                    output.Append("{unk#92}");
                    return;
                }

                case MacroCode.ZombieCondition:
                {
                    var value = input.ReadValueU8() - 1;
                    var length1 = input.ReadValueU8();
                    var length2 = input.ReadValueU8();
                    var length = ((length1 * 0xFF) + length2 - 0x100) & 0xFFFF;

                    if (true)
                    {
                        var oldPosition = input.Position;
                        if (this is RebornDecoder)
                        {
                            input.Position += length - 4;
                            var check1 = input.ReadValueU8();
                            var check2 = input.ReadValueU8();
                            var check3 = input.ReadValueU8();
                            var check4 = input.ReadValueU8();
                            if (check1 != 0xEF || check2 != 0xA3 || check3 != 0xBF || check4 != 0x96)
                            {
                                throw new FormatException();
                            }
                        }
                        else
                        {
                            input.Position += length - 2;
                            var check1 = input.ReadValueU8();
                            var check2 = input.ReadValueU8();
                            if (check1 != 0xFF || check2 != 0x96)
                            {
                                throw new FormatException();
                            }
                        }
                        input.Position = oldPosition;
                    }

                    //output.Append(_($"{{zombie {value} {length}}}"));
                    output.Append(_($"{{zombie {value}}}"));
                    return;
                }

                case MacroCode.ZombieConditionEnd:
                {
                    output.Append("{/zombie}");
                    return;
                }

                case MacroCode.Unknown98:
                {
                    var unknown1 = input.ReadValueU8();
                    var unknown2 = input.ReadValueU8();
                    output.Append(_($"{{unk#98 {unknown1} {unknown2}}}"));
                    return;
                }

                case MacroCode.Unknown99:
                {
                    var unknown1 = input.ReadValueU8();
                    var unknown2 = input.ReadValueU8();
                    output.Append(_($"{{unk#99 {unknown1} {unknown2}}}"));
                    return;
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

                case MacroCode.Unknown9A:
                {
                    var unknown = input.ReadValueU8();
                    output.Append(_($"{{unk#9A {unknown}}}"));
                    return;
                }

                case MacroCode.Unknown9C:
                {
                    var unknown1 = input.ReadValueU8();
                    var unknown2 = input.ReadValueU8();
                    output.Append(_($"{{unk#9C {unknown1} {unknown2}}}"));
                    return;
                }

                case MacroCode.SetFontPalette:
                {
                    var index = input.ReadValueU8();
                    output.Append(_($"{{pal {index}}}"));
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

                case MacroCode.MaybeNewPage:
                {
                    output.Append("{np}");
                    return;
                }

                case MacroCode.RebornUnknownAA:
                {
                    var unknown = input.ReadValueU8();
                    output.Append(_($"{{unk#AA {unknown}}}"));
                    return;
                }

                case MacroCode.RebornUnknownB0:
                {
                    output.Append("{unk#B0}");
                    return;
                }

                case MacroCode.RebornUnknownB1:
                {
                    output.Append("{unk#B1}");
                    return;
                }

                case MacroCode.RebornUnknownB2:
                {
                    var unknown1 = input.ReadValueU8() - 1;
                    var unknown2 = input.ReadValueU8() - 1;
                    var unknown3 = input.ReadValueU8() - 1;
                    output.Append(_($"{{symbol {unknown1} {unknown2} {unknown3}}}"));
                    return;
                }

                case MacroCode.RebornUnknownB3:
                {
                    var unknown1 = input.ReadValueU8() - 1;
                    var unknown2 = input.ReadValueU8() - 1;
                    var unknown3 = input.ReadValueU8() - 1;
                    output.Append(_($"{{button {unknown1} {unknown2} {unknown3}}}"));
                    return;
                }
            }

            throw new NotImplementedException();
        }
    }
}
