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
        private static int GetCodepointLength(byte b)
        {
            if ((b & 0xF8) == 0xF0) // b11110xxx
            {
                return 4;
            }
            if ((b & 0xF0) == 0xE0) // b1110xxxx
            {
                return 3;
            }
            if ((b & 0xE0) == 0xC0) // b110xxxxx
            {
                return 2;
            }
            if ((b & 0x80) == 0x00) // b0xxxxxxx
            {
                return 1;
            }
            throw new NotSupportedException();
        }

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
                var byteLength = GetCodepointLength(b);
                var bytesRead = input.Read(bytes, 1, byteLength - 1);
                if (bytesRead != byteLength - 1)
                {
                    throw new InvalidOperationException();
                }

                var s = encoding.GetString(bytes, 0, byteLength);
                if (s.Length > 1)
                {
                    string labelName = s switch
                    {
                        "\U00100000" => "heart",
                        "\U00100001" => "unknown1",

                        "\U0010000B" => "unknown11", // !
                        "\U0010000C" => "unknown12",
                        "\U0010000D" => "enemy",
                        "\U0010000E" => "leader",
                        "\U0010000F" => "guest",
                        _ => throw new NotSupportedException(),
                    };
                    output.Append($"{{gaiji {labelName}}}");
                    continue;
                }

                var c = s[0];
                if (c < '\xF8FC' || c > '\xF8FF')
                {
                    output.Append(c);
                    continue;
                }

                var controlCode = c switch
                {
                    '\xF8FC' => MacroControlCode.Span,
                    '\xF8FD' => MacroControlCode.UnknownFD,
                    '\xF8FF' => MacroControlCode.Macro,
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
