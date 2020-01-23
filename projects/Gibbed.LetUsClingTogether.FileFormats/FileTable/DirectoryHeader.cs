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

using System.IO;
using Gibbed.IO;

namespace Gibbed.LetUsClingTogether.FileFormats.FileTable
{
    internal struct DirectoryHeader
    {
        public ushort Id;
        public byte Unknown02;
        public byte DataBlockSize;
        public uint DataBaseOffset;
        public byte Unknown08;
        public bool IsInInstallData;
        public ushort BatchCount;
        public ushort NameTableCount;
        public ushort NameTableIndex;
        public uint BatchTableOffset;
        public uint DataInstallBaseOffset;

        public static DirectoryHeader Read(Stream input, Endian endian)
        {
            DirectoryHeader instance;
            instance.Id = input.ReadValueU16(endian);
            instance.Unknown02 = input.ReadValueU8();
            instance.DataBlockSize = input.ReadValueU8();
            instance.DataBaseOffset = input.ReadValueU32(endian);
            instance.Unknown08 = input.ReadValueU8();
            instance.IsInInstallData = input.ReadValueB8();
            instance.BatchCount = input.ReadValueU16(endian);
            instance.NameTableCount = input.ReadValueU16(endian);
            instance.NameTableIndex = input.ReadValueU16(endian);
            instance.BatchTableOffset = input.ReadValueU32(endian);
            instance.DataInstallBaseOffset = input.ReadValueU32(endian);
            return instance;
        }

        public static void Write(Stream output, DirectoryHeader instance, Endian endian)
        {
            output.WriteValueU16(instance.Id, endian);
            output.WriteValueU8(instance.Unknown02);
            output.WriteValueU8(instance.DataBlockSize);
            output.WriteValueU32(instance.DataBaseOffset, endian);
            output.WriteValueU8(instance.Unknown08);
            output.WriteValueB8(instance.IsInInstallData);
            output.WriteValueU16(instance.BatchCount, endian);
            output.WriteValueU16(instance.NameTableCount, endian);
            output.WriteValueU16(instance.NameTableIndex, endian);
            output.WriteValueU32(instance.BatchTableOffset, endian);
            output.WriteValueU32(instance.DataInstallBaseOffset, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }

        public override string ToString()
        {
            return this.IsInInstallData
                ? $"{this.Id}, INSTALLABLE, {this.BatchCount} batches @ {this.BatchTableOffset}"
                : $"{this.Id}, {this.BatchCount} batches @ {this.BatchTableOffset}";
        }
    }
}
