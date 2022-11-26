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

namespace Gibbed.TacticsOgre.SheetFormats
{
    public struct DescriptorInfo
    {
        public readonly int EntrySize;
        public readonly bool HasStrings;
        public readonly bool RowsAsTableArray;
        public Func<IDescriptor> Instantiate;

        public DescriptorInfo(int entrySize, bool hasStrings, bool rowsAsTableArray, Func<IDescriptor> instantiate)
        {
            if (instantiate == null)
            {
                throw new ArgumentNullException(nameof(instantiate));
            }

            this.EntrySize = entrySize;
            this.HasStrings = hasStrings;
            this.RowsAsTableArray = rowsAsTableArray;
            this.Instantiate = instantiate;
        }
    }
}
