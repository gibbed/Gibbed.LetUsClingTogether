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
using System.Text;
using Gibbed.IO;

namespace Gibbed.Reborn.FileFormats
{
    public class TextureFile
    {
        public const uint Signature = 0x00787462; // 'btx\0'

        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public uint Stride { get; set; }
        public TextureFormat Format { get; set; }
        public string Name { get; set; }
        public byte[] DataBytes { get; set; }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            var basePosition = input.Position;

            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var probablyVersion = input.ReadValueU32(endian);
            var width = input.ReadValueU16(endian);
            var height = input.ReadValueU16(endian);
            var stride = input.ReadValueU32(endian);
            var format = (TextureFormat)input.ReadValueU8();
            var unknown11 = input.ReadValueU8();
            var unknown12 = input.ReadValueU8();
            var unknown13 = input.ReadValueU8();
            var unknown14 = input.ReadValueU8();
            var unknown15 = input.ReadValueU8();
            var unknown16 = input.ReadValueU8();
            var unknown17 = input.ReadValueU8();
            var dataOffset = input.ReadValueU32(endian);
            var unknown1C = input.ReadValueU32(endian);
            var unknown20 = input.ReadValueU32(endian);
            var nameOffset = input.ReadValueU32(endian);
            var unknown28 = input.ReadValueU32(endian);
            var unknown2COffset = input.ReadValueU32(endian);
            var unknown30 = input.ReadValueU32(endian);

            if (probablyVersion != 0x0103 ||
                unknown11 != 0 ||
                unknown12 != 0 ||
                unknown13 != 1 ||
                unknown14 != 1 ||
                unknown15 != 5 ||
                unknown16 != 0 ||
                unknown17 != 0 ||
                unknown1C != 255 ||
                unknown28 != 1 ||
                unknown30 != 0)
            {
                throw new FormatException();
            }

            input.Position = basePosition + nameOffset;
            var name = input.ReadStringZ(Encoding.UTF8);

            var (calculatedStride, calculatedRows) = format.CalculateBufferStrideAndRows(width, height, 0);
            if (calculatedStride != stride)
            {
                throw new InvalidOperationException();
            }

            var dataSize = calculatedRows * calculatedStride;
            input.Position = basePosition + dataOffset;
            var dataBytes = input.ReadBytes(dataSize);

            this.Width = width;
            this.Height = height;
            this.Stride = stride;
            this.Format = format;
            this.Name = name;
            this.DataBytes = dataBytes;
        }
    }
}
