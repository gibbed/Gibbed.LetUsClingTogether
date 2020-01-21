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
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using Gibbed.TacticsOgre.FileFormats.FileTable;

namespace Gibbed.TacticsOgre.FileFormats
{
    public class FileTableFile
    {
        public const ushort Signature = 0x1EF1;

        public List<NameTableEntry> NameTable { get; }
        public List<DirectoryEntry> Directories { get; }

        public FileTableFile()
        {
            this.NameTable = new List<NameTableEntry>();
            this.Directories = new List<DirectoryEntry>();
        }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU16(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var nameTableCount = input.ReadValueU16(endian);
            var directoryCount = input.ReadValueU16(endian);

            var unknown06 = input.ReadValueU16(endian);
            if (unknown06 != 4)
            {
                throw new FormatException();
            }

            var fileTableOffset = input.ReadValueU32(endian);
            var totalSize = input.ReadValueU32(endian); // size of FILETABLE.BIN
            var titleId1 = input.ReadString(16, true, Encoding.ASCII);
            var titleId2 = input.ReadString(16, true, Encoding.ASCII);

            var unknown30 = input.ReadValueU16(endian);
            var unknown32 = input.ReadValueU8();
            var unknown33 = input.ReadValueU8();
            if (unknown30 != 0 || (unknown32 != 0 && unknown32 != 1) || unknown33 != 5)
            {
                throw new FormatException();
            }

            var unknown34 = input.ReadBytes(16); // GUID?

            var nameTableEntries = new NameTableEntry[nameTableCount];
            for (int i = 0; i < nameTableCount; i++)
            {
                nameTableEntries[i] = NameTableEntry.Read(input, endian);
            }

            var directoryHeaders = new DirectoryHeader[directoryCount];
            for (int i = 0; i < directoryCount; i++)
            {
                directoryHeaders[i] = DirectoryHeader.Read(input, endian);
            }

            var totalBatchCount = directoryHeaders.Sum(s => s.BatchCount);
            if (input.Position + (totalBatchCount * 8) != fileTableOffset)
            {
                throw new InvalidOperationException();
            }

            var batchHeaders = new BatchHeader[totalBatchCount];
            for (int i = 0; i < totalBatchCount; i++)
            {
                batchHeaders[i] = BatchHeader.Read(input, endian);
            }

            var totalFileCount = batchHeaders.Sum(d => d.FileCount);

            int fileTableSize = 0;
            foreach (var batchHeader in batchHeaders)
            {
                var fileSizeSize = (batchHeader.Flags & BatchFlags.LargeFileSize) != 0
                    ? 4
                    : 2;
                fileTableSize += batchHeader.FileCount * (2 + fileSizeSize);
            }

            if (totalSize - fileTableOffset != fileTableSize)
            {
                throw new InvalidOperationException();
            }

            this.NameTable.Clear();
            this.NameTable.AddRange(nameTableEntries);

            this.Directories.Clear();
            using (var data = input.ReadToMemoryStream(fileTableSize))
            {
                foreach (var directoryHeader in directoryHeaders)
                {
                    var batchIndexBase = directoryHeader.BatchTableOffset / 8;

                    var directory = new DirectoryEntry();
                    directory.Id = directoryHeader.Id;

                    for (ushort i = 0; i < directoryHeader.BatchCount; i++)
                    {
                        if ((directoryHeader.BatchTableOffset % 8) != 0)
                        {
                            throw new FormatException();
                        }

                        var batchHeader = batchHeaders[batchIndexBase + i];

                        var readSize = (batchHeader.Flags & BatchFlags.LargeFileSize) != 0
                            ? (Func<uint>)(() => data.ReadValueU32(endian))
                            : () => data.ReadValueU16(endian);

                        data.Position = batchHeader.FileTableOffset;
                        for (int j = 0; j < batchHeader.FileCount; j++)
                        {
                            FileEntry file;
                            file.Id = batchHeader.BaseFileId + (uint)j;
                            file.DataBlockOffset = data.ReadValueU16(endian);
                            file.DataSize = readSize();
                            directory.Files.Add(file);
                        }
                    }

                    this.Directories.Add(directory);
                }
            }
        }
    }
}
