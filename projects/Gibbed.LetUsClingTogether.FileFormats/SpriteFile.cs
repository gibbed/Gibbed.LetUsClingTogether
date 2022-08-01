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
using System.IO;
using Gibbed.IO;

namespace Gibbed.LetUsClingTogether.FileFormats
{
    public class SpriteFile
    {
        public Sprite.Sprite Sprite { get; set; }

        public void Serialize(Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian)
        {
            long basePosition = input.Position;

            var unknown00 = input.ReadValueU32(endian);
            if (unknown00 != 0)
            {
                throw new FormatException();
            }

            var totalSize = input.ReadValueU32(endian);
            if (basePosition + totalSize > input.Length)
            {
                throw new FormatException();
            }

            var unknown08 = input.ReadValueU16(endian); // probably frame count?
            var unknown0A = input.ReadValueU16(endian); // probably header size
            var unknown0C = input.ReadValueU32(endian); // probably version

            if (unknown08 != 1 || unknown0A != 16 || unknown0C != 3)
            {
                throw new FormatException();
            }

            if (input.Position > basePosition + totalSize)
            {
                throw new FormatException();
            }

            var dataSize = input.ReadValueS32();
            input.Seek(-4, SeekOrigin.Current);
            
            if (dataSize < 4 || input.Position + dataSize > basePosition + totalSize)
            {
                throw new FormatException();
            }

            this.Sprite = FileFormats.Sprite.Sprite.Read(input, endian);
        }
    }
}
