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
    public class BooleanDescriptor : IDescriptor
    {
        private readonly int _MinimumWidth;

        public BooleanDescriptor(int minimumWidth)
        {
            if (minimumWidth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumWidth));
            }

            // implicit minimum width of 5
            this._MinimumWidth = minimumWidth > 0 ? minimumWidth : 5;
        }

        public int EntrySize => 1;
        public bool HasStrings => false;

        public Tommy.TomlNode Export(Stream stream, Endian endian, Dictionary<uint, List<Tommy.TomlString>> strings)
        {
            bool value = stream.ReadValueB8();
            return new Tommy.TomlBoolean()
            {
                Value = value,
                MinimumInlineWidth = this._MinimumWidth,
            };
        }
    }
}
