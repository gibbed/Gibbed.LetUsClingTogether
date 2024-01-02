/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
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
using Gibbed.IO;
using Gibbed.Reborn.FileFormats;
using NDesk.Options;

namespace Gibbed.Reborn.UnpackPAC
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
                { "h|help", "show this message and exit", v => showHelp = v != null },
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_pac [output_directory]", GetExecutableName());
                Console.WriteLine("Unpack specified archive.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extras[0];
            string outputBasePath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, null) + "_unpacked";

            var inputBytes = File.ReadAllBytes(inputPath);

            using (var input = new MemoryStream(inputBytes, false))
            {
                var header = new ArcFile();
                header.Deserialize(input);

                var endian = header.Endian;

                var entryCount = header.Entries.Count;
                for (int i = 0; i < entryCount; i++)
                {
                    var entry = header.Entries[i];

                    var outputName = entry.Name;

                    if (outputName.Contains(".") == false)
                    {
                        outputName = $"{outputName}{TranslateTypeToSuffix(entry.Type)}";
                    }

                    var outputPath = Path.Combine(outputBasePath, outputName);

                    if (verbose == true)
                    {
                        Console.WriteLine(outputPath);
                    }

                    input.Position = entry.Offset;

                    var dataSize = input.ReadValueU32(endian);
                    var dataOffset = input.ReadValueU16(endian);
                    var dataUnknown = input.ReadValueU8();

                    input.Position = entry.Offset + dataOffset;

                    var outputParentPath = Path.GetDirectoryName(outputPath);
                    if (string.IsNullOrEmpty(outputParentPath) == false)
                    {
                        Directory.CreateDirectory(outputParentPath);
                    }

                    using (var output = File.Create(outputPath))
                    {
                        output.WriteFromStream(input, dataSize);
                    }
                }
            }
        }

        private static string TranslateTypeToSuffix(uint type) => type switch
        {
            0x006A706C => ".jpl",
            0x0074796C => ".lyt",
            0x00786574 => ".tex",
            _ => $".0x{type:X8}",
        };
    }
}
