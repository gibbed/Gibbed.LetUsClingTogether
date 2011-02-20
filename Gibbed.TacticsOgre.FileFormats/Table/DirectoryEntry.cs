using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.TacticsOgre.FileFormats.Table
{
    public class DirectoryEntry
    {
        public ushort Id;
        public List<FileEntry> Files
            = new List<FileEntry>();
    }
}
