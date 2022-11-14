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

namespace Gibbed.LetUsClingTogether.SheetFormats
{
    public class PrimitiveDescriptor : IDescriptor
    {
        private readonly PrimitiveType _Type;
        private readonly int _MinimumWidth;
        private readonly Dictionary<long, string> _EnumMembers;

        public PrimitiveDescriptor(PrimitiveType type, int minimumWidth, Dictionary<long, string> enumMembers)
        {
            if (minimumWidth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumWidth));
            }
            
            this._Type = type;
            this._MinimumWidth = minimumWidth;

            if (enumMembers != null && enumMembers.Count > 0)
            {
                this._EnumMembers = new();
                foreach (var kv in enumMembers)
                {
                    this._EnumMembers.Add(kv.Key, kv.Value);
                }
            }
        }

        public int EntrySize => this.GetEntrySize();
        public bool HasStrings => this._Type == PrimitiveType.String;

        public Tommy.TomlNode Export(Stream stream, Endian endian, Dictionary<uint, List<Tommy.TomlString>> strings)
        {
            Tommy.TomlNode node = this._Type switch
            {
                PrimitiveType.Boolean => stream.ReadValueB8(),
                PrimitiveType.String => ReadString(stream, endian, strings),
                PrimitiveType.Float32 => stream.ReadValueF32(endian),
                _ => null,
            };
            if (node == null)
            {
                long value = this._Type switch
                {
                    PrimitiveType.Int8 => stream.ReadValueS8(),
                    PrimitiveType.UInt8 => stream.ReadValueU8(),
                    PrimitiveType.Int16 => stream.ReadValueS16(endian),
                    PrimitiveType.UInt16 => stream.ReadValueU16(endian),
                    PrimitiveType.Int32 => stream.ReadValueS32(endian),
                    PrimitiveType.UInt32 => stream.ReadValueU32(endian),
                    _ => throw new NotSupportedException(),
                };
                if (this._EnumMembers != null &&
                    this._EnumMembers.TryGetValue(value, out var enumName) == true)
                {
                    Tommy.TomlString stringNode = new();
                    stringNode.Value = enumName;
                    node = stringNode;
                }
                else
                {
                    node = value;
                }
            }
            node.MinimumInlineWidth = this._MinimumWidth;
            return node;
        }

        private static Tommy.TomlNode ReadString(Stream stream, Endian endian, Dictionary<uint, List<Tommy.TomlString>> strings)
        {
            var offset = stream.ReadValueU32(endian);
            if (offset == 0)
            {
                return "";
            }
            var node = new Tommy.TomlString();
            if (strings.TryGetValue(offset, out var offsetStrings) == false)
            {
                offsetStrings = strings[offset] = new List<Tommy.TomlString>();
            }
            offsetStrings.Add(node);
            return node;
        }

        private int GetEntrySize() => this._Type switch
        {
            PrimitiveType.Boolean => 1,
            PrimitiveType.Int8 => 1,
            PrimitiveType.UInt8 => 1,
            PrimitiveType.Int16 => 2,
            PrimitiveType.UInt16 => 2,
            PrimitiveType.Int32 => 4,
            PrimitiveType.UInt32 => 4,
            PrimitiveType.Float32 => 4,
            PrimitiveType.String => 4,
            _ => throw new NotImplementedException(),
        };
    }
}
