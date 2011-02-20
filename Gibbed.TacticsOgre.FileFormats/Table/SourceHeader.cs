using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Helpers;

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
