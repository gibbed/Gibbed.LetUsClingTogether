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
using Gibbed.LetUsClingTogether.FileFormats;
using NDesk.Options;
using static Gibbed.LetUsClingTogether.FileFormats.InvariantShorthand;
using PackId = Gibbed.LetUsClingTogether.UnpackFILETABLE.FileTableManifest.PackId;

namespace Gibbed.LetUsClingTogether.UnpackFILETABLE
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            bool unpackNestedPacks = true;
            bool unpackNestedZIPs = true;
            bool verbose = false;
            bool showHelp = false;

            OptionSet options = new()
            {
                { "np|dont-unpack-nested-packs", "don't unpack nested .pack files", v => unpackNestedPacks = v == null },
                { "nz|dont-unpack-zips", "don't unpack nested .zip files", v => unpackNestedZIPs = v == null },
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_FILETABLE [output_directory]", GetExecutableName());
                Console.WriteLine("Unpack specified archive.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extras[0];
            string outputBasePath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, null) + "_unpacked";

            FileTableFile table;
            using (var input = File.OpenRead(inputPath))
            {
                table = new FileTableFile();
                table.Deserialize(input);
            }

            var rootLookup = Lookup.Load(table.IsReborn == false ? "psp" : "reborn");

            var inputBasePath = Path.GetDirectoryName(inputPath);

            // TODO(gibbed):
            //  - better name lookup for name hashes (FNV32)
            //    (don't hardcode the list)
            var names = new string[]
            {
                "MENU_COMMON_PACK",
                "MENU_TEXTURE_MISC_PACK",
                "MN_AT_ORGANIZE",
                "MN_BIRTHDAY",
                "MN_BT_MAIN",
                "MN_BT_RESULT",
                "MN_COMMON",
                "MN_COMMONWIN",
                "MN_EVENT",
                "MN_INPUT",
                "MN_ITEMICON",
                "MN_KEY_LAYOUT",
                "MN_MOVIE",
                "MN_NETWORK",
                "MN_OPTION",
                "MN_ORGANIZE",
                "MN_SHOP2",
                "MN_STAFFROLL",
                "MN_STATUS",
                "MN_TITLE",
                "MN_WARRENREPORT",
                "MN_WORLD",
            };
            var nameHashLookup = names.ToDictionary(v => v.HashFNV32(), v => v);

            var isInstallDataCounts = table.Directories
                .GroupBy(d => d.IsInInstallData)
                .OrderBy(v => v.Key)
                .Select(v => v.Count())
                .ToArray();

            var tableManifestPath = Path.Combine(outputBasePath, "@manifest.toml");
            FileTableManifest tableManifest = new()
            {
                Endian = table.Endian,
                TitleId1 = table.TitleId1,
                TitleId2 = table.TitleId2,
                Unknown32 = table.Unknown32,
                ParentalLevel = table.ParentalLevel,
                InstallDataCryptoKey = table.InstallDataCryptoKey,
                IsInInstallDataDefault =
                    isInstallDataCounts.Length == 2 &&
                    isInstallDataCounts[1] > isInstallDataCounts[0],
            };

            string language = table.TitleId1 switch
            {
                "ULJM05753" => "jp",
                "ULUS10565" => "en",
                "ULES10500" => "en",
                _ => null,
            };
            if (string.IsNullOrEmpty(language) == true)
            {
                Console.WriteLine($"Warning: unknown language for {table.TitleId1}.");
            }

            foreach (var directory in table.Directories)
            {
                var directoryPath = _($"{directory.Id}");
                var directoryLookup = rootLookup[directoryPath];

                var directoryLookupPath = directoryLookup["path"]?.AsString?.Value;
                if (directoryLookupPath != null)
                {
                    directoryPath = directoryLookupPath.Replace('/', Path.DirectorySeparatorChar);
                }

                TableDirectory tableDirectory = new()
                {
                    Id = directory.Id,
                    BasePath = Path.Combine(outputBasePath, directoryPath),
                    Lookup = directoryLookup,
                };

                List<IFileContainer> fileContainers = new()
                {
                    tableDirectory,
                };

                var binPath = Path.Combine(inputBasePath, _($"{directory.Id:X4}.BIN"));
                using (var input = File.OpenRead(binPath))
                {
                    Queue<QueuedFile> fileQueue = new();
                    foreach (var file in directory.Files)
                    {
                        long dataOffset;
                        dataOffset = directory.DataBaseOffset;
                        dataOffset += (file.DataBlockOffset << directory.DataBlockSize) * FileTableFile.BaseDataBlockSize;

                        fileQueue.Enqueue(new()
                        {
                            Id = file.Id,
                            Parent = tableDirectory,
                            NameHash = file.NameHash,
                            DataStream = input,
                            DataOffset = dataOffset,
                            DataSize = file.DataSize,
                        });
                    }

                    IFileContainer lastParent = null;
                    List<long> sceneIds = null;
                    List<long> eventEntryIds = null;
                    while (fileQueue.Count > 0)
                    {
                        var file = fileQueue.Dequeue();
                        var parent = file.Parent;

                        long id;

                        StringBuilder filePathBuilder = new();

                        if (file.PackRawId.HasValue == false)
                        {
                            id = file.Id;
                            filePathBuilder.Append(_($"{id}"));
                        }
                        else
                        {
                            id = file.PackRawId.Value;

                            if (string.IsNullOrEmpty(parent.PackFileType) == true)
                            {
                                var packId = PackId.Create(file.PackRawId).Value;
                                filePathBuilder.Append(_($"{packId.DirectoryId}_{packId.FileId}"));
                            }
                            else if (parent.PackFileType == "scenario")
                            {
                                if (lastParent == null || lastParent != parent)
                                {
                                    lastParent = parent;
                                    sceneIds = new();
                                    eventEntryIds = new();
                                    var sceneIdsNode = parent.Lookup["scene_ids"].AsArray;
                                    if (sceneIdsNode != null)
                                    {
                                        foreach (Tommy.TomlNode sceneIdNode in sceneIdsNode)
                                        {
                                            sceneIds.Add(sceneIdNode.AsInteger.Value);
                                        }
                                    }
                                    var eventEntryIdsNode = parent.Lookup["event_entry_ids"].AsArray;
                                    if (eventEntryIdsNode != null)
                                    {
                                        foreach (Tommy.TomlNode sceneIdNode in eventEntryIdsNode)
                                        {
                                            eventEntryIds.Add(sceneIdNode.AsInteger.Value);
                                        }
                                    }
                                }

                                byte scenarioType;
                                long scenarioId;
                                if (sceneIds.Contains(id) == true)
                                {
                                    // script
                                    scenarioType = 0;
                                    scenarioId = id;
                                }
                                else if (sceneIds.Contains(id - 0x08000) == true)
                                {
                                    // resources
                                    scenarioType = 1;
                                    scenarioId = id - 0x08000;
                                }
                                else if (sceneIds.Contains(id - 0x10000) == true)
                                {
                                    // messages
                                    scenarioType = 2;
                                    scenarioId = id - 0x10000;
                                }
                                else if (eventEntryIds.Contains(id - 0x0A000) == true)
                                {
                                    // actors
                                    scenarioType = 6;
                                    scenarioId = id - 0x0A000;
                                }
                                else
                                {
                                    GetScenarioId(id, out scenarioType, out scenarioId);
                                }

                                if (scenarioId < 0 || scenarioId > ushort.MaxValue)
                                {
                                    throw new InvalidOperationException();
                                }

                                // override pack ID
                                file.PackRawId = PackId.Create(scenarioType, (ushort)scenarioId).RawId;

                                id = file.PackRawId.Value;
                                filePathBuilder.Append(_($"{scenarioType}_{scenarioId}"));
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }

                        if (parent.IdCounts != null)
                        {
                            var idCounts = parent.IdCounts;
                            idCounts.TryGetValue(id, out int idCount);
                            idCount++;
                            idCounts[id] = idCount;
                            if (idCount > 1)
                            {
                                filePathBuilder.Append(_($"_DUP_{idCount}"));
                            }
                        }

                        string fileName;
                        if (file.NameHash == null)
                        {
                            fileName = null;
                        }
                        else if (nameHashLookup.TryGetValue(file.NameHash.Value, out fileName) == false)
                        {
                            fileName = null;
                        }

                        var filePath = filePathBuilder.ToString();

                        var fileLookup = parent.Lookup[filePath];
                        var fileLookupPath = fileLookup["path"]?.AsString?.Value;
                        if (fileLookupPath != null)
                        {
                            filePath = fileLookupPath.Replace('/', Path.DirectorySeparatorChar);
                        }
                        else if (file.NameHash != null)
                        {
                            if (fileName != null)
                            {
                                filePathBuilder.Append(_($"_{fileName}"));
                            }
                            else
                            {
                                filePathBuilder.Append(_($"_HASH[{file.NameHash.Value:X8}]"));
                            }
                        }

                        HandleFile(
                            file,
                            fileName,
                            filePath,
                            fileLookup,
                            fileQueue,
                            fileContainers,
                            verbose,
                            unpackNestedPacks,
                            unpackNestedZIPs);
                    }
                }

                foreach (var fileContainer in fileContainers)
                {
                    WriteManifest(fileContainer.ManifestPath, fileContainer, language);
                }

                tableManifest.Directories.Add(new()
                {
                    Id = directory.Id,
                    DataBlockSize = directory.DataBlockSize,
                    IsInInstallData = directory.IsInInstallData,
                    FileManifest = CleanPathForManifest(
                        PathHelper.GetRelativePath(outputBasePath, tableDirectory.ManifestPath)),
                });
            }

            WriteManifest(tableManifestPath, tableManifest);
        }

        private static void GetScenarioId(long id, out byte type, out long scenarioId)
        {
            switch (id)
            {
                case >= 0x08000 and <= 0x09FFF:
                {
                    // should have been caught already
                    throw new NotSupportedException();
                    /*
                    scenarioId = id - 0x08000;
                    type = 1;
                    return;
                    */
                }
                case >= 0x0A000 and <= 0x0AFFF:
                {
                    // should have been caught already
                    throw new NotSupportedException();
                    /*
                    type = 6;
                    scenarioId = id - 0x0A000;
                    return;
                    */
                }
                case >= 0x0D000 and <= 0x0DFFF:
                {
                    // entry unit list
                    type = 7;
                    scenarioId = id - 0x0D000;
                    return;
                }
                case >= 0x10000 and <= 0x10FFF:
                {
                    // messages
                    // should have been caught already
                    throw new NotSupportedException();
                    /*
                    type = 2;
                    scenarioId = id - 0x10000;
                    return;
                    */
                }
                case >= 0x11000 and <= 0x11FFF:
                {
                    // portraits
                    type = 3;
                    scenarioId = id - 0x11000;
                    return;
                }
                case >= 0x12000 and <= 0x12FFF:
                {
                    // animations
                    scenarioId = id - 0x12000;
                    type = 4;
                    return;
                }
                case >= 0x13000 and <= 0x13FFF:
                {
                    // sounds
                    scenarioId = id - 0x13000;
                    type = 8;
                    return;
                }
                case >= 0x50000 and <= 0x50FFF:
                {
                    // units
                    scenarioId = id - 0x50000;
                    type = 5;
                    return;
                }
            }
            throw new NotSupportedException();
        }

        private static string GetLookupString(Tommy.TomlNode node, string key)
        {
            var stringNode = node[key]?.AsString;
            return stringNode != null
                ? stringNode.Value
                : null;
        }

        private static void HandleFile(
            QueuedFile file,
            string fileName,
            string filePath,
            Tommy.TomlNode fileLookup,
            Queue<QueuedFile> fileQueue,
            List<IFileContainer> fileContainers,
            bool verbose,
            bool unpackNestedPacks,
            bool unpackNestedZIPs)
        {
            var parent = file.Parent;

            MemoryStream temp = null;
            Stream input = file.DataStream;
            long dataOffset = file.DataOffset;
            uint dataSize = file.DataSize;
            string zipName = null;
            var sheetFormat = fileLookup["sheet_format"]?.AsString?.Value;

            if (unpackNestedZIPs == true && dataSize >= 4)
            {
                input.Position = dataOffset;
                var fileMagic = input.ReadValueU32(Endian.Little);
                if (fileMagic == 0x04034B50) // 'PK\x03\x04'
                {
                    input.Position = dataOffset;
                    using (ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new(input))
                    {
                        zip.IsStreamOwner = false;

                        var zipEntry = zip.GetNextEntry();
                        if (zipEntry == null)
                        {
                            throw new InvalidOperationException();
                        }

                        if (zipEntry.Size > int.MaxValue)
                        {
                            throw new InvalidOperationException();
                        }

                        temp = zip.ReadToMemoryStream((int)zipEntry.Size);
                        input = temp;
                        dataOffset = 0;
                        dataSize = (uint)zipEntry.Size;
                        zipName = zipEntry.Name;

                        zipEntry = zip.GetNextEntry();
                        if (zipEntry != null)
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }

            if (unpackNestedPacks == true && dataSize >= 8)
            {
                input.Position = dataOffset;
                var fileMagic = input.ReadValueU32(Endian.Little);
                if (fileMagic == PackFile.Signature || fileMagic.Swap() == PackFile.Signature)
                {
                    input.Position = dataOffset;
                    var nestedPack = HandleNestedPack(input, fileQueue, file.Id, filePath, fileLookup, parent);
                    fileContainers.Add(nestedPack);
                    parent.FileManifests.Add(new()
                    {
                        Id = file.Id,
                        NameHash = file.NameHash,
                        Name = fileName,
                        IsZip = zipName != null,
                        ZipName = zipName,
                        IsPack = true,
                        PackId = PackId.Create(file.PackRawId),
                        SheetFormat = sheetFormat,
                        Path = CleanPathForManifest(PathHelper.GetRelativePath(parent.BasePath, nestedPack.ManifestPath)),
                    });
                    return;
                }
            }

            var outputPath = Path.Combine(parent.BasePath, filePath);

            var outputParentPath = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputParentPath) == false)
            {
                Directory.CreateDirectory(outputParentPath);
            }

            string fileExtension = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(fileExtension) == true)
            {
                input.Position = dataOffset;
                fileExtension = FileDetection.Guess(input, (int)dataSize, dataSize);
                outputPath = Path.ChangeExtension(outputPath, fileExtension);
            }

            if (verbose == true)
            {
                Console.WriteLine(outputPath);
            }

            input.Position = dataOffset;
            using (var output = File.Create(outputPath))
            {
                output.WriteFromStream(input, dataSize);
            }

            parent.FileManifests.Add(new()
            {
                Id = file.Id,
                NameHash = file.NameHash,
                Name = fileName,
                IsZip = zipName != null,
                ZipName = zipName,
                PackId = PackId.Create(file.PackRawId),
                SheetFormat = sheetFormat,
                Path = CleanPathForManifest(PathHelper.GetRelativePath(parent.BasePath, outputPath)),
            });
        }

        private static IFileContainer HandleNestedPack(
            Stream input,
            Queue<QueuedFile> fileQueue,
            int id,
            string path,
            Tommy.TomlNode lookup,
            IFileContainer parent)
        {
            var basePosition = input.Position;

            path = Path.ChangeExtension(path, null);

            PackFile packFile = new();
            packFile.Deserialize(input);

            NestedPack container = new()
            {
                Id = id,
                BasePath = Path.Combine(parent.BasePath, path),
                Parent = parent,
                Lookup = lookup,
                PackFileType = GetLookupString(lookup, "pack_file_type"),
            };

            var hasIds = packFile.Entries.Any(e => e.RawId != 0);
            var entryCount = packFile.Entries.Count;
            for (int i = 0; i < entryCount; i++)
            {
                var entry = packFile.Entries[i];
                uint nextEntryOffset = i + 1 < entryCount
                    ? packFile.Entries[i + 1].Offset
                    : packFile.TotalSize;
                uint entrySize = nextEntryOffset - entry.Offset;
                fileQueue.Enqueue(new()
                {
                    Id = i,
                    Parent = container,
                    PackRawId = hasIds == false ? (uint?)null : entry.RawId,
                    DataStream = input,
                    DataOffset = basePosition + entry.Offset,
                    DataSize = entrySize,
                });
            }

            return container;
        }

        private class QueuedFile
        {
            public int Id { get; set; }
            public IFileContainer Parent { get; set; }
            public uint? NameHash { get; set; }
            public uint? PackRawId { get; set; }
            public Stream DataStream { get; set; }
            public long DataOffset { get; set; }
            public uint DataSize { get; set; }
        }

        private interface IFileContainer
        {
            int Id { get; }
            string BasePath { get; }
            string ManifestPath { get; }
            IFileContainer Parent { get; }
            Dictionary<long, int> IdCounts { get; }
            List<FileTableManifest.File> FileManifests { get; }
            public Tommy.TomlNode Lookup { get; }
            public string PackFileType { get; }
        }

        private class TableDirectory : IFileContainer
        {
            public TableDirectory()
            {
                this.IdCounts = new Dictionary<long, int>();
                this.FileManifests = new List<FileTableManifest.File>();
            }

            public int Id { get; set; }
            public string BasePath { get; set; }
            public string ManifestPath { get { return Path.Combine(this.BasePath, "@manifest.toml"); } }
            public IFileContainer Parent { get { return null; } }
            public Dictionary<long, int> IdCounts { get; }
            public List<FileTableManifest.File> FileManifests { get; }
            public Tommy.TomlNode Lookup { get; set; }
            public string PackFileType { get { return null; } }
        }

        private class NestedPack : IFileContainer
        {
            public NestedPack()
            {
                this.IdCounts = new Dictionary<long, int>();
                this.FileManifests = new List<FileTableManifest.File>();
            }

            public int Id { get; set; }
            public string BasePath { get; set; }
            public string ManifestPath { get { return Path.Combine(this.BasePath, "@manifest.toml"); } }
            public IFileContainer Parent { get; set; }
            public Dictionary<long, int> IdCounts { get; }
            public List<FileTableManifest.File> FileManifests { get; }
            public Tommy.TomlNode Lookup { get; set; }
            public string PackFileType { get; set; }
        }

        private static string CleanPathForManifest(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, '/')
                       .Replace(Path.AltDirectorySeparatorChar, '/');
        }

        private static void WriteManifest(string path, FileTableManifest manifest)
        {
            Tommy.TomlArray directoryArray = new()
            {
                IsMultiline = true,
            };

            foreach (var directory in manifest.Directories)
            {
                Tommy.TomlTable directoryTable = new()
                {
                    IsInline = true,
                    ["id"] = directory.Id,
                };
                if (directory.DataBlockSize != 4)
                {
                    directoryTable["data_block_size"] = directory.DataBlockSize;
                }
                if (directory.IsInInstallData != manifest.IsInInstallDataDefault)
                {
                    directoryTable["in_install_data"] = directory.IsInInstallData;
                }
                directoryTable["file_manifest"] = directory.FileManifest;
                directoryArray.Add(directoryTable);
            }

            Tommy.TomlTable rootTable = new()
            {
                ["endian"] = _($"{manifest.Endian}"),
                ["title_id_1"] = manifest.TitleId1,
                ["title_id_2"] = manifest.TitleId2,
                ["unknown32"] = manifest.Unknown32,
                ["parental_level"] = manifest.ParentalLevel,
                ["install_data_crypto_key"] = Convert.ToBase64String(manifest.InstallDataCryptoKey),
                ["is_in_install_data_default"] = manifest.IsInInstallDataDefault,
                ["directories"] = directoryArray,
            };

            StringBuilder sb = new();
            using (StringWriter writer = new(sb))
            {
                rootTable.WriteTo(writer);
            }

            var pathParent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(pathParent) == false)
            {
                Directory.CreateDirectory(pathParent);
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void WriteManifest(string path, IFileContainer directory, string language)
        {
            Tommy.TomlArray fileArray = new()
            {
                IsMultiline = true,
            };

            foreach (var fileManifest in directory.FileManifests)
            {
                Tommy.TomlTable fileTable = new()
                {
                    IsInline = true,
                };

                if ((directory is NestedPack) == false)
                {
                    fileTable["id"] = fileManifest.Id;
                    if (fileManifest.Name != null)
                    {
                        fileTable["name"] = fileManifest.Name;
                    }
                    else if (fileManifest.NameHash != null)
                    {
                        fileTable["name_hash"] = fileManifest.NameHash.Value;
                    }
                }
                else
                {
                    if (fileManifest.PackId != null)
                    {
                        fileTable["pack_id"] = new Tommy.TomlTable()
                        {
                            IsInline = true,
                            ["dir"] = fileManifest.PackId.Value.DirectoryId,
                            ["file"] = fileManifest.PackId.Value.FileId,
                        };
                    }
                }

                if (fileManifest.IsZip == true)
                {
                    fileTable["zip"] = true;
                    fileTable["zip_name"] = fileManifest.ZipName;
                }

                if (fileManifest.IsPack == true)
                {
                    fileTable["pack"] = true;
                }

                if (string.IsNullOrEmpty(fileManifest.SheetFormat) == false)
                {
                    fileTable["sheet_format"] = fileManifest.SheetFormat;
                }

                fileTable["path"] = fileManifest.Path;

                fileArray.Add(fileTable);
            }

            Tommy.TomlTable rootTable = new();

            if (string.IsNullOrEmpty(language) == false)
            {
                rootTable["language"] = language;
            }

            if (string.IsNullOrEmpty(directory.PackFileType) == false)
            {
                rootTable["pack_file_type"] = directory.PackFileType;
            }

            rootTable["files"] = fileArray;

            StringBuilder sb = new();
            using (StringWriter writer = new(sb))
            {
                rootTable.WriteTo(writer);
            }

            var pathParent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(pathParent) == false)
            {
                Directory.CreateDirectory(pathParent);
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
    }
}
