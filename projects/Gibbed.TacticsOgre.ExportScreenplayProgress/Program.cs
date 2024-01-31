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
using System.Text;
using Gibbed.IO;
using Gibbed.TacticsOgre.SheetFormats;
using NDesk.Options;

namespace Gibbed.TacticsOgre.ExportScreenplayProgress
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
            bool? isReborn = null;
            bool showHelp = false;

            OptionSet options = new()
            {
                { "r|reborn", "set is reborn", v => isReborn = v != null },
                { "v|verbose", "be verbose", v => verbose = v != null },
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

            if (extras.Count < 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_pgrs [output_toml]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            const string serializerName = "screenplay_progress";

            var inputPath = Path.GetFullPath(extras[0]);
            string outputPath = extras.Count > 1
                ? Path.GetFullPath(extras[1])
                : Path.ChangeExtension(inputPath, ".progress.toml");

            if (isReborn == null || serializerName == null)
            {
                var manifestPath = GetManifestPath(inputPath);
                if (string.IsNullOrEmpty(manifestPath) == false &&
                    File.Exists(manifestPath) == true)
                {
                    var inputName = Path.GetFileName(inputPath);
                    var (manifestIsReborn, _) = GetOptionsFromManifest(
                        manifestPath,
                        inputName);
                    if (isReborn == null)
                    {
                        isReborn = manifestIsReborn;
                    }
                }
            }

            var descriptorFactory = DescriptorLoader.Load(isReborn == true);

            if (descriptorFactory.TryGet(serializerName, out var serializerInfo) == false)
            {
                Console.WriteLine($"Failed to get descriptor for '{serializerName}'!");
                return;
            }

            Tommy.TomlArray rowsArray = new()
            {
                IsMultiline = true,
            };

            var inputBytes = File.ReadAllBytes(inputPath);
            using (MemoryStream input = new(inputBytes, false))
            {
                var basePosition = input.Position;

                uint signature = 0x53524750; // 'PGRS'
                var magic = input.ReadValueU32(Endian.Little);
                if (magic != signature && magic.Swap() != signature)
                {
                    throw new FormatException();
                }
                var endian = magic == signature ? Endian.Little : Endian.Big;

                var entrySize = input.ReadValueU16(endian);
                if (entrySize != serializerInfo.EntrySize)
                {
                    Console.WriteLine("Expected size {0}, got {1}??", serializerInfo.EntrySize, entrySize);
                    //return;
                }

                var entryCount = input.ReadValueU16(endian);

                var serializer = serializerInfo.Instantiate();
                using (MemoryStream data = new(inputBytes, 8, serializerInfo.EntrySize * entryCount, false))
                {
                    for (int i = 0; i < entryCount; i++)
                    {
                        rowsArray.Add(serializer.Export(data, endian, null));
                    }
                }
            }

            Tommy.TomlTable rootTable = new();
            rootTable["sheet_format"] = serializerName;
            rootTable["rows"] = rowsArray;

            StringBuilder sb = new();
            using (StringWriter writer = new(sb))
            {
                rootTable.WriteTo(writer);
            }

            var outputParentPath = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputParentPath) == false)
            {
                Directory.CreateDirectory(outputParentPath);
            }

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        private static string GetManifestPath(string path)
        {
            var parentPath = Path.GetDirectoryName(path);
            return string.IsNullOrEmpty(parentPath) == false
                ? Path.Combine(parentPath, "@manifest.toml")
                : null;
        }

        private static (bool isReborn, object dummy) GetOptionsFromManifest(
            string path,
            string name)
        {
            const bool isRebornDefault = false;

            Tommy.TomlTable rootTable;
            var inputBytes = File.ReadAllBytes(path);
            using (MemoryStream input = new(inputBytes, false))
            using (StreamReader reader = new(input, true))
            {
                rootTable = Tommy.TOML.Parse(reader);
            }

            var isReborn = rootTable["reborn"].AsBoolean?.Value ?? isRebornDefault;

            return (isReborn, null);
        }
    }
}
