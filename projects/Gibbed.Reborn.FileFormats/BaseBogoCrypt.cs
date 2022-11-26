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

namespace Gibbed.Reborn.FileFormats
{
    public abstract class BaseBogoCrypt
    {
        protected static void Enshift(byte[] bytes, int offset, int count, int shift)
        {
            if (count <= 0)
            {
                return;
            }
            var lastByte = bytes[offset + count - 1];
            int leftShift = 8 - shift;
            int o = offset + count - 1;
            for (int i = count - 1; i > 0; i--, o--)
            {
                bytes[o] = (byte)((bytes[o - 1] << leftShift) | (bytes[o] >> shift));
            }
            bytes[o] = (byte)((lastByte << leftShift) | (bytes[o] >> shift));
        }

        protected static void Deshift(byte[] bytes, int offset, int count, int shift)
        {
            if (count <= 0)
            {
                return;
            }
            var firstByte = bytes[offset];
            int rightShift = 8 - shift;
            int o = offset;
            for (int i = 0; i < count - 1; i++, o++)
            {
                bytes[o] = (byte)((bytes[o] << shift) | (bytes[o + 1] >> rightShift));
            }
            bytes[o] = (byte)((bytes[o] << shift) | (firstByte >> rightShift));
        }

        protected static void XorWithTable(byte[] bytes, int offset, int count, byte[] table, int seed)
        {
            if (count <= 0)
            {
                return;
            }
            int tableLength = table.Length >> 2;
            int tableIndex = seed;
            for (int i = 0, o = offset; i + 4 <= count; i += 4, o += 4)
            {
                tableIndex %= tableLength;
                var tableOffset = tableIndex * 4;
                tableIndex++;
                bytes[o + 0] ^= table[tableOffset + 0];
                bytes[o + 1] ^= table[tableOffset + 1];
                bytes[o + 2] ^= table[tableOffset + 2];
                bytes[o + 3] ^= table[tableOffset + 3];
            }
        }

        protected static void XorWithTableReverse(byte[] bytes, int offset, int count, byte[] table, int seed)
        {
            if (count <= 0)
            {
                return;
            }
            int tableLength = table.Length >> 2;
            int tableIndex = seed;
            for (int i = 0, o = offset; i + 4 <= count; i += 4, o += 4)
            {
                tableIndex %= tableLength;
                var tableOffset = tableIndex * 4;
                tableIndex++;
                bytes[o + 0] ^= table[tableOffset + 3];
                bytes[o + 1] ^= table[tableOffset + 2];
                bytes[o + 2] ^= table[tableOffset + 1];
                bytes[o + 3] ^= table[tableOffset + 0];
            }
        }
    }
}
