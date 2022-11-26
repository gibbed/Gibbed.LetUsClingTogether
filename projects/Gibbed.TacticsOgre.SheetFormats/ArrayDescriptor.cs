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
using Tommy;

namespace Gibbed.TacticsOgre.SheetFormats
{
    public class ArrayDescriptor : IDescriptor
    {
        private readonly IDescriptor _Item;
        private readonly int _Count;
        private readonly int _MinimumWidth;
        private readonly bool _IsInline;

        public ArrayDescriptor(IDescriptor itemDescriptor, int count, int minimumWidth, bool isInline)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (minimumWidth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumWidth));
            }
            this._Item = itemDescriptor ?? throw new ArgumentNullException(nameof(itemDescriptor));
            this._Count = count;
            this._IsInline = isInline;
        }

        public int EntrySize => this._Item.EntrySize * this._Count;
        public bool HasStrings => this._Item.HasStrings;

        public TomlNode Export(Stream stream, Endian endian, Dictionary<uint, List<TomlString>> strings)
        {
            var item = this._Item;
            var array = new TomlArray();
            array.IsMultiline = true;
            array.MinimumInlineWidth = this._MinimumWidth;
            for (int i = 0; i < this._Count; i++)
            {
                array.Add(item.Export(stream, endian, strings));
            }
            return array;
        }
    }
}
