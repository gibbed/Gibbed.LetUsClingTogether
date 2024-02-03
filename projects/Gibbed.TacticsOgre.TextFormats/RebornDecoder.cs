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
        protected override Encoding Encoding => Encoding.UTF8;

        protected override CodepointType Decode(Stream input, Endian endian, byte[] bytes, out int length, out uint codepoint)
        {
            var b = input.ReadValueU8();
            if (b == 0)
            {
                length = default;
                codepoint = default;
                return CodepointType.Null;
            }

            bytes[0] = b;
            length = UTF8Helper.Length(b);
            if (length < 1)
            {
                throw new InvalidOperationException();
            }
            var read = input.Read(bytes, 1, length - 1);
            if (read != length - 1)
            {
                throw new InvalidOperationException();
            }

            if (UTF8Helper.DecodeUnsafe(bytes, 0, length, out codepoint) == false)
            {
                throw new InvalidOperationException();
            }

            // unicode private use areas, with the exception of control codes
            if ((codepoint >= 0x00E000u && codepoint <= 0x00F8FBu/*0x00F8FFu*/) ||
                (codepoint >= 0x0F0000u && codepoint <= 0x0FFFFDu) ||
                (codepoint >= 0x100000u && codepoint <= 0x10FFFDu))
            {
                return CodepointType.Gaiji;
            }

            if (codepoint < 0xF8FCu || codepoint > 0xF8FFu)
            {
                return CodepointType.RawBytes;
            }

            return codepoint switch
            {
                0xF8FCu => CodepointType.Span,
                0xF8FDu => CodepointType.UnknownFD,
                0xF8FFu => CodepointType.Macro,
                _ => throw new NotSupportedException(),
            };
        }
    }
}
