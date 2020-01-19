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

namespace Gibbed.TacticsOgre.UnpackPack
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

            using (var input = File.OpenRead(inputPath))
            {
                if (input.ReadValueU32() != 0x646B6170)
                {
                    throw new FormatException();
                }

                var count = input.ReadValueU32();
                var offsets = new uint[count];
                for (uint i = 0; i < count; i++)
                {
                    offsets[i] = input.ReadValueU32();
                }
                var end = input.ReadValueU32();

                for (uint i = 0; i < count; i++)
                {
                    uint offset = offsets[i];
                    uint nextOffset = i + 1 >= count ? end : offsets[i + 1];
                    uint size = nextOffset - offset;

                    input.Seek(offset, SeekOrigin.Begin);
                    var filePath = Path.Combine(outputPath, i.ToString());
                    var ext = FileExtensions.Detect(input, size);
                    filePath = Path.ChangeExtension(filePath, ext);

                    Console.WriteLine(filePath);
                    input.Seek(offset, SeekOrigin.Begin);
                    using (var output = File.Create(filePath))
                    {
                        output.WriteFromStream(input, size);
                    }
                }
            }
        }
    }
}
