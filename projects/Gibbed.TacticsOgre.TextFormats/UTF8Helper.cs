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

namespace Gibbed.TacticsOgre.TextFormats
{
    internal static class UTF8Helper
    {
        public static int Length(byte b)
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
            return 0;
        }

        internal static bool DecodeUnsafe(byte[] bytes, int offset, int length, out uint codepoint)
        {
            var b0 = bytes[offset + 0];
            if (length == 1)
            {
                codepoint = b0;
                return true;
            }
            else if (length == 2)
            {
                var b1 = bytes[offset + 1];
                if (b1 >= 0x80 && b1 <= 0xBF &&
                    (b0 & 0x1E) != 0)
                {
                    codepoint =
                        ((uint)(b1 & 0x1F) << 6) |
                        ((uint)(b0 & 0x3F) << 0);
                    return true;
                }
            }
            else if (length == 3)
            {
                var b1 = bytes[offset + 1];
                var b2 = bytes[offset + 2];
                if (b1 >= 0x80 && b1 <= 0xBF &&
                    b2 >= 0x80 && b2 <= 0xBF &&
                    ((b0 & 0x0F) != 0 || (b1 & 0x20) != 0))
                {
                    codepoint =
                        ((uint)(b0 & 0x0F) << 12) |
                        ((uint)(b1 & 0x3F) << 6) |
                        ((uint)(b2 & 0x3F) << 0);
                    return true;
                }
            }
            else if (length == 4)
            {
                var b1 = bytes[offset + 1];
                var b2 = bytes[offset + 2];
                var b3 = bytes[offset + 3];
                if (b1 >= 0x80 && b1 <= 0xBF &&
                    b2 >= 0x80 && b2 <= 0xBF &&
                    b3 >= 0x80 && b3 <= 0xBF &&
                    ((b0 & 0x07) != 0 || (b1 & 0x30) != 0))
                {
                    codepoint =
                        ((uint)(b0 & 0x07) << 18) |
                        ((uint)(b1 & 0x3F) << 12) |
                        ((uint)(b2 & 0x3F) << 6) |
                        ((uint)(b3 & 0x3F) << 0);
                    return true;
                }
            }

            codepoint = default;
            return false;
        }

        public static bool Decode(byte[] bytes, int offset, int count, out int length, out uint codepoint)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            length = Length(bytes[offset]);
            if (length < 0)
            {
                codepoint = default;
                return false;
            }

            if (length > count)
            {
                throw new IndexOutOfRangeException($"codepoint too long for bytes ({length} > {count})");
            }

            return DecodeUnsafe(bytes, offset, length, out codepoint);
        }
    }
}
