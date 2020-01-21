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

namespace Gibbed.TacticsOgre.FileFormats.FileTable
{
    public struct BatchHeader
    {
        public ushort BaseFileId;
        public ushort FileCount;
        public ushort FileTableOffset;
        public BatchFlags Flags;

        public static BatchHeader Read(Stream input, Endian endian)
        {
            BatchHeader instance;
            instance.BaseFileId = input.ReadValueU16(endian);
            instance.FileCount = input.ReadValueU16(endian);
            instance.FileTableOffset = input.ReadValueU16(endian);
            var rawFlags = input.ReadValueU16(endian);
            instance.Flags = (BatchFlags)rawFlags;
            if ((rawFlags & (~24576)) != 0)
            {
                throw new NotSupportedException("unknown directory flags");
            }
            return instance;
        }

        public override string ToString()
        {
            return $"{this.BaseFileId}=>{this.BaseFileId+this.FileCount-1}";
        }
    }
}
