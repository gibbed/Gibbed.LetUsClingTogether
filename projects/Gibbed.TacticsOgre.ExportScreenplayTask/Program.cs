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
using Gibbed.TacticsOgre.FileFormats;
using Gibbed.TacticsOgre.FileFormats.Screenplay;
using NDesk.Options;
using static Gibbed.TacticsOgre.Extensions.InvariantShorthand;
using ValueType = Gibbed.TacticsOgre.FileFormats.Screenplay.ValueType;

namespace Gibbed.TacticsOgre.ExportScreenplayTask
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

            OptionSet options = new()
            {
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_task [output_toml]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPath = Path.GetFullPath(extras[0]);
            string outputPath = extras.Count > 1
                ? Path.GetFullPath(extras[1])
                : Path.ChangeExtension(inputPath, ".task.toml");

            ScreenplayTaskFile task = new();
            var inputBytes = File.ReadAllBytes(inputPath);
            using (var input = new MemoryStream(inputBytes, false))
            {
                task.Deserialize(input);
            }

            Tommy.TomlTable entriesTable = new()
            {
            };

            foreach (var kv in task.Entries)
            {
                var key = kv.Key;

                Tommy.TomlArray instructionsArray = new()
                {
                    IsMultiline = true,
                };
                foreach (var instruction in kv.Value)
                {
                    Tommy.TomlTable instructionTable = new();
                    instructionTable["op"] = _($"{instruction.Opcode}");
                    var opcodeInfo = instruction.Opcode.GetArguments();
                    if (opcodeInfo.TargetType != TargetType.None)
                    {
                        instructionTable["target"] = instruction.Target;
                    }
                    if (opcodeInfo.ValueType != ValueType.None)
                    {
                        instructionTable["value"] = instruction.Value;
                    }
                    instructionsArray.Add(instructionTable);
                }

                entriesTable.Add(_($"{key}"), instructionsArray);
            }

            Tommy.TomlTable rootTable = new();
            rootTable["endian"] = _($"{task.Endian}");
            rootTable["entries"] = entriesTable;

            StringBuilder sb = new();
            using (StringWriter writer = new(sb))
            {
                rootTable.WriteTo(writer);
            }

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }
    }
}
