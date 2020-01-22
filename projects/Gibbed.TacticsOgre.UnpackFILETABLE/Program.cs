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
using Gibbed.TacticsOgre.FileFormats;
using NDesk.Options;
using Newtonsoft.Json;

namespace Gibbed.TacticsOgre.UnpackFILETABLE
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_FILETABLE [output_directory]", GetExecutableName());
                Console.WriteLine("Unpack specified archive.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extra[0];
            string outputBasePath = extra.Count > 1 ? extra[1] : Path.ChangeExtension(inputPath, null) + "_unpacked";

            FileTableFile table;
            using (var input = File.OpenRead(inputPath))
            {
                table = new FileTableFile();
                table.Deserialize(input);
            }

            var inputBasePath = Path.GetDirectoryName(inputPath);

            // TODO(gibbed):
            //  - generate file index for successful repacking
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

            var tableManifestPath = Path.Combine(outputBasePath, "@manifest.json");
            var tableManifest = new FileTableManifest()
            {
                TitleId1 = table.TitleId1,
                TitleId2 = table.TitleId2,
                Unknown32 = table.Unknown32,
                ParentalLevel = table.ParentalLevel,
                InstallDataCryptoKey = table.InstallDataCryptoKey,
            };

            foreach (var directory in table.Directories)
            {
                var binPath = Path.Combine(inputBasePath, $"{directory.Id:X4}.BIN");
                var outputDirectoryPath = Path.Combine(outputBasePath, $"{directory.Id}");

                var idCounts = new Dictionary<long, int>();

                var fileManifest = new List<FileTableManifest.File>();
                using (var input = File.OpenRead(binPath))
                {
                    foreach (var file in directory.Files)
                    {
                        var nameBuilder = new StringBuilder();
                        nameBuilder.Append($"{file.Id}");

                        string name = null;
                        if (file.NameHash != null)
                        {
                            if (nameHashLookup.TryGetValue(file.NameHash.Value, out name) == true)
                            {
                                nameBuilder.Append($"_{name}");
                            }
                            else
                            {
                                nameBuilder.Append($"_HASH[{file.NameHash.Value:X8}]");
                            }
                        }

                        int idCount;
                        idCounts.TryGetValue(file.Id, out idCount);
                        idCount++;
                        idCounts[file.Id] = idCount;

                        if (idCount > 1)
                        {
                            nameBuilder.Append($"_DUP_{idCount}");
                        }

                        var outputPath = Path.Combine(outputDirectoryPath, nameBuilder.ToString());

                        var outputParentPath = Path.GetDirectoryName(outputPath);
                        if (string.IsNullOrEmpty(outputParentPath) == false)
                        {
                            Directory.CreateDirectory(outputParentPath);
                        }

                        var dataOffset = file.DataOffset;

                        input.Position = dataOffset;
                        var extension = FileExtensions.Detect(input, file.DataSize);
                        outputPath = Path.ChangeExtension(outputPath, extension);

                        if (verbose == true)
                        {
                            Console.WriteLine(outputPath);
                        }

                        input.Position = dataOffset;
                        using (var output = File.Create(outputPath))
                        {
                            output.WriteFromStream(input, file.DataSize);
                        }

                        fileManifest.Add(new FileTableManifest.File()
                        {
                            Id = file.Id,
                            NameHash = file.NameHash,
                            Name = name,
                            Path = CleanPathForManifest(PathHelper.GetRelativePath(outputDirectoryPath, outputPath)),
                        });
                    }
                }

                var fileManifestPath = Path.Combine(outputDirectoryPath, "@manifest.json");
                WriteManifest(fileManifestPath, fileManifest);

                tableManifest.Directories.Add(new FileTableManifest.Directory()
                {
                    Id = directory.Id,
                    IsInInstallData = directory.IsInInstallData,
                    FileManifest = CleanPathForManifest(PathHelper.GetRelativePath(outputBasePath, fileManifestPath)),
                });
            }

            WriteManifest(tableManifestPath, tableManifest);
        }

        private static string CleanPathForManifest(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, '/')
                       .Replace(Path.AltDirectorySeparatorChar, '/');
        }

        private static void WriteManifest(string path, FileTableManifest manifest)
        {
            string content;
            using (var stringWriter = new StringWriter())
            using (var writer = new JsonTextWriter(stringWriter))
            {
                writer.Indentation = 2;
                writer.IndentChar = ' ';
                writer.Formatting = Formatting.Indented;
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, manifest);
                writer.Flush();
                stringWriter.Flush();
                content = stringWriter.ToString();
            }
            File.WriteAllText(path, content, Encoding.UTF8);
        }

        private static void WriteManifest(string path, List<FileTableManifest.File> manifest)
        {
            string content;
            using (var stringWriter = new StringWriter())
            using (var writer = new JsonTextWriter(stringWriter))
            {
                writer.Indentation = 2;
                writer.IndentChar = ' ';
                writer.Formatting = Formatting.Indented;
                writer.WriteStartArray();
                foreach (var fileManifest in manifest)
                {
                    writer.WriteStartObject();
                    var oldFormatting = writer.Formatting;
                    writer.Formatting = Formatting.None;
                    writer.WritePropertyName("id");
                    writer.WriteValue(fileManifest.Id);
                    if (fileManifest.Name != null)
                    {
                        writer.WritePropertyName("name");
                        writer.WriteValue(fileManifest.Name);
                    }
                    else if (fileManifest.NameHash != null)
                    {
                        writer.WritePropertyName("name_hash");
                        writer.WriteValue(fileManifest.NameHash.Value);
                    }
                    writer.WritePropertyName("path");
                    writer.WriteValue(fileManifest.Path);
                    writer.WriteEndObject();
                    writer.Formatting = oldFormatting;
                }
                writer.WriteEndArray();
                writer.Flush();
                stringWriter.Flush();
                content = stringWriter.ToString();
            }
            File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
