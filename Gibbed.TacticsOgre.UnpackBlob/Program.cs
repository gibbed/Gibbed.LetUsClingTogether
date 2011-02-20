using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Gibbed.Helpers;
using Gibbed.TacticsOgre.FileFormats;
using NDesk.Options;

namespace Gibbed.TacticsOgre.UnpackBlob
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
                var count = input.ReadValueU32();

                var ids = new uint[count];
                var sizes = new uint[count];

                for (uint i = 0; i < count; i++)
                {
                    ids[i] = input.ReadValueU32();
                    sizes[i] = input.ReadValueU32();
                }
                var end = (uint)input.Length;

                for (uint i = 0; i < count; i++)
                {
                    uint id = ids[i];
                    uint size = sizes[i];

                    long current = input.Position;
                    var ext = FileExtensions.Detect(input, size);
                    input.Seek(current, SeekOrigin.Begin);

                    var filePath = Path.Combine(outputPath,
                        string.Format("{0}_{1:X4}_{2:X2}_{3:X2}",
                        i,
                        (id & 0x0000FFFF) >> 0,
                        (id & 0x00FF0000) >> 16,
                        (id & 0xFF000000) >> 24));
                    filePath = Path.ChangeExtension(filePath, ext);

                    Console.WriteLine(filePath);
                    using (var output = File.Create(filePath))
                    {
                        output.WriteFromStream(input, size);
                    }
                }
            }
        }
    }
}
