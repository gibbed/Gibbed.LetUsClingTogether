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
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.TacticsOgre.FileFormats
{
    public class EventMessagesFile
    {
        public const uint Signature = 0x53454D45; // 'EMES'

        private readonly Dictionary<ushort, Entry> _Entries;

        public EventMessagesFile()
        {
            this.Endian = Endian.Little;
            this._Entries = new();
        }

        public Endian Endian { get; set; }
        public Dictionary<ushort, Entry> Entries => this._Entries;

        public void Serialize(Stream output, Text.Formatter formatter)
        {
            this.Serialize(output, formatter, true);
        }

        public void Serialize(Stream output, Text.Formatter formatter, bool includeTags)
        {
            if (this.Entries.Count > ushort.MaxValue)
            {
                throw new InvalidOperationException("too many entries");
            }

            var endian = this.Endian;
            var count = (ushort)this.Entries.Count;

            var entryPositions = new long[count];
            byte[] data;
            using (MemoryStream temp = new())
            {
                int i = 0;
                foreach (var entry in this.Entries.Values)
                {
                    entryPositions[i] = temp.Position;
                    temp.WriteValueU16(entry.NameId, endian);
                    formatter.Encode(entry.Text, temp, endian);
                }
                data = temp.ToArray();
            }

            output.WriteValueU32(Signature, endian);
            output.WriteValueU16(count, endian);
            output.WriteValueS16(-1, endian);

            foreach (var id in this.Entries.Keys)
            {
                output.WriteValueU16(id, endian);
            }

            if ((count & 1) != 0)
            {
                output.WriteValueS16(-1, endian);
            }

            var dataPosition = output.Position + (4 * count);
            for (int i = 0; i < count; i++)
            {
                var entryPosition = dataPosition + entryPositions[i];
                if (entryPosition > uint.MaxValue)
                {
                    throw new InvalidOperationException();
                }
                output.WriteValueU32((uint)entryPosition, endian);
            }

            output.WriteBytes(data);
        }

        public void Deserialize(Stream input, Text.Formatter formatter)
        {
            this.Deserialize(input, formatter, true);
        }

        public void Deserialize(Stream input, Text.Formatter formatter, bool includeTags)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var count = input.ReadValueU16(endian);

            var unknown = input.ReadValueS16(endian); // probably padding
            if (unknown != -1)
            {
                throw new FormatException();
            }

            var ids = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                ids[i] = input.ReadValueU16(endian);
            }

            input.Position = input.Position.Align(4);

            var offsets = new uint[count];
            for (int i = 0; i < count; i++)
            {
                offsets[i] = input.ReadValueU32(endian);
            }

            Dictionary<ushort, Entry> entries = new();
            for (int i = 0; i < count; i++)
            {
                input.Position = offsets[i];
                var nameId = input.ReadValueU16(endian);
                var text = formatter.Decode(input, endian/*, includeTags*/);
                entries.Add(ids[i], new(nameId, text));
            }

            this.Endian = endian;
            this.Entries.Clear();
            foreach (var kv in entries)
            {
                this.Entries[kv.Key] = kv.Value;
            }
        }

        public struct Entry : IEquatable<Entry>
        {
            public readonly ushort NameId;
            public readonly string Text;

            public Entry(ushort nameId, string text)
            {
                this.NameId = nameId;
                this.Text = text;
            }

            #region Deconstruct & tuple operators
            public void Deconstruct(out ushort nameId, out string text)
            {
                nameId = this.NameId;
                text = this.Text;
            }

            public static implicit operator (ushort nameId, string text)(Entry value)
            {
                return (value.NameId, value.Text);
            }

            public static implicit operator Entry((ushort nameId, string text) value)
            {
                return new(value.nameId, value.text);
            }
            #endregion
            #region Equals, IEquatable & equality operators
            public override bool Equals(object obj)
            {
                return obj is Entry message && this.Equals(message) == true;
            }

            public bool Equals(Entry other)
            {
                return this.NameId == other.NameId &&
                       this.Text == other.Text;
            }

            public override int GetHashCode()
            {
                int hashCode = -1763348271;
                hashCode = hashCode * -1521134295 + this.NameId.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Text);
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
