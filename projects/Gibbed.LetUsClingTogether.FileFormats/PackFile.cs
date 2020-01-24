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
using Gibbed.IO;

namespace Gibbed.LetUsClingTogether.FileFormats
{
    public class PackFile
    {
        public const uint Signature = 0x646B6170; // 'pakd'

        public PackFile()
        {
            this.EntryOffsets = new List<uint>();
        }

        public Endian Endian { get; set; }
        public List<uint> EntryOffsets { get; }
        public uint EndOffset { get; set; }

        public int GetHeaderSize()
        {
            return 4 + (this.EntryOffsets.Count * 4) + 4;
        }

        public void Serialize(Stream output)
        {
            var endian = this.Endian;
            output.WriteValueU32(Signature, endian);
            foreach (var entryOffset in this.EntryOffsets)
            {
                output.WriteValueU32(entryOffset, endian);
            }
            output.WriteValueU32(this.EndOffset, endian);
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
            var endOffset = input.ReadValueU32(endian);

            this.Endian = endian;
            this.EntryOffsets.Clear();
            this.EntryOffsets.AddRange(entryOffsets);
            this.EndOffset = endOffset;
        }
    }
}
