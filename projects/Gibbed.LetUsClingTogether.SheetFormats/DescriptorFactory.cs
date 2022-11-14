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
using System.Linq;

namespace Gibbed.LetUsClingTogether.SheetFormats
{
	public class DescriptorFactory
	{
		private readonly Dictionary<string, DescriptorInfo> _Lookup;

		public DescriptorFactory()
		{
			this._Lookup = new();
		}

		public void Add(string name, DescriptorInfo info)
        {
			this._Lookup.Add(name, info);
        }

		public bool TryGet(string name, out DescriptorInfo info)
		{
			if (string.IsNullOrEmpty(name) == true)
			{
				throw new ArgumentNullException(nameof(name));
			}
			return this._Lookup.TryGetValue(name, out info);
		}

		public IEnumerable<KeyValuePair<string, DescriptorInfo>> Get(int entrySize, bool hasStrings)
			=> this._Lookup
				.Where(kv => kv.Value.EntrySize == entrySize && kv.Value.HasStrings == hasStrings)
				.Select(kv => kv);
	}
}
