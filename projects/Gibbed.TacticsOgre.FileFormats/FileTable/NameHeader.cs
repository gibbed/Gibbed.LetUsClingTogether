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

using System.IO;
using Gibbed.IO;

namespace Gibbed.TacticsOgre.FileFormats.FileTable
{
    internal struct NameHeader
    {
        public uint NameHash;
        public ushort DirectoryId;
        public ushort FileId;

        public static NameHeader Read(Stream input, Endian endian)
        {
            NameHeader instance;
            instance.NameHash = input.ReadValueU32(endian);
            instance.DirectoryId = input.ReadValueU16(endian);
            instance.FileId = input.ReadValueU16(endian);
            return instance;
        }

        public static void Write(Stream output, NameHeader instance, Endian endian)
        {
            output.WriteValueU32(instance.NameHash, endian);
            output.WriteValueU16(instance.DirectoryId, endian);
            output.WriteValueU16(instance.FileId, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }

        public override string ToString()
        {
            return $"{this.NameHash:X8} => {this.DirectoryId} {this.FileId}";
        }
    }
}
