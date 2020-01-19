using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.TacticsOgre.FileFormats.Table
{
    [Flags]
    public enum GroupFlags : ushort
    {
        /// <summary>
        /// Has extended file sizes. (32-bit vs 16-bit)
        /// </summary>
        ExtendedSizes = 1 << 13,
        Unknown14 = 1 << 14,
    }
}
