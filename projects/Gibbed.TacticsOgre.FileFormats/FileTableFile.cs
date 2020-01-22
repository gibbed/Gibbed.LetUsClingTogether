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

        public const int BaseDataBlockSize = 0x800;

        public List<DirectoryEntry> Directories { get; }

        public FileTableFile()
        {
            this.Directories = new List<DirectoryEntry>();
        }

        private delegate FileHeader ReadFileHeader(Stream input, Endian endian);
        private delegate void WriteFileHeader(Stream input, FileHeader instance, Endian endian);

        private static readonly int[] _FileTableEntrySizes;
        private static readonly ReadFileHeader[] _FileTableEntryReaders;
        private static readonly WriteFileHeader[] _FileTableEntryWriters;

        static FileTableFile()
        {
            _FileTableEntrySizes = new[] { 2, 0, 4, 6, 6, 8 };
            _FileTableEntryReaders = new ReadFileHeader[]
            {
                null,
                null,
                FileHeader.Read2,
                FileHeader.Read3,
                FileHeader.Read4,
                FileHeader.Read5,
            };
            _FileTableEntryWriters = new WriteFileHeader[]
            {
                null,
                null,
                FileHeader.Write2,
                FileHeader.Write3,
                FileHeader.Write4,
                FileHeader.Write5,
            };
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

            var nameTableEntries = new NameHeader[nameTableCount];
            for (int i = 0; i < nameTableCount; i++)
            {
                nameTableEntries[i] = NameHeader.Read(input, endian);
            }

            var directoryHeaders = new DirectoryHeader[directoryCount];
            for (int i = 0; i < directoryCount; i++)
            {
                var directoryHeader = directoryHeaders[i] = DirectoryHeader.Read(input, endian);
                if (directoryHeader.Unknown02 != 0 ||
                    directoryHeader.Unknown08 != 0)
                {
                    throw new FormatException();
                }
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
                var fileTableEntrySizeIndex = (int)batchHeader.Flags;
                if (fileTableEntrySizeIndex == 1 || fileTableEntrySizeIndex > 5)
                {
                    throw new NotSupportedException();
                }
                fileTableSize += batchHeader.FileCount * _FileTableEntrySizes[fileTableEntrySizeIndex];
            }

            if (totalSize - fileTableOffset != fileTableSize)
            {
                throw new InvalidOperationException();
            }

            this.Directories.Clear();
            using (var data = input.ReadToMemoryStream(fileTableSize))
            {
                foreach (var directoryHeader in directoryHeaders)
                {
                    var batchIndexBase = directoryHeader.BatchTableOffset / 8;

                    var directory = new DirectoryEntry();
                    directory.Id = directoryHeader.Id;

                    for (int i = 0; i < directoryHeader.BatchCount; i++)
                    {
                        if ((directoryHeader.BatchTableOffset % 8) != 0)
                        {
                            throw new FormatException();
                        }

                        var batchHeader = batchHeaders[batchIndexBase + i];

                        var readDataHeader = _FileTableEntryReaders[(int)batchHeader.Flags];
                        if (readDataHeader == null)
                        {
                            throw new NotSupportedException();
                        }

                        data.Position = batchHeader.FileTableOffset;
                        for (int j = 0; j < batchHeader.FileCount; j++)
                        {
                            var fileHeader = readDataHeader(data, endian);

                            var fileId = batchHeader.BaseFileId + (uint)j;

                            long dataOffset;
                            dataOffset = directoryHeader.DataBaseOffset;
                            dataOffset += (fileHeader.DataBlockOffset << directoryHeader.DataBlockSize) * BaseDataBlockSize;

                            uint? nameHash = null;
                            if (directoryHeader.NameTableCount > 0)
                            {
                                if (directoryHeader.NameTableIndex == 0xFFFF)
                                {
                                    throw new InvalidOperationException();
                                }

                                var nameIndex = Array.FindIndex(
                                    nameTableEntries,
                                    directoryHeader.NameTableIndex,
                                    directoryHeader.NameTableCount,
                                    nte => nte.DirectoryId == directoryHeader.Id &&
                                           nte.FileId == fileId);
                                if (nameIndex >= 0)
                                {
                                    nameHash = nameTableEntries[nameIndex].NameHash;
                                }
                            }

                            FileEntry file;
                            file.Id = fileId;
                            file.NameHash = nameHash;
                            file.DataOffset = dataOffset;
                            file.DataSize = fileHeader.DataSize;
                            directory.Files.Add(file);
                        }
                    }

                    this.Directories.Add(directory);
                }
            }
        }
    }
}
