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
    public class PSPDecoder : BaseDecoder
    {
        private readonly Encodings.BaseEncoding _Encoding;

        public PSPDecoder(Encodings.BaseEncoding encoding)
        {
            this._Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        protected override Encoding Encoding => this._Encoding;

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
            if (b < 0xFC)
            {
                if ((b & 0xE0) == 0)
                {
                    bytes[1] = input.ReadValueU8();
                    length = 2;
                }
                else
                {
                    length = 1;
                }
                codepoint = default;
                return CodepointType.RawBytes;
            }

            length = 1;
            codepoint = default;
            return b switch
            {
                0xFC => CodepointType.Span,
                0xFD => CodepointType.UnknownFD,
                0xFF => CodepointType.Macro,
                _ => throw new NotSupportedException(),
            };
        }

        public static PSPDecoder ForEN()
        {
            return new(new Encodings.EnglishEncoding());
        }

        public static PSPDecoder ForJP()
        {
            return new(new Encodings.JapaneseEncoding());
        }
    }
}
