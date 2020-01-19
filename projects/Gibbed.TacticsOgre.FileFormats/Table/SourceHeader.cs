/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
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
using System.Text;
using System.IO;
using Gibbed.IO;

namespace Gibbed.TacticsOgre.FileFormats.Table
{
    public class SourceHeader
    {
        public ushort Id;
        public ushort Unknown02;
        public ushort Unknown04;
        public ushort Unknown06;
        public ushort Unknown08;
        public ushort DirectoryCount;
        public ushort Unknown0C;
        public short Unknown0E;
        public uint DirectoryTableOffset;
        public uint Unknown14;

        public void Deserialize(Stream input)
        {
            this.Id = input.ReadValueU16();
            this.Unknown02 = input.ReadValueU16();
            this.Unknown04 = input.ReadValueU16();
            this.Unknown06 = input.ReadValueU16();
            this.Unknown08 = input.ReadValueU16();
            this.DirectoryCount = input.ReadValueU16();
            this.Unknown0C = input.ReadValueU16();
            this.Unknown0E = input.ReadValueS16();
            this.DirectoryTableOffset = input.ReadValueU32();
            this.Unknown14 = input.ReadValueU32();
        }
    }
}
