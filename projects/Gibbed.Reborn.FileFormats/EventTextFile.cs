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
using Gibbed.TacticsOgre.TextFormats;

namespace Gibbed.Reborn.FileFormats
{
    public class EventTextFile
    {
        public const uint Signature = 0x65747874; // 'etxt'

        private readonly List<Entry> _Entries;

        public EventTextFile()
        {
            this.Endian = Endian.Little;
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
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var count = input.ReadValueS32(endian);
            var headerSize = input.ReadValueU32(endian);
            var entrySize = input.ReadValueU32(endian);

            if (headerSize != 16 || entrySize != 32)
            {
                throw new FormatException();
            }

            var entryHeaders = new EntryHeader[count];
            for (int i = 0; i < count; i++)
            {
                EntryHeader entryHeader;
                entryHeader.IdOffset = input.ReadValueU32(endian);
                entryHeader.Unknown04 = input.ReadValueU32(endian);
                entryHeader.ValueOffset = input.ReadValueU32(endian);
                entryHeader.Unknown0C = input.ReadValueU32(endian);
                entryHeader.Unknown10Offset = input.ReadValueU32(endian);
                entryHeader.Unknown14 = input.ReadValueU32(endian);
                entryHeader.Unknown18Offset = input.ReadValueU32(endian);
                entryHeader.Unknown1C = input.ReadValueU32(endian);
                entryHeaders[i] = entryHeader;
                if (entryHeader.Unknown04 != 0 ||
                    entryHeader.Unknown0C != 0 ||
                    entryHeader.Unknown14 != 0 ||
                    entryHeader.Unknown1C != 0)
                {
                    throw new FormatException();
                }
            }

            RebornDecoder decoder = new();

            var entries = new Entry[count];
            for (int i = 0; i < count; i++)
            {
                var entryHeader = entryHeaders[i];
                input.Position = entryHeader.IdOffset;
                var id = input.ReadStringZ(Encoding.UTF8);
                input.Position = entryHeader.ValueOffset;
                var value = decoder.Decode(input, endian);
                input.Position = entryHeader.Unknown10Offset;
                var unknown10 = input.ReadStringZ(Encoding.UTF8);
                input.Position = entryHeader.Unknown18Offset;
                var unknown18 = input.ReadStringZ(Encoding.UTF8);
                entries[i] = new(id, value, unknown10, unknown18);
            }

            this.Endian = endian;
            this.Entries.Clear();
            this.Entries.AddRange(entries);
        }

        private struct EntryHeader
        {
            public uint IdOffset;
            public uint Unknown04;
            public uint ValueOffset;
            public uint Unknown0C;
            public uint Unknown10Offset;
            public uint Unknown14;
            public uint Unknown18Offset;
            public uint Unknown1C;
        }

        public struct Entry : IEquatable<Entry>
        {
            public readonly string Id;
            public readonly string Value;
            public readonly string Unknown10;
            public readonly string Unknown18;

            public Entry(string id, string value, string unknown10, string unknown18)
            {
                this.Id = id;
                this.Value = value;
                this.Unknown10 = unknown10;
                this.Unknown18 = unknown18;
            }

            #region Deconstruct & tuple operators
            public void Deconstruct(out string id, out string value, out string unknown10, out string unknown18)
            {
                id = this.Id;
                value = this.Value;
                unknown10 = this.Unknown10;
                unknown18 = this.Unknown18;
            }

            public static implicit operator (string id, string value, string unknown10, string unknown18)(Entry value)
            {
                return (value.Id, value.Value, value.Unknown10, value.Unknown18);
            }

            public static implicit operator Entry((string id, string value, string unknown10, string unknown18) value)
            {
                return new(value.id, value.value, value.unknown10, value.unknown18);
            }
            #endregion
            #region Equals, IEquatable & equality operators
            public override bool Equals(object obj)
            {
                return obj is Entry entry && this.Equals(entry) == true;
            }

            public bool Equals(Entry other)
            {
                return
                    this.Id == other.Id &&
                    this.Value == other.Value &&
                    this.Unknown10 == other.Unknown10 &&
                    this.Unknown18 == other.Unknown18;
            }

            public override int GetHashCode()
            {
                int hashCode = 149833570;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Id);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Value);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Unknown10);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Unknown18);
                return hashCode;
            }

            public static bool operator ==(Entry left, Entry right)
            {
                return left.Equals(right) == true;
            }

            public static bool operator !=(Entry left, Entry right)
            {
                return left.Equals(right) == false;
            }
            #endregion
        }
    }
}
