﻿/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
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
using Gibbed.IO;
using Gibbed.LetUsClingTogether.FileFormats;
using NDesk.Options;
using static Gibbed.LetUsClingTogether.FileFormats.InvariantShorthand;
using FileTable = Gibbed.LetUsClingTogether.FileFormats.FileTable;
using FileTableManifest = Gibbed.LetUsClingTogether.UnpackFILETABLE.FileTableManifest;

namespace Gibbed.LetUsClingTogether.PackFILETABLE
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            bool verbose = false;
            bool showHelp = false;

            var options = new OptionSet()
            {
                { "v|verbose", "be verbose", v => verbose = v != null },
                { "h|help", "show this message and exit",  v => showHelp = v != null },
            };

            List<string> extras;
            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_manifest [output_directory]", GetExecutableName());
                Console.WriteLine("Unpack specified archive.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string baseManifestInputPath = extras[0];
            string tableManifestPath;

            if (Directory.Exists(baseManifestInputPath) == true)
            {
                tableManifestPath = Path.Combine(baseManifestInputPath, "@manifest.toml");
            }
            else
            {
                tableManifestPath = baseManifestInputPath;
                baseManifestInputPath = Path.GetDirectoryName(tableManifestPath);
            }

            string outputBasePath = extras.Count > 1 ? extras[1] : baseManifestInputPath + "_packed";

            FileTableManifest tableManifest;
            ReadManifest(tableManifestPath, out tableManifest);

            var table = new FileTableFile()
            {
                Endian = tableManifest.Endian,
                TitleId1 = tableManifest.TitleId1,
                TitleId2 = tableManifest.TitleId2,
                Unknown32 = tableManifest.Unknown32,
                ParentalLevel = tableManifest.ParentalLevel,
                InstallDataCryptoKey = tableManifest.InstallDataCryptoKey,
            };

            var endian = table.Endian;

            foreach (var directoryManifest in tableManifest.Directories)
            {
                var fileManifestPath = Path.Combine(baseManifestInputPath, CleanPathForManifest(directoryManifest.FileManifest));
                List<FileTableManifest.File> fileManifests;
                ReadManifest(fileManifestPath, out fileManifests);

                var baseInputPath = Path.GetDirectoryName(fileManifestPath);

                var outputPath = Path.Combine(outputBasePath, _($"{directoryManifest.Id:X4}.BIN"));

                var outputParentPath = Path.GetDirectoryName(outputPath);
                if (string.IsNullOrEmpty(outputParentPath) == false)
                {
                    Directory.CreateDirectory(outputParentPath);
                }

                var directory = new FileTable.DirectoryEntry()
                {
                    Id = directoryManifest.Id,
                    DataBaseOffset = 0,
                    DataBlockSize = directoryManifest.DataBlockSize,
                    // TODO(gibbed): don't currently support building install data
                    IsInInstallData = false,
                    DataInstallBaseOffset = 0, // directoryManifest.IsInInstallData == false ? 0u : 0xCCCCCCCCu,
                };

                using (var output = File.Create(outputPath))
                {
                    var dataBlockSize = (uint)FileTableFile.BaseDataBlockSize << directory.DataBlockSize;
                    uint dataBlockOffset = 0;

                    foreach (var fileManifest in fileManifests.OrderBy(fm => fm.Id))
                    {
                        var inputPath = Path.Combine(baseInputPath, CleanPathForManifest(fileManifest.Path));

                        output.Position = dataBlockOffset * dataBlockSize;

                        uint dataSize;
                        using (var temp = new MemoryStream())
                        {
                            if (fileManifest.IsPack == false)
                            {
                                using (var input = File.OpenRead(inputPath))
                                {
                                    if (input.Length > uint.MaxValue)
                                    {
                                        throw new InvalidOperationException("file too large");
                                    }
                                    dataSize = (uint)input.Length;
                                    temp.WriteFromStream(input, dataSize);
                                }
                            }
                            else
                            {
                                dataSize = HandleNestedPack(inputPath, temp, endian);
                            }

                            if (fileManifest.IsZip == false)
                            {
                                temp.Position = 0;
                                output.WriteFromStream(temp, dataSize);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }

                        var file = new FileTable.FileEntry()
                        {
                            Id = (ushort)fileManifest.Id,
                            NameHash = fileManifest.Name != null ? fileManifest.Name.HashFNV32() : fileManifest.NameHash,
                            DataBlockOffset = dataBlockOffset,
                            DataSize = dataSize,
                        };

                        if (dataSize > 0)
                        {
                            var dataBlockCount = dataSize.Align((uint)dataBlockSize) / dataBlockSize;
                            dataBlockOffset += dataBlockCount;
                        }

                        directory.Files.Add(file);
                    }
                }

                table.Directories.Add(directory);
            }

            var tablePath = Path.Combine(outputBasePath, "FILETABLE.BIN");

            var tableParentPath = Path.GetDirectoryName(tablePath);
            if (string.IsNullOrEmpty(tableParentPath) == false)
            {
                Directory.CreateDirectory(tableParentPath);
            }

            byte[] tableBytes;
            using (var output = new MemoryStream())
            {
                table.Serialize(output);
                output.Flush();
                tableBytes = output.ToArray();
            }

            File.WriteAllBytes(tablePath, tableBytes);
        }

        private static uint HandleNestedPack(
            string rootFileManifestPath,
            Stream output,
            Endian endian)
        {
            var opStack = new Stack<PackOperation>();
            opStack.Push(new PackOperation()
            {
                Type = PackOperationType.File,
                Path = rootFileManifestPath,
                IsPack = true,
            });

            var paddingBytes = new byte[16];

            var basePosition = output.Position;
            while (opStack.Count > 0)
            {
                var op = opStack.Pop();
                if (op.Type == PackOperationType.Pad)
                {
                    output.WriteBytes(paddingBytes);
                    continue;
                }

                if (op.Type == PackOperationType.Header)
                {
                    var endPosition = output.Position;

                    var nestedPack = op.Parent;

                    var packFile = new PackFile()
                    {
                        Endian = endian,
                    };

                    var count = nestedPack.Entries.Count;

                    var hasIds = nestedPack.Entries.Any(e => e.RawId != null);

                    for (int i = 0; i < count; i++)
                    {
                        var entry = nestedPack.Entries[i];
                        var entryOffset = (uint)(entry.Position - nestedPack.HeaderPosition);
                        packFile.Entries.Add(new PackFile.Entry(entry.RawId ?? 0, entryOffset));

                        if (i + 1 < count)
                        {
                            var nextEntry = nestedPack.Entries[i + 1];
                            if (entry.Position + entry.Size > nextEntry.Position)
                            {
                                throw new InvalidOperationException();
                            }
                        }
                    }

                    uint totalSize;
                    if (count > 0)
                    {
                        var lastEntry = nestedPack.Entries[count - 1];
                        totalSize = (uint)((lastEntry.Position - nestedPack.HeaderPosition) + lastEntry.Size);
                    }
                    else
                    {
                        totalSize = (uint)(nestedPack.DataPosition - nestedPack.HeaderPosition);
                    }

                    packFile.TotalSize = totalSize;

                    output.Position = nestedPack.HeaderPosition;
                    packFile.Serialize(output);

                    if (output.Position > nestedPack.DataPosition)
                    {
                        throw new InvalidOperationException();
                    }

                    if (nestedPack.Parent != null)
                    {
                        var previousParentEntry = nestedPack.Parent.Entries[nestedPack.ParentIndex];
                        nestedPack.Parent.Entries[nestedPack.ParentIndex] = new NestedPackEntry(
                            nestedPack.HeaderPosition,
                            packFile.TotalSize + 16 /* padding */,
                            previousParentEntry.RawId);
                    }

                    output.Position = endPosition;
                    continue;
                }

                if (op.Type != PackOperationType.File)
                {
                    throw new NotSupportedException();
                }

                if (op.IsPack == false)
                {
                    long dataPosition = output.Position;
                    uint dataSize;
                    using (var input = File.OpenRead(op.Path))
                    {
                        if (input.Length > uint.MaxValue)
                        {
                            throw new InvalidOperationException("file too large");
                        }
                        dataSize = (uint)input.Length;
                        output.WriteFromStream(input, dataSize);
                    }
                    op.Parent.Entries.Add(new NestedPackEntry(dataPosition, dataSize, op.PackId));
                }
                else
                {
                    var basePath = Path.GetDirectoryName(op.Path);
                    List<FileTableManifest.File> fileManifests;
                    ReadManifest(op.Path, out fileManifests);

                    var headerPosition = output.Position;

                    var hasIds = fileManifests.Any(fm => fm.PackId != null);
                    var headerSize = PackFile.GetHeaderSize(fileManifests.Count, hasIds);
                    output.Position += headerSize;

                    var dataPosition = output.Position;

                    var nestedPack = new NestedPack()
                    {
                        HeaderPosition = headerPosition,
                        DataPosition = dataPosition,
                    };

                    if (op.Parent != null)
                    {
                        nestedPack.Parent = op.Parent;
                        nestedPack.ParentIndex = op.Parent.Entries.Count;
                        op.Parent.Entries.Add(new NestedPackEntry(-1, 0, op.PackId));
                    }

                    opStack.Push(new PackOperation()
                    {
                        Type = PackOperationType.Header,
                        Parent = nestedPack,
                    });

                    if (op.Parent != null)
                    {
                        opStack.Push(new PackOperation()
                        {
                            Type = PackOperationType.Pad,
                            Parent = nestedPack,
                        });
                    }

                    foreach (var fileManifest in fileManifests.AsEnumerable().Reverse())
                    {
                        opStack.Push(new PackOperation()
                        {
                            Type = PackOperationType.File,
                            Parent = nestedPack,
                            Path = Path.Combine(basePath, CleanPathForManifest(fileManifest.Path)),
                            IsPack = fileManifest.IsPack,
                            PackId = fileManifest.PackId?.RawId,
                        });
                    }
                }
            }

            return (uint)(output.Position - basePosition);
        }

        private class PackOperation
        {
            public PackOperationType Type { get; set; }
            public NestedPack Parent { get; set; }
            public string Path { get; set; }
            public bool IsPack { get; set; }
            public uint? PackId { get; set; }
            public bool IsZip { get; set; }
            public string ZipFileName { get; set; }
        }

        private enum PackOperationType
        {
            Invalid = 0,
            File,
            Pad,
            Header,
        }

        private class NestedPack
        {
            public NestedPack()
            {
                this.Entries = new List<NestedPackEntry>();
            }

            public NestedPack Parent { get; set; }
            public int ParentIndex { get; set; }
            public long HeaderPosition { get; set; }
            public long DataPosition { get; set; }
            public List<NestedPackEntry> Entries { get; }
        }

        private struct NestedPackEntry
        {
            public NestedPackEntry(long position, uint size, uint? id)
            {
                this.Position = position;
                this.Size = size;
                this.RawId = id;
            }

            public uint? RawId { get; set; }
            public long Position { get; set; }
            public uint Size { get; set; }

            public override string ToString()
            {
                return $"@{this.Position} {this.Size} => {this.Position + this.Size})";
            }
        }

        private static string CleanPathForManifest(string path)
        {
            return path.Replace('/', Path.DirectorySeparatorChar);
        }

        private static void ReadManifest(string path, out FileTableManifest manifest)
        {
            Tommy.TomlTable rootTable;
            var inputBytes = File.ReadAllBytes(path);
            using (MemoryStream input = new(inputBytes, false))
            using (StreamReader reader = new(input, true))
            {
                rootTable = Tommy.TOML.Parse(reader);
            }

            if (TryParseEnum<Endian>(rootTable["endian"], out var endian) == false)
            {
                throw new FormatException();
            }

            manifest = new FileTableManifest();
            manifest.Endian = endian;
            manifest.TitleId1 = rootTable["title_id_1"]?.AsString?.Value ?? throw new FormatException();
            manifest.TitleId2 = rootTable["title_id_2"]?.AsString?.Value ?? throw new FormatException();
            manifest.Unknown32 = (byte)(rootTable["unknown32"]?.AsInteger?.Value ?? throw new FormatException());
            manifest.ParentalLevel = (byte)(rootTable["parental_level"]?.AsInteger?.Value ?? throw new FormatException());
            manifest.InstallDataCryptoKey = Convert.FromBase64String(rootTable["install_data_crypto_key"]?.AsString?.Value ?? throw new FormatException());
            manifest.IsInInstallDataDefault = rootTable["is_in_install_data_default"]?.AsBoolean?.Value ?? throw new FormatException();
            foreach (Tommy.TomlTable directoryTable in rootTable["directories"])
            {
                var directory = new FileTableManifest.Directory();
                directory.Id = (ushort)(directoryTable["id"]?.AsInteger?.Value ?? throw new FormatException());
                directory.DataBlockSize = (byte)(directoryTable["data_block_size"]?.AsInteger?.Value ?? 4);
                directory.IsInInstallData = directoryTable["is_in_install_data"]?.AsBoolean?.Value ?? manifest.IsInInstallDataDefault;
                directory.FileManifest = directoryTable["file_manifest"]?.AsString?.Value ?? throw new FormatException();
                manifest.Directories.Add(directory);
            }
        }

        private static void ReadManifest(string path, out List<FileTableManifest.File> manifests)
        {
            Tommy.TomlTable rootTable;
            var inputBytes = File.ReadAllBytes(path);
            using (MemoryStream input = new(inputBytes, false))
            using (StreamReader reader = new(input, true))
            {
                rootTable = Tommy.TOML.Parse(reader);
            }

            var packFileType = rootTable["pack_file_type"].AsString?.Value;

            manifests = new List<FileTableManifest.File>();
            foreach (Tommy.TomlTable fileTable in rootTable["files"])
            {
                var manifest = new FileTableManifest.File();
                manifest.Id = (int?)(fileTable["id"]?.AsInteger?.Value ?? null);
                manifest.NameHash = (uint?)(fileTable["name_hash"]?.AsInteger?.Value ?? null);
                manifest.Name = fileTable["name"]?.AsString?.Value;
                manifest.PackId = TranslatePackId(packFileType, ReadManifestPackId(fileTable["pack_id"]?.AsTable));
                manifest.IsZip = fileTable["zip"]?.AsBoolean?.Value ?? false;
                manifest.ZipName = fileTable["zip_name"]?.AsString?.Value;
                manifest.IsPack = fileTable["pack"]?.AsBoolean?.Value ?? false;
                //manifest.SheetFormat = fileTable["sheet_format"]?.AsString;
                manifest.Path = fileTable["path"]?.AsString?.Value;
                manifests.Add(manifest);
            }
        }

        private static FileTableManifest.PackId? TranslatePackId(string type, FileTableManifest.PackId? packId) => type switch
        {
            null => packId,
            "scenario" => TranslateScenarioPackId(packId),
            _ => throw new NotSupportedException(),
        };

        private static FileTableManifest.PackId? TranslateScenarioPackId(FileTableManifest.PackId? packId)
        {
            if (packId == null)
            {
                throw new ArgumentNullException(nameof(packId));
            }
            return TranslateScenarioPackId(packId.Value);
        }

        private static FileTableManifest.PackId TranslateScenarioPackId(FileTableManifest.PackId packId)
        {
            uint baseId = packId.DirectoryId switch
            {
                0 => 0, // scripts
                1 => 0x08000, // resources
                6 => 0x0A000, // actor list
                7 => 0x0D000, // entry unit list
                2 => 0x10000, // messages
                3 => 0x11000, // portraits
                4 => 0x12000, // animations
                8 => 0x13000, // sounds
                5 => 0x50000, // units
                _ => throw new NotSupportedException()
            };
            return FileTableManifest.PackId.Create(baseId + packId.FileId);
        }

        private static FileTableManifest.PackId? ReadManifestPackId(Tommy.TomlTable table)
        {
            if (table == null)
            {
                return null;
            }
            FileTableManifest.PackId packId = default;
            packId.DirectoryId = (ushort)(table["dir"]?.AsInteger?.Value ?? throw new FormatException());
            packId.FileId = (ushort)(table["file"]?.AsInteger?.Value ?? throw new FormatException());
            return packId;
        }

        private static bool TryParseEnum<T>(Tommy.TomlNode node, out T value)
            where T : struct
        {
            var s = node?.AsString?.Value;
            if (string.IsNullOrEmpty(s) == true)
            {
                value = default;
                return false;
            }
            if (Enum.TryParse(s, true, out value) == false)
            {
                value = default;
                return false;
            }
            return true;
        }
    }
}
