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

namespace Gibbed.TacticsOgre.FileFormats.FileTable
{
    public struct DirectoryHeader
    {
        public ushort Id;
        public ushort Unknown02;
        public ushort Unknown04;
        public ushort Unknown06;
        public ushort Unknown08;
        public ushort BatchCount;
        public ushort Unknown0C;
        public short Unknown0E;
        public uint BatchTableOffset;
        public uint Unknown14;

        public static DirectoryHeader Read(Stream input, Endian endian)
        {
            DirectoryHeader instance;
            instance.Id = input.ReadValueU16(endian);
            instance.Unknown02 = input.ReadValueU16(endian);
            instance.Unknown04 = input.ReadValueU16(endian);
            instance.Unknown06 = input.ReadValueU16(endian);
            instance.Unknown08 = input.ReadValueU16(endian);
            instance.BatchCount = input.ReadValueU16(endian);
            instance.Unknown0C = input.ReadValueU16(endian);
            instance.Unknown0E = input.ReadValueS16(endian);
            instance.BatchTableOffset = input.ReadValueU32(endian);
            instance.Unknown14 = input.ReadValueU32(endian);
            return instance;
        }

        public static void Write(Stream output, DirectoryHeader instance, Endian endian)
        {
            output.WriteValueU16(instance.Id, endian);
            output.WriteValueU16(instance.Unknown02, endian);
            output.WriteValueU16(instance.Unknown04, endian);
            output.WriteValueU16(instance.Unknown06, endian);
            output.WriteValueU16(instance.Unknown08, endian);
            output.WriteValueU16(instance.BatchCount, endian);
            output.WriteValueU16(instance.Unknown0C, endian);
            output.WriteValueS16(instance.Unknown0E, endian);
            output.WriteValueU32(instance.BatchTableOffset, endian);
            output.WriteValueU32(instance.Unknown14, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }
    }
}
