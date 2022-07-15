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

namespace Gibbed.LetUsClingTogether.ScriptFormats
{
    public struct FileHeader
    {
        public const uint SignatureValue = 0x80000000u;
        public const uint SignatureMask = 0xFFFF0000u;
        public const uint VersionMask = 0x0000FFFFu;

        public const uint ExpectedVersion = 0x000C;

        public Endian Endian;
        public uint TotalSize;
        public uint AuthorNameOffset;
        public uint SourceNameOffset;
        public uint MaybeSourceVersionOffset;
        public uint EventCountTableOffset;
        public uint EventTableOffset;
        public uint Unknown1C;
        public uint Unknown20Offset;
        public uint Unknown24Offset;
        public uint Unknown28Offset;
        public uint Unknown2COffset;
        public uint Unknown30Offset;
        public uint Unknown34;
        public uint Unknown38Offset;
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

        public static FileHeader Read(Stream input)
        {
            var magicAndVersion = input.ReadValueU32(Endian.Little);
            if ((magicAndVersion & SignatureMask) != SignatureValue &&
                (magicAndVersion.Swap() & SignatureMask) != SignatureValue)
            {
                throw new FormatException();
            }
            var endian = (magicAndVersion & SignatureMask) == SignatureValue
                ? Endian.Little
                : Endian.Big;

            FileHeader instance;
            instance.Endian = endian;
            instance.TotalSize = input.ReadValueU32(endian);
            instance.AuthorNameOffset = input.ReadValueU32(endian);
            instance.SourceNameOffset = input.ReadValueU32(endian);
            instance.MaybeSourceVersionOffset = input.ReadValueU32(endian);
            instance.EventCountTableOffset = input.ReadValueU32(endian);
            instance.EventTableOffset = input.ReadValueU32(endian);
            instance.Unknown1C = input.ReadValueU32(endian);
            instance.Unknown20Offset = input.ReadValueU32(endian);
            instance.Unknown24Offset = input.ReadValueU32(endian);
            instance.Unknown28Offset = input.ReadValueU32(endian);
            instance.Unknown2COffset = input.ReadValueU32(endian);
            instance.Unknown30Offset = input.ReadValueU32(endian);
            instance.Unknown34 = input.ReadValueU32(endian);
            instance.Unknown38Offset = input.ReadValueU32(endian);
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
            if (instance.Unknown34 != 0 &&
                instance.Unknown34 != 4 &&
                instance.Unknown34 != 8 &&
                instance.Unknown34 != 12 &&
                instance.Unknown34 != 20 &&
                instance.Unknown34 != 108 &&
                instance.Unknown34 != 124)
            {
                throw new FormatException();
            }
            if (instance.Unknown38Offset == 0x80)
            {
                if (instance.Unknown34 == 0)
                {
                    if (instance.Unknown40Offset != 0x80)
                    {
                        throw new FormatException();
                    }
                }
                else if (instance.Unknown34 < 108)
                {
                    if (instance.Unknown40Offset != 0xC0)
                    {
                        throw new FormatException();
                    }
                }
                else
                {
                    if (instance.Unknown40Offset != 0x100)
                    {
                        throw new FormatException();
                    }
                }
            }
            else if (instance.Unknown38Offset == 0xC0)
            {
                if (instance.Unknown34 == 0)
                {
                    if (instance.Unknown40Offset != 0xC0)
                    {
                        throw new FormatException();
                    }
                }
                else if (instance.Unknown34 < 108)
                {
                    if (instance.Unknown40Offset != 0x100)
                    {
                        throw new FormatException();
                    }
                }
                else
                {
                    if (instance.Unknown40Offset != 0x140)
                    {
                        throw new FormatException();
                    }
                }
            }
            else
            {
                throw new FormatException();
            }
            return instance;
        }
    }
}
