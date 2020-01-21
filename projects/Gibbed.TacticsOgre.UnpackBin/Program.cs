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
using System.Text;
using Gibbed.IO;
using Gibbed.TacticsOgre.FileFormats;
using NDesk.Options;

namespace Gibbed.TacticsOgre.UnpackBin
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
            //  - name lookup for name hashes (FNV32)

            foreach (var directory in table.Directories)
            {
                var binPath = Path.Combine(inputBasePath, $"{directory.Id:X4}.BIN");
                var outputDirectoryPath = Path.Combine(outputBasePath, $"{directory.Id}");

                var idCounts = new Dictionary<long, int>();

                using (var input = File.OpenRead(binPath))
                {
                    foreach (var file in directory.Files)
                    {
                        var nameBuilder = new StringBuilder();
                        nameBuilder.Append($"{file.Id}");

                        var nameIndex = table.NameTable.FindIndex(nte => nte.DirectoryId == directory.Id &&
                                                                         nte.FileId == file.Id);
                        if (nameIndex >= 0)
                        {
                            nameBuilder.Append($"_{table.NameTable[nameIndex].NameHash:X8}");
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

                        var dataOffset = file.DataBlockOffset * 0x8000;

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
                    }
                }
            }
        }
    }
}
