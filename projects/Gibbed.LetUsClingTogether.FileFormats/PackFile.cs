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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;

namespace Gibbed.LetUsClingTogether.FileFormats
{
    public class PackFile
    {
        public const uint Signature = 0x646B6170; // 'pakd'

        public PackFile()
        {
            this.Entries = new List<Entry>();
        }

        public Endian Endian { get; set; }
        public List<Entry> Entries { get; }
        public uint TotalSize { get; set; }

        public static int GetHeaderSize(int count, bool hasIds)
        {
            var size = 4 + 4 + (count * 4) + 4;
            if (hasIds == true)
            {
                size += count * 4;
            }
            size += 4;
            return size.Align(16);
        }

        public int GetHeaderSize()
        {
            return GetHeaderSize(this.Entries.Count, this.Entries.Any(e => e.RawId != 0));
        }

        public void Serialize(Stream output)
        {
            var endian = this.Endian;
            output.WriteValueU32(Signature, endian);
            output.WriteValueS32(this.Entries.Count, endian);
            bool hasIds = false;
            foreach (var entry in this.Entries)
            {
                if (entry.RawId != 0)
                {
                    hasIds = true;
                }
                output.WriteValueU32(entry.Offset, endian);
            }
            output.WriteValueU32(this.TotalSize, endian);
            if (hasIds == true)
            {
                foreach (var entry in this.Entries)
                {
                    output.WriteValueU32(entry.RawId, endian);
                }
            }
            output.WriteValueU32(0, endian);
        }

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var entryCount = input.ReadValueU32(endian);
            var entryOffsets = new uint[entryCount];
            for (int i = 0; i < entryOffsets.Length; i++)
            {
                entryOffsets[i] = input.ReadValueU32(endian);
            }
            var totalSize = input.ReadValueU32(endian);

            var entryIds = new uint[entryCount];
            if (entryCount > 0)
            {
                var headerSize = 4 + 4 + (4 * entryCount) + 4 + 4;
                var entryIdSize = entryOffsets[0] - headerSize;

                // Sanity checking id space.
                /*
                var alignedHeaderSize = headerSize.Align(16);
                var padding = alignedHeaderSize - headerSize;
                var extraIdSize = entryOffsets[0] - alignedHeaderSize;
                if (extraIdSize > 0 && entryCount * 4 > padding && entryCount * 4 > entryIdSize)
                {
                    throw new FormatException();
                }
                */

                for (int i = 0, o = 0; i < entryCount && o < entryIdSize; i++, o += 4)
                {
                    entryIds[i] = input.ReadValueU32(endian);
                }
            }

            var unknown = input.ReadValueU32(endian);
            if (unknown != 0)
            {
                throw new FormatException();
            }

            this.Endian = endian;
            this.Entries.Clear();
            for (int i = 0; i < entryCount; i++)
            {
                this.Entries.Add(new Entry(entryIds[i], entryOffsets[i]));
            }
            this.TotalSize = totalSize;
        }

        public struct Entry
        {
            public readonly uint RawId;
            public readonly uint Offset;

            public Entry(uint rawId, uint offset)
            {
                this.RawId = rawId;
                this.Offset = offset;
            }

            public ushort FileId { get { return (ushort)(this.RawId & 0xFFFFu); } }
            public ushort DirectoryId { get { return (ushort)((this.RawId >> 16) & 0xFFFFu); } }
        }
    }
}
