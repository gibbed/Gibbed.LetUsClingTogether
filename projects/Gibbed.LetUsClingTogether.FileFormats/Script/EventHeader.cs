﻿/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.LetUsClingTogether.FileFormats.Script
{
    public struct EventHeader
    {
        public uint NameOffset;
        public ushort TableIndex;
        public ushort Unknown06;
        public uint CodeOffset;
        public int CodeCount;
        public uint FunctionTableOffset;
        public uint JumpTableOffset;
        public uint Unknown18Offset;
        public uint Unknown1C;
        public ushort Unknown20;
        public ushort Index;
        public uint Unknown24;
        public uint Unknown28;
        public uint Unknown2C;

        public static EventHeader Read(Stream input, Endian endian)
        {
            EventHeader instance;
            instance.NameOffset = input.ReadValueU32(endian);
            instance.TableIndex = input.ReadValueU16(endian);
            instance.Unknown06 = input.ReadValueU16(endian);
            instance.CodeOffset = input.ReadValueU32(endian);
            instance.CodeCount = input.ReadValueS32(endian);
            instance.FunctionTableOffset = input.ReadValueU32(endian);
            instance.JumpTableOffset = input.ReadValueU32(endian);
            instance.Unknown18Offset = input.ReadValueU32(endian);
            instance.Unknown1C = input.ReadValueU32(endian);
            instance.Unknown20 = input.ReadValueU16(endian);
            instance.Index = input.ReadValueU16(endian);
            instance.Unknown24 = input.ReadValueU32(endian);
            instance.Unknown28 = input.ReadValueU32(endian);
            instance.Unknown2C = input.ReadValueU32(endian);

            if (instance.Unknown06 != 255 ||
                instance.Unknown20 != 0 ||
                instance.Unknown24 != 0 ||
                instance.Unknown28 != 0 ||
                instance.Unknown2C != 0)
            {
                throw new NotSupportedException();
            }
            /*if (instance.Unknown1C != 0 &&
                instance.Unknown1C != 4 &&
                instance.Unknown1C != 8 &&
                instance.Unknown1C != 12 &&
                instance.Unknown1C != 16 &&
                instance.Unknown1C != 20 &&
                instance.Unknown1C != 24 &&
                instance.Unknown1C != 32)
            {
                throw new FormatException();
            }*/
            return instance;
        }

        public override string ToString()
        {
            return $"{Unknown1C}";
        }
    }
}