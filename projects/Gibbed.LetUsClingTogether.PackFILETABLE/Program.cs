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
using Gibbed.IO;
using Gibbed.LetUsClingTogether.FileFormats;
using NDesk.Options;
using Newtonsoft.Json;
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
                { "v|verbose", "be verbose (list files)", v => verbose = v != null },
                { "h|help", "show this message and exit",  v => showHelp = v != null },
            };

            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extra.Count < 1 || extra.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_manifest [output_directory]", GetExecutableName());
                Console.WriteLine("Unpack specified archive.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string baseManifestInputPath = extra[0];
            string tableManifestPath;

            if (Directory.Exists(baseManifestInputPath) == true)
            {
                tableManifestPath = Path.Combine(baseManifestInputPath, "@manifest.json");
            }
            else
            {
                tableManifestPath = baseManifestInputPath;
                baseManifestInputPath = Path.GetDirectoryName(tableManifestPath);
            }

            string outputBasePath = extra.Count > 1 ? extra[1] : baseManifestInputPath + "_packed";

            var tableManifest = ReadManifest<FileTableManifest>(tableManifestPath);

            var table = new FileTableFile()
            {
                TitleId1 = tableManifest.TitleId1,
                TitleId2 = tableManifest.TitleId2,
                Unknown32 = tableManifest.Unknown32,
                ParentalLevel = tableManifest.ParentalLevel,
                InstallDataCryptoKey = tableManifest.InstallDataCryptoKey,
            };

            foreach (var directoryManifest in tableManifest.Directories)
            {
                var fileManifestPath = Path.Combine(baseManifestInputPath, CleanPathForManifest(directoryManifest.FileManifest));
                var fileManifests = ReadManifest<List<FileTableManifest.File>>(fileManifestPath);

                var baseInputPath = Path.GetDirectoryName(fileManifestPath);

                var outputPath = Path.Combine(outputBasePath, $"{directoryManifest.Id:X4}.BIN");

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
                    IsInInstallData = false,
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
                        using (var input = File.OpenRead(inputPath))
                        {
                            if (input.Length > uint.MaxValue)
                            {
                                throw new InvalidOperationException("file too large");
                            }
                            dataSize = (uint)input.Length;
                            output.WriteFromStream(input, dataSize);
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
                    //output.SetLength(dataBlockOffset * dataBlockSize);
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

        private static string CleanPathForManifest(string path)
        {
            return path.Replace('/', Path.DirectorySeparatorChar);
        }

        private static T ReadManifest<T>(string path)
        {
            var content = File.ReadAllText(path);
            using (var stringReader = new StringReader(content))
            using (var reader = new JsonTextReader(stringReader))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}
