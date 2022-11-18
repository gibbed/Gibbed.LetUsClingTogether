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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;

namespace Gibbed.LetUsClingTogether.SheetFormats
{
    public class StructDescriptor : IDescriptor, IEnumerable<KeyValuePair<string, IDescriptor>>, IEnumerable
    {
        private readonly List<KeyValuePair<string, IDescriptor>> _Fields;
        private readonly int _MinimumWidth;
        private readonly bool _IsInline;

        public StructDescriptor(int minimumWidth, bool isInline)
        {
            if (minimumWidth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumWidth));
            }
            this._Fields = new List<KeyValuePair<string, IDescriptor>>();
            this._MinimumWidth = minimumWidth;
            this._IsInline = isInline;
        }

        public IEnumerable<KeyValuePair<string, IDescriptor>> Fields
        {
            get { return this._Fields; }
        }

        public int EntrySize => this._Fields.Sum(kv => kv.Value.EntrySize);
        public bool HasStrings => this._Fields.Any(kv => kv.Value.HasStrings);

        public void Add(string name, PrimitiveType type)
        {
            this.Add(name, type, 0);
        }

        public void Add(string name, PrimitiveType type, int minimumWidth)
        {
            if (type == PrimitiveType.String)
            {
                this.Add(name, new StringDescriptor(minimumWidth));
            }
            else if (type.IsInteger() == true)
            {
                this.Add(name, new IntegerDescriptor(type, IntegerBase.Decimal, minimumWidth, null));
            }
            else if (type.IsFloat() == true)
            {
                this.Add(name, new FloatDescriptor(type, minimumWidth));
            }
            else if (type.IsUndefined() == true)
            {
                this.Add(name, new UndefinedDescriptor(type, minimumWidth));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Add(string name, IDescriptor descriptor)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            this._Fields.Add(new KeyValuePair<string, IDescriptor>(name, descriptor));
        }

        public Tommy.TomlNode Export(Stream stream, Endian endian, Dictionary<uint, List<Tommy.TomlString>> strings)
        {
            var table = new Tommy.TomlTable()
            {
                MinimumInlineWidth = this._MinimumWidth,
                IsInline = this._IsInline,
            };
            foreach (var kv in this._Fields)
            {
                table[kv.Key] = kv.Value.Export(stream, endian, strings);
            }
            return table;
        }

        IEnumerator<KeyValuePair<string, IDescriptor>> IEnumerable<KeyValuePair<string, IDescriptor>>.GetEnumerator()
        {
            return this._Fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._Fields.GetEnumerator();
        }
    }
}
