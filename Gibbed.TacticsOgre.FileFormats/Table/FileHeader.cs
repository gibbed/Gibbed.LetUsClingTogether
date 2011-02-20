using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.TacticsOgre.FileFormats.Table
{
    public class FileHeader
    {
        public ushort Index; // index * 0x8000 to get offset
        public uint Offset { get { return (uint)this.Index * 0x8000u; } }
        public uint Size;

        public void Deserialize(Stream input)
        {
            this.Index = input.ReadValueU16();
            this.Size = input.ReadValueU32();
        }
    }
}
