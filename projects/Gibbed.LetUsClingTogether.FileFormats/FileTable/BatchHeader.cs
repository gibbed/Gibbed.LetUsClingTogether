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

namespace Gibbed.LetUsClingTogether.FileFormats.FileTable
{
    internal struct BatchHeader
    {
        public ushort BaseFileId;
        public ushort FileCount;
        public int FileTableOffset;
        public BatchFlags Flags;

        public static BatchHeader Read(Stream input, Endian endian)
        {
            BatchHeader instance;
            instance.BaseFileId = input.ReadValueU16(endian);
            instance.FileCount = input.ReadValueU16(endian);
            var rawFlags = input.ReadValueU32(endian);
            instance.FileTableOffset = (int)(rawFlags & 0x1FFFFFFF);
            instance.Flags = (BatchFlags)((rawFlags >> 29) & 7);
            return instance;
        }

        public static void Write(Stream output, BatchHeader instance, Endian endian)
        {
            if (instance.FileTableOffset > 0x1FFFFFFF)
            {
                throw new ArgumentOutOfRangeException("file table offset too large", nameof(instance));
            }

            output.WriteValueU16(instance.BaseFileId, endian);
            output.WriteValueU16(instance.FileCount, endian);
            uint rawFlags = (uint)instance.FileTableOffset;
            rawFlags |= (((uint)instance.Flags) & 7) << 29;
            output.WriteValueU32(rawFlags, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }

        public override string ToString()
        {
            return this.Flags == BatchFlags.None
                ? $"{this.BaseFileId} => {this.BaseFileId + this.FileCount - 1}"
                : $"{this.BaseFileId} => {this.BaseFileId + this.FileCount - 1} | {this.Flags}";
        }
    }
}
