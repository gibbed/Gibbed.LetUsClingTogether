/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.TacticsOgre.FileFormats.Sprite
{
    public struct Palette
    {
        public uint[] Colors;

        public static Palette Read(Stream input, Endian endian)
        {
            var basePosition = input.Position;

            var totalSize = input.ReadValueU32(endian);
            if (basePosition + totalSize > input.Length)
            {
                throw new EndOfStreamException();
            }

            var dataSize = input.ReadValueU32(endian);
            var dataOffset = input.ReadValueU32(endian);
            var unknown0COffset = input.ReadValueU32(endian);

            if (dataOffset != 0x10)
            {
                throw new FormatException();
            }
            else if (dataOffset + dataSize != unknown0COffset)
            {
                throw new FormatException();
            }

            if (dataOffset + dataSize + 32 != totalSize)
            {
                throw new InvalidOperationException();
            }

            if (dataSize % 4 != 0)
            {
                throw new FormatException();
            }

            var colors = new uint[dataSize / 4];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = input.ReadValueU32(endian);
            }

            var unknownTail = input.ReadBytes(32);

            Palette instance;
            instance.Colors = colors;
            return instance;
        }
    }
}
