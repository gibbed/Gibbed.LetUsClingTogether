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

namespace Gibbed.Reborn.FileFormats
{
    public sealed class DataBogoCrypt : BaseBogoCrypt
    {
        public static void Encrypt(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (count == 0)
            {
                return;
            }
            if (offset < 0 || offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (count < 16 || offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            XorWithTable(bytes, offset + 16, count - 16, bytes[offset + 15] & 0x7F, Table3);
            XorWithTable(bytes, offset, count, 8, Table2);
            Enshift(bytes, offset, count, 3);
        }

        public static void Decrypt(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (count == 0)
            {
                return;
            }
            if (offset < 0 || offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (count < 16 || offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            Deshift(bytes, offset, count, 3);
            XorWithTable(bytes, offset, count, 8, Table2);
            XorWithTable(bytes, offset + 16, count - 16, bytes[offset + 15] & 0x7F, Table3);
        }
    }
}
