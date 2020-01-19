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
