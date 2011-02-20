using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.TacticsOgre.FileFormats.Table
{
    public class GroupHeader
    {
        public ushort Id;
        public ushort FileCount;
        public ushort FileTableOffset;
        public GroupFlags Flags;

        public void Deserialize(Stream input)
        {
            this.Id = input.ReadValueU16();
            this.FileCount = input.ReadValueU16();
            this.FileTableOffset = input.ReadValueU16();
            this.Flags = (GroupFlags)input.ReadValueU16();

            if (((ushort)this.Flags & (~24576)) != 0)
            {
                throw new NotSupportedException("unknown directory flags");
            }
        }
    }
}
