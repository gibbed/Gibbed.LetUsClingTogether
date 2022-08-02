/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
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
using static Gibbed.LetUsClingTogether.FileFormats.InvariantShorthand;

namespace Gibbed.LetUsClingTogether.FileFormats.Text
{
    public class Formatter
    {
        private readonly BaseEncoding _Encoding;

        public Formatter(BaseEncoding encoding)
        {
            this._Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public void Encode(string text, Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public string Decode(Stream input, Endian endian)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var encoding = this._Encoding;

            StringBuilder output = new();
            List<byte> pendingBytes = new();

            for (; ;)
            {
                var b = input.ReadValueU8();
                if (b == 0)
                {
                    break;
                }

                if (b < 0xFC)
                {
                    pendingBytes.Add(b);
                    if ((b & 0xE0) == 0)
                    {
                        b = input.ReadValueU8();
                        pendingBytes.Add(b);
                    }
                    continue;
                }

                if (pendingBytes.Count > 0)
                {
                    var span = encoding.GetString(pendingBytes.ToArray());
                    output.Append(span.Replace("{", "{{").Replace("}", "}}"));
                    pendingBytes.Clear();
                }

                if (b == 0xFC)
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

                    output.Append(Decode(input, endian));
                    break;
                }
                else if (b == 0xFD)
                {
                    var unknown1 = input.ReadValueU8();
                    output.Append(_($"{{unk#FD {unknown1}}}"));
                }
                else if (b == 0xFF)
                {
                    var opcode = (FormatOpcode)input.ReadValueU8();
                    if (opcode == FormatOpcode.InsertNewline)
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
                        DecodeFormatOpcode(input, output, opcode);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (pendingBytes.Count > 0)
            {
                var span = encoding.GetString(pendingBytes.ToArray());
                output.Append(span.Replace("{", "{{").Replace("}", "}}"));
            }

            return output.ToString();
        }

        private static void DecodeFormatOpcode(
            Stream stream, StringBuilder output, FormatOpcode opcode)
        {
            switch (opcode)
            {
                case FormatOpcode.IndicateWrapArea:
                {
                    var width = stream.ReadValueU8();
                    var unknown = stream.ReadValueU8();
                    //output.Append(_($"{{wrap {width} {unknown}}}"));
                    return;
                }

                case FormatOpcode.SetTextColor:
                {
                    var r = stream.ReadValueU8();
                    var g = stream.ReadValueS8();
                    var b = stream.ReadValueU8();
                    var a = stream.ReadValueU8();
                    r--;
                    g--;
                    b--;
                    a--;
                    output.Append(_($"{"{"}#{r:X2}{g:X2}{b:X2}{a:X2}{"}"}"));
                    return;
                }

                case FormatOpcode.Unknown83:
                {
                    var unknown1 = stream.ReadValueU8();
                    var unknown2 = stream.ReadValueU8();
                    var unknown3 = stream.ReadValueS8();
                    var unknown4 = stream.ReadValueU8();
                    var unknown5 = stream.ReadValueU8();
                    output.Append(_($"{{var str {unknown1} {unknown2} {unknown3} {unknown4} {unknown5}}}"));
                    return;
                }

                case FormatOpcode.Unknown84:
                {
                    var unknown1 = stream.ReadValueU8();
                    var unknown2 = stream.ReadValueS8();
                    var unknown3 = stream.ReadValueU8();
                    output.Append(_($"{{var num {unknown1} {unknown2} {unknown3}}}"));
                    return;
                }

                case FormatOpcode.Unknown85:
                {
                    var unknown1 = stream.ReadValueU8();
                    var unknown2 = stream.ReadValueU8();
                    output.Append(_($"{{unk#85 {unknown1} {unknown2}}}"));
                    return;
                }

                case FormatOpcode.InsertPageBreak:
                {
                    output.Append("{pp}\n");
                    return;
                }

                case FormatOpcode.InsertPixelSpacing:
                {
                    var width = stream.ReadValueU8();
                    output.Append(_($"{{space {width}}}"));
                    return;
                }

                case FormatOpcode.Unknown89:
                {
                    var unknown = stream.ReadValueU8();
                    output.Append(_($"{{unk#89 {unknown}}}"));
                    return;
                }

                case FormatOpcode.InsertIcon:
                {
                    var index = stream.ReadValueU8();
                    output.Append(_($"{{icon {index}}}"));
                    return;
                }

                case FormatOpcode.InsertSelect:
                {
                    var defaultOption = stream.ReadValueU8();
                    output.Append(defaultOption == 1
                        ? "{select}\n"
                        : _($"{{select {defaultOption}}}\n"));
                    return;
                }

                case FormatOpcode.Unknown8D:
                {
                    var unknown1 = stream.ReadValueU8();
                    var unknown2 = stream.ReadValueU8();
                    output.Append(_($"{{unk#8D {unknown1} {unknown2}}}"));
                    return;
                }

                case FormatOpcode.Unknown92:
                {
                    output.Append("{unk#92}");
                    return;
                }

                case FormatOpcode.ZombieCondition:
                {
                    var value = stream.ReadValueU8() - 1;
                    var length1 = stream.ReadValueU8();
                    var length2 = stream.ReadValueU8();
                    var length = ((length1 * 0xFF) + length2 - 0x100) & 0xFFFF;

                    if (true)
                    {
                        var oldPosition = stream.Position;
                        stream.Position += length - 2;
                        var check1 = stream.ReadValueU8();
                        var check2 = stream.ReadValueU8();
                        if (check1 != 0xFF || check2 != 0x96)
                        {
                            throw new FormatException();
                        }
                        stream.Position = oldPosition;
                    }

                    //output.Append(_($"{{zombie {value} {length}}}"));
                    output.Append(_($"{{zombie {value}}}"));
                    return;
                }

                case FormatOpcode.ZombieConditionEnd:
                {
                    output.Append("{/zombie}");
                    return;
                }

                case FormatOpcode.Unknown98:
                {
                    var unknown1 = stream.ReadValueU8();
                    var unknown2 = stream.ReadValueU8();
                    output.Append(_($"{{unk#98 {unknown1} {unknown2}}}"));
                    return;
                }

                case FormatOpcode.Unknown99:
                {
                    var unknown1 = stream.ReadValueU8();
                    var unknown2 = stream.ReadValueU8();
                    output.Append(_($"{{unk#99 {unknown1} {unknown2}}}"));
                    return;
                }

                case FormatOpcode.InsertHeroName:
                {
                    output.Append("{hero name}");
                    return;
                }

                case FormatOpcode.InsertOrderName:
                {
                    output.Append("{order name}");
                    return;
                }

                case FormatOpcode.Unknown9A:
                {
                    var unknown = stream.ReadValueU8();
                    output.Append(_($"{{unk#9A {unknown}}}"));
                    return;
                }

                case FormatOpcode.Unknown9C:
                {
                    var unknown1 = stream.ReadValueU8();
                    var unknown2 = stream.ReadValueU8();
                    output.Append(_($"{{unk#9C {unknown1} {unknown2}}}"));
                    return;
                }

                case FormatOpcode.SetFontPalette:
                {
                    var index = stream.ReadValueU8();
                    output.Append(_($"{{pal {index}}}"));
                    return;
                }

                case FormatOpcode.ResetFontPalette:
                {
                    output.Append("{pal reset}");
                    return;
                }

                case FormatOpcode.UppercaseFollowingCharacter:
                {
                    output.Append("{uc}");
                    return;
                }

                case FormatOpcode.MaybeNewPage:
                {
                    output.Append("{np}");
                    return;
                }
            }

            throw new NotImplementedException();
        }

        public static Formatter ForEN()
        {
            return new(new EnglishEncoding());
        }

        public static Formatter ForJP()
        {
            return new(new JapaneseEncoding());
        }
    }
}
