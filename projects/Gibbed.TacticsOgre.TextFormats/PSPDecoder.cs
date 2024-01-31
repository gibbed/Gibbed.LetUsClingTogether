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
using System.Collections.Generic;
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

        public override string Decode(Stream input, Endian endian)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var encoding = this._Encoding;

            StringBuilder output = new();
            List<byte> pendingBytes = new();

            for (; ; )
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

                if (this.Decode((MacroControlCode)b, input, endian, output) == true)
                {
                    break;
                }
            }

            if (pendingBytes.Count > 0)
            {
                var span = encoding.GetString(pendingBytes.ToArray());
                output.Append(span.Replace("{", "{{").Replace("}", "}}"));
            }

            return output.ToString();
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
