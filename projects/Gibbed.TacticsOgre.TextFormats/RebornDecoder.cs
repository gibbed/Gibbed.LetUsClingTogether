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

namespace Gibbed.TacticsOgre.TextFormats
{
    public class RebornDecoder : BaseDecoder
    {
        public override string Decode(Stream input, Endian endian)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var encoding = Encoding.UTF8;
            var decoder = encoding.GetDecoder();

            StringBuilder output = new();

            var bytes = new byte[4];
            var chars = new char[2];
            for (; ; )
            {
                var b = input.ReadValueU8();
                if (b == 0)
                {
                    break;
                }

                bytes[0] = b;
                var byteLength = UTF8Helper.Length(b);
                var bytesRead = input.Read(bytes, 1, byteLength - 1);
                if (bytesRead != byteLength - 1)
                {
                    throw new InvalidOperationException();
                }

                if (UTF8Helper.DecodeUnsafe(bytes, 0, byteLength, out var codepoint) == false)
                {
                    throw new InvalidOperationException();
                }

                // unicode private use areas, with the exception of control codes
                if ((codepoint >= 0x00E000u && codepoint <= 0x00F8FBu/*0x00F8FFu*/) ||
                    (codepoint >= 0x0F0000u && codepoint <= 0x0FFFFDu) ||
                    (codepoint >= 0x100000u && codepoint <= 0x10FFFDu))
                {
                    string gaijiLabel = codepoint switch
                    {
                        0x100000u => "heart",

                        0x10000Du => "enemy",
                        0x10000Eu => "leader",
                        0x10000Fu => "guest",

                        _ => $"0x{codepoint:X}",
                    };
                    output.Append($"{{gaiji {gaijiLabel}}}");
                    continue;
                }

                if (codepoint < 0xF8FCu || codepoint > 0xF8FFu)
                {
                    output.Append(Encoding.UTF8.GetString(bytes, 0, byteLength));
                    continue;
                }

                var controlCode = codepoint switch
                {
                    0xF8FCu => MacroControlCode.Span,
                    0xF8FDu => MacroControlCode.UnknownFD,
                    0xF8FFu => MacroControlCode.Macro,
                    _ => throw new NotSupportedException(),
                };

                if (this.Decode(controlCode, input, endian, output) == true)
                {
                    break;
                }
            }

            return output.ToString();
        }
    }
}
