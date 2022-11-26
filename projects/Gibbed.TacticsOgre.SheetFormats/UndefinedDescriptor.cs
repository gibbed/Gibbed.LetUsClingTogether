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

namespace Gibbed.TacticsOgre.SheetFormats
{
    public class UndefinedDescriptor : IDescriptor
    {
        private readonly PrimitiveType _Type;
        private readonly int _MinimumWidth;

        public UndefinedDescriptor(PrimitiveType type, int minimumWidth)
        {
            if (minimumWidth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumWidth));
            }

            if (type.IsUndefined() == false)
            {
                throw new ArgumentException("not an undefined type", nameof(type));
            }

            this._Type = type;
            this._MinimumWidth = minimumWidth;
        }

        public int EntrySize => this.GetEntrySize();
        public bool HasStrings => false;

        public Tommy.TomlNode Export(Stream stream, Endian endian, Dictionary<uint, List<Tommy.TomlString>> strings)
        {
            Tommy.TomlNode node = this._Type switch
            {
                PrimitiveType.Undefined8 => stream.ReadValueU8(),
                PrimitiveType.Undefined16 => stream.ReadValueU16(endian),
                PrimitiveType.Undefined32 => stream.ReadValueU32(endian),
                PrimitiveType.Undefined64 => stream.ReadValueU64(endian),
                _ => throw new NotSupportedException(),
            };
            node.MinimumInlineWidth = this._MinimumWidth;
            return node;
        }

        private int GetEntrySize() => this._Type switch
        {
            PrimitiveType.Undefined8 => 1,
            PrimitiveType.Undefined16 => 2,
            PrimitiveType.Undefined32 => 4,
            PrimitiveType.Undefined64 => 8,
            _ => throw new NotImplementedException(),
        };
    }
}
