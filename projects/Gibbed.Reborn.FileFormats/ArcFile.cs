/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
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
using System.Text;
using Gibbed.IO;

namespace Gibbed.Reborn.FileFormats
{
    public class ArcFile
    {
        public const uint Signature = 0x00435241; // 'ARC\0'

        private readonly List<Entry> _Entries;

        public ArcFile()
        {
            this._Entries = new();
        }

        public Endian Endian { get; set; }
        public List<Entry> Entries => this._Entries;

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            var basePosition = input.Position;

            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var version = input.ReadValueU16(endian);
            if (version != 0x100)
            {
                throw new FormatException($"unexpected version {version:X}");
            }

            int entryCount = input.ReadValueU16(endian);
            int offsetCount = input.ReadValueU16(endian);

            input.Position = basePosition + 0x30;
            var offsets = new uint[offsetCount];
            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = input.ReadValueU32(endian);
            }

            input.Position = basePosition + (0x30 + offsetCount * 4).Align(32);
            var entries = new Entry[entryCount];
            for (int i = 0; i < entries.Length; i++)
            {
                var entryPosition = input.Position;

                Entry entry;
                entry.Type = input.ReadValueU32(endian);
                entry.SmallId = input.ReadValueU16(endian);
                var entryOffsetIndex = input.ReadValueU16(endian);
                entry.NameHash = input.ReadValueU32(endian);
                entry.UnknownC = input.ReadValueU8();
                entry.HasBigId = input.ReadValueB8();
                var entryNameLength = input.ReadValueU8();
                entry.UnknownF = input.ReadValueU8();
                entry.BigId = input.ReadValueU32(endian);

                var nextPosition = entryPosition + (20 + entryNameLength).Align(32);

                entry.Offset = offsets[entryOffsetIndex];
                entry.Name = input.ReadString(entryNameLength, true, Encoding.ASCII);

                if (entry.UnknownC != 0 || entry.UnknownF != 0)
                {
                    throw new FormatException();
                }

                input.Position = nextPosition;

                entries[i] = entry;
            }

            this.Endian = endian;
            this.Entries.Clear();
            this.Entries.AddRange(entries);
        }

        public struct Entry
        {
            public uint Type;
            public ushort SmallId;
            public uint Offset;
            public uint NameHash;
            public byte UnknownC;
            public bool HasBigId;
            public byte UnknownF;
            public uint BigId;
            public string Name;

            public uint Id => this.HasBigId == false ? this.SmallId : this.BigId;
        }
    }
}
