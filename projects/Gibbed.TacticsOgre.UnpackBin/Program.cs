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
using System.Xml;
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

            OptionSet options = new OptionSet()
            {
                {
                    "v|verbose",
                    "be verbose (list files)",
                    v => verbose = v != null
                },

                {
                    "h|help",
                    "show this message and exit", 
                    v => showHelp = v != null
                },
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_table [output_directory]", GetExecutableName());
                Console.WriteLine("Unpack specified archive.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extra[0];
            string outputPath = extra.Count > 1 ? extra[1] : Path.ChangeExtension(inputPath, null) + "_unpacked";

            Directory.CreateDirectory(outputPath);

            var table = new TableFile();
            using (var input = File.OpenRead(inputPath))
            {
                table.Deserialize(input);
            }

            foreach (var source in table.Directories)
            {
                var sourcePath = Path.Combine(outputPath,
                    source.Id.ToString());
                var dataPath =
                    Path.Combine(Path.GetDirectoryName(inputPath),
                        string.Format("{0:X4}.bin", source.Id));
                Directory.CreateDirectory(sourcePath);

                using (var input = File.OpenRead(dataPath))
                {
                    foreach (var file in source.Files)
                    {
                        var filePath = Path.Combine(sourcePath,
                            Path.Combine(
                                file.Group.ToString(), 
                                file.Id.ToString()));
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        input.Seek(file.Offset, SeekOrigin.Begin);
                        var ext = FileExtensions.Detect(input, file.Size);
                        filePath = Path.ChangeExtension(filePath, ext);

                        Console.WriteLine(filePath);

                        input.Seek(file.Offset, SeekOrigin.Begin);
                        using (var output = File.Create(filePath))
                        {
                            output.WriteFromStream(input, file.Size);
                        }
                    }
                }
            }
        }
    }
}
