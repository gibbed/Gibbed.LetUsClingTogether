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

namespace Gibbed.TacticsOgre.ScriptFormats
{
    public struct ScriptFileHeader
    {
        public const uint SignatureValue = 0x80000000u;
        public const uint SignatureMask = 0xFFFF0000u;
        public const uint VersionMask = 0x0000FFFFu;

        public const uint ExpectedVersion = 0x000C;

        public ushort Version;
        public Endian Endian;
        public uint TotalSize;
        public uint AuthorNameOffset;
        public uint SourceNameOffset;
        public uint DateOffset;
        public uint ScriptCountTableOffset;
        public uint ScriptTableOffset;
        public uint Unknown1C;
        public uint IntTableOffset;
        public uint FloatTableOffset;
        public uint VariableTableOffset;
        public uint Unknown2COffset;
        public uint RequestTablesOffset;
        public uint FrameDataSize;
        public uint FrameDataOffset;
        public uint Unknown3C;
        public uint Unknown40Offset;
        public uint Unknown44Offset;
        public uint Unknown48Offset;
        public uint StringTableOffset;
        public short Unknown50;
        public short Unknown52;
        public uint Unknown54Offset;
        public uint Unknown58;
        public uint Unknown5COffset;
        public float Unknown60;

        public static ScriptFileHeader Read(Stream input)
        {
            var magicAndVersion = input.ReadValueU32(Endian.Little);
            ushort version;
            if ((magicAndVersion & SignatureMask) == SignatureValue)
            {
                version = (ushort)(magicAndVersion & VersionMask);
            }
            else if ((magicAndVersion.Swap() & SignatureMask) == SignatureValue)
            {
                version = (ushort)(magicAndVersion.Swap() & VersionMask);
            }
            else
            {
                throw new FormatException();
            }
            if (version != ExpectedVersion)
            {
                throw new FormatException();
            }
            var endian = (magicAndVersion & SignatureMask) == SignatureValue
                ? Endian.Little
                : Endian.Big;

            ScriptFileHeader instance;
            instance.Version = version;
            instance.Endian = endian;
            instance.TotalSize = input.ReadValueU32(endian);
            instance.AuthorNameOffset = input.ReadValueU32(endian);
            instance.SourceNameOffset = input.ReadValueU32(endian);
            instance.DateOffset = input.ReadValueU32(endian);
            instance.ScriptCountTableOffset = input.ReadValueU32(endian);
            instance.ScriptTableOffset = input.ReadValueU32(endian);
            instance.Unknown1C = input.ReadValueU32(endian);
            instance.IntTableOffset = input.ReadValueU32(endian);
            instance.FloatTableOffset = input.ReadValueU32(endian);
            instance.VariableTableOffset = input.ReadValueU32(endian);
            instance.Unknown2COffset = input.ReadValueU32(endian);
            instance.RequestTablesOffset = input.ReadValueU32(endian);
            instance.FrameDataSize = input.ReadValueU32(endian);
            instance.FrameDataOffset = input.ReadValueU32(endian);
            instance.Unknown3C = input.ReadValueU32(endian);
            instance.Unknown40Offset = input.ReadValueU32(endian);
            instance.Unknown44Offset = input.ReadValueU32(endian);
            instance.Unknown48Offset = input.ReadValueU32(endian);
            instance.StringTableOffset = input.ReadValueU32(endian);
            instance.Unknown50 = input.ReadValueS16(endian);
            instance.Unknown52 = input.ReadValueS16(endian);
            instance.Unknown54Offset = input.ReadValueU32(endian);
            instance.Unknown58 = input.ReadValueU32(endian);
            instance.Unknown5COffset = input.ReadValueU32(endian);
            instance.Unknown60 = input.ReadValueF32(endian);
            if (instance.Unknown1C != 0 ||
                instance.Unknown3C != 0 ||
                instance.Unknown50 != -1 ||
                instance.Unknown52 != 0 ||
                instance.Unknown58 != 0 ||
                instance.Unknown60.Equals(0.0f) == false)
            {
                throw new FormatException();
            }

            long expectedUnknown40Offset = instance.FrameDataOffset + instance.FrameDataSize switch
            {
                0 => 0x00,
                4 => 0x40,
                8 => 0x40,
                12 => 0x40,
                20 => 0x40,
                108 => 0x80,
                124 => 0x80,
                _ => throw new NotSupportedException(),
            };

            if (instance.Unknown40Offset != expectedUnknown40Offset)
            {
                throw new FormatException();
            }

            return instance;
        }
    }
}
