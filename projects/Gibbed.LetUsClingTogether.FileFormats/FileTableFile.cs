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
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using Gibbed.LetUsClingTogether.FileFormats.FileTable;

namespace Gibbed.LetUsClingTogether.FileFormats
{
    public class FileTableFile
    {
        private delegate FileHeader ReadFileHeader(Stream input, Endian endian);
        private delegate void WriteFileHeader(Stream input, FileHeader instance, Endian endian);

        private static readonly int[] _FileTableEntrySizes;
        private static readonly ReadFileHeader[] _FileTableEntryReaders;
        private static readonly WriteFileHeader[] _FileTableEntryWriters;

        static FileTableFile()
        {
            _FileTableEntrySizes = new[] { 4, 0, 4, 6, 6, 8 };
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

        public const ushort Signature = 0x1EF1;
        public const int BaseDataBlockSize = 0x800;

        private readonly List<DirectoryEntry> _Directories;

        public FileTableFile()
        {
            this._Directories = new();
        }

        public Endian Endian { get; set; }
        public bool IsReborn { get; set; }
        public string TitleId1 { get; set; }
        public string TitleId2 { get; set; }
        public byte Unknown32 { get; set; }

        // This value is used by the game when writing save data. PARENTAL_LEVEL in SFO.
        public byte ParentalLevel { get; set; }

        public byte[] InstallDataCryptoKey { get; set; }

        public List<DirectoryEntry> Directories => this._Directories;

        private static IEnumerable<int> ToBatchCounts<T>(IEnumerable<T> source, Func<T, int> predicate)
        {
            using var e = source.GetEnumerator();
            for (bool hasMore = e.MoveNext(); hasMore == true;)
            {
                int first = predicate(e.Current), last = first, next;
                while ((hasMore = e.MoveNext()) && (next = predicate(e.Current)) > last && next - last == 1)
                {
                    last = next;
                }
                yield return last - first + 1;
            }
        }

        public void Serialize(Stream output)
        {
            if (this.IsReborn == true)
            {
                throw new NotSupportedException();
            }

            var endian = this.Endian;

            List<DirectoryHeader> directoryHeaders = new();
            List<NameHeader> nameHeaders = new();
            List<BatchHeader> batchHeaders = new();

            byte[] fileTableBytes;
            using (MemoryStream fileTable = new())
            {
                foreach (var directory in this.Directories.OrderBy(d => d.Id))
                {
                    ushort nameIndex = (ushort)nameHeaders.Count;
                    ushort nameCount = 0;

                    int batchIndex = batchHeaders.Count;

                    var files = directory.Files.OrderBy(f => f.Id).ToList();
                    var batchFileCounts = ToBatchCounts(files, f => f.Id).ToList();

                    int fileIndex = 0;
                    foreach (var batchFileCount in batchFileCounts)
                    {
                        if (batchFileCount < 1)
                        {
                            continue;
                        }

                        if (batchFileCount > ushort.MaxValue)
                        {
                            throw new InvalidOperationException();
                        }

                        if (fileTable.Position > 0x1FFFFFFF)
                        {
                            throw new InvalidOperationException();
                        }

                        long fileTablePosition = fileTable.Position;

                        bool hasLargeDataOffset = false;
                        bool hasLargeDataSize = false;
                        for (int i = 0, o = fileIndex; i < batchFileCount; i++, o++)
                        {
                            var file = files[o];
                            if (file.DataBlockOffset > ushort.MaxValue)
                            {
                                hasLargeDataOffset = true;
                            }
                            if (file.DataSize > ushort.MaxValue)
                            {
                                hasLargeDataSize = true;
                            }
                        }

                        BatchFlags batchFlags;
                        if (hasLargeDataOffset == true && hasLargeDataSize == true)
                        {
                            batchFlags = BatchFlags.LargeDataSize | /*BatchFlags.Unknown |*/ BatchFlags.LargeDataOffset;
                            throw new NotSupportedException("needs testing in-game to verify");
                        }
                        else if (hasLargeDataOffset == true)
                        {
                            batchFlags = BatchFlags.Unknown | BatchFlags.LargeDataOffset;
                        }
                        else if (hasLargeDataSize == true)
                        {
                            batchFlags = BatchFlags.LargeDataSize | BatchFlags.Unknown;
                        }
                        else
                        {
                            batchFlags = BatchFlags.Unknown;
                        }

                        var writeFileHeader = _FileTableEntryWriters[(int)batchFlags];

                        for (int i = 0, o = fileIndex; i < batchFileCount; i++, o++)
                        {
                            var file = files[o];
                            if (file.NameHash != null)
                            {
                                nameHeaders.Add(new()
                                {
                                    NameHash = file.NameHash.Value,
                                    DirectoryId = directory.Id,
                                    FileId = file.Id,
                                });
                                nameCount++;
                            }
                            FileHeader fileHeader;
                            fileHeader.DataBlockOffset = file.DataBlockOffset;
                            fileHeader.DataSize = file.DataSize;
                            writeFileHeader(fileTable, fileHeader, endian);
                        }

                        BatchHeader batchHeader;
                        batchHeader.BaseFileId = files[fileIndex].Id;
                        batchHeader.FileCount = (ushort)batchFileCount;
                        batchHeader.FileTableOffset = (int)fileTablePosition;
                        batchHeader.Flags = batchFlags;
                        batchHeaders.Add(batchHeader);

                        fileIndex += batchFileCount;
                    }

                    DirectoryHeader directoryHeader;
                    directoryHeader.Id = directory.Id;
                    directoryHeader.IsEncrypted = directory.IsEncrypted;
                    directoryHeader.DataBaseOffset = directory.DataBaseOffset;
                    directoryHeader.DataBlockSize = directory.DataBlockSize;
                    directoryHeader.Unknown08 = 0;
                    directoryHeader.IsInInstallData = directory.IsInInstallData;
                    directoryHeader.BatchCount = (ushort)batchFileCounts.Count;
                    directoryHeader.NameTableCount = nameCount;
                    directoryHeader.NameTableIndex = nameCount > 0 ? nameIndex : ushort.MaxValue;
                    directoryHeader.BatchTableOffset = (uint)(batchIndex * 8);
                    directoryHeader.DataInstallBaseOffset = directory.DataInstallBaseOffset;
                    directoryHeaders.Add(directoryHeader);
                }

                fileTable.Flush();
                fileTableBytes = fileTable.ToArray();
            }

            output.WriteValueU16(Signature, endian);
            output.WriteValueU16((ushort)nameHeaders.Count, endian);
            output.WriteValueU16((ushort)directoryHeaders.Count, endian);
            output.WriteValueU16(4, endian);
            var fileTableOffsetPosition = output.Position;
            output.WriteValueU32(0xFFFFFFFFu, endian);
            var totalSizePosition = output.Position;
            output.WriteValueU32(0xFFFFFFFFu, endian);
            output.WriteString(this.TitleId1, 16, Encoding.ASCII);
            output.WriteString(this.TitleId2, 16, Encoding.ASCII);
            output.WriteValueU16(0, endian);
            output.WriteValueU8(this.Unknown32);
            output.WriteValueU8(this.ParentalLevel);
            output.WriteBytes(this.InstallDataCryptoKey);

            foreach (var nameTableEntry in nameHeaders)
            {
                nameTableEntry.Write(output, endian);
            }

            foreach (var directoryHeader in directoryHeaders)
            {
                directoryHeader.Write(output, endian);
            }

            foreach (var batchHeader in batchHeaders)
            {
                batchHeader.Write(output, endian);
            }

            var fileTableOffset = (uint)output.Position;
            output.WriteBytes(fileTableBytes);

            var totalSize = (uint)output.Position;

            output.Position = fileTableOffsetPosition;
            output.WriteValueU32(fileTableOffset, endian);

            output.Position = totalSizePosition;
            output.WriteValueU32(totalSize, endian);
        }

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU16(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var nameCount = input.ReadValueU16(endian);
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
            var parentalLevel = input.ReadValueU8();
            if (unknown30 != 0 || (unknown32 != 0 && unknown32 != 1))
            {
                throw new FormatException();
            }

            var installDataCryptoKey = input.ReadBytes(16);

            var nameHeaders = new NameHeader[nameCount];
            for (int i = 0; i < nameCount; i++)
            {
                nameHeaders[i] = NameHeader.Read(input, endian);
            }

            var directoryHeaders = new DirectoryHeader[directoryCount];
            for (int i = 0; i < directoryCount; i++)
            {
                var directoryHeader = directoryHeaders[i] = DirectoryHeader.Read(input, endian);
                if (directoryHeader.Unknown08 != 0)
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

            bool isReborn = input.Length > totalSize ? true : false;

            List<DirectoryEntry> directories = new();
            using (var data = input.ReadToMemoryStream(fileTableSize))
            {
                foreach (var directoryHeader in directoryHeaders)
                {
                    var batchIndexBase = directoryHeader.BatchTableOffset / 8;

                    DirectoryEntry directory = new()
                    {
                        Id = directoryHeader.Id,
                        IsEncrypted = directoryHeader.IsEncrypted,
                        DataBlockSize = directoryHeader.DataBlockSize,
                        DataBaseOffset = directoryHeader.DataBaseOffset,
                        IsInInstallData = directoryHeader.IsInInstallData,
                        DataInstallBaseOffset = directoryHeader.DataInstallBaseOffset,
                    };

                    for (int i = 0; i < directoryHeader.BatchCount; i++)
                    {
                        if ((directoryHeader.BatchTableOffset % 8) != 0)
                        {
                            throw new FormatException();
                        }

                        var batchHeader = batchHeaders[batchIndexBase + i];

                        if (isReborn == true && batchHeader.Flags == BatchFlags.None)
                        {
                            var batchBlockOffsets = new uint[batchHeader.FileCount];
                            data.Position = batchHeader.FileTableOffset;
                            for (int j = 0; j < batchHeader.FileCount; j++)
                            {
                                batchBlockOffsets[j] = data.ReadValueU32(endian);
                            }
                            ushort fileId = batchHeader.BaseFileId;
                            for (int j = 0; j < batchHeader.FileCount; j++, fileId++)
                            {
                                input.Position = totalSize + batchBlockOffsets[j] * 8;
                                var filePath = input.ReadStringZ(Encoding.ASCII);
                                var dataSize = input.ReadValueU32(endian);

                                FileEntry file;
                                file.Id = fileId;
                                file.NameHash = default;
                                file.DataBlockOffset = default;
                                file.DataSize = dataSize;
                                file.ExternalPath = filePath;
                                directory.Files.Add(file);
                            }
                        }
                        else
                        {
                            var readDataHeader = _FileTableEntryReaders[(int)batchHeader.Flags];
                            if (readDataHeader == null)
                            {
                                throw new NotSupportedException();
                            }

                            data.Position = batchHeader.FileTableOffset;
                            ushort fileId = batchHeader.BaseFileId;
                            for (int j = 0; j < batchHeader.FileCount; j++, fileId++)
                            {
                                var fileHeader = readDataHeader(data, endian);

                                uint? nameHash = null;
                                if (directoryHeader.NameTableCount > 0)
                                {
                                    if (directoryHeader.NameTableIndex == 0xFFFF)
                                    {
                                        throw new InvalidOperationException();
                                    }

                                    var nameIndex = Array.FindIndex(
                                        nameHeaders,
                                        directoryHeader.NameTableIndex,
                                        directoryHeader.NameTableCount,
                                        nte => nte.DirectoryId == directoryHeader.Id &&
                                               nte.FileId == fileId);
                                    if (nameIndex >= 0)
                                    {
                                        nameHash = nameHeaders[nameIndex].NameHash;
                                    }
                                }

                                FileEntry file;
                                file.Id = fileId;
                                file.NameHash = nameHash;
                                file.DataBlockOffset = fileHeader.DataBlockOffset;
                                file.DataSize = fileHeader.DataSize;
                                file.ExternalPath = default;
                                directory.Files.Add(file);
                            }
                        }
                    }

                    directories.Add(directory);
                }

                this.Endian = endian;
                this.IsReborn = isReborn;
                this.TitleId1 = titleId1;
                this.TitleId2 = titleId2;
                this.Unknown32 = unknown32;
                this.ParentalLevel = parentalLevel;
                this.InstallDataCryptoKey = installDataCryptoKey;
                this.Directories.Clear();
                this.Directories.AddRange(directories);
            }
        }
    }
}
