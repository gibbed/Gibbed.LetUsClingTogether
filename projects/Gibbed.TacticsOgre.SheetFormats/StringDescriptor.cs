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
using Gibbed.IO;

namespace Gibbed.TacticsOgre.SheetFormats
{
    public class StringDescriptor : IDescriptor
    {
        private readonly int _MinimumWidth;
        private readonly bool _IsReborn;

        public StringDescriptor(int minimumWidth, bool isReborn)
        {
            if (minimumWidth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumWidth));
            }

            this._MinimumWidth = minimumWidth;
            this._IsReborn = isReborn;
        }

        public int EntrySize => this._IsReborn == true ? 8 : 4;
        public bool HasStrings => true;

        public Tommy.TomlNode Export(Stream stream, Endian endian, Dictionary<uint, List<Tommy.TomlString>> strings)
        {
            Tommy.TomlNode node = ReadString(stream, endian, strings, this._IsReborn);
            node.MinimumInlineWidth = this._MinimumWidth;
            return node;
        }

        private static Tommy.TomlNode ReadString(Stream stream, Endian endian, Dictionary<uint, List<Tommy.TomlString>> strings, bool isReborn)
        {
            uint offset;
            if (isReborn == true)
            {
                ulong bigOffset = stream.ReadValueU64(endian);
                if ((bigOffset >> 32) != 0)
                {
                    throw new InvalidOperationException();
                }
                offset = (uint)bigOffset;
            }
            else
            {
                offset = stream.ReadValueU32(endian);
            }
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
    }
}
