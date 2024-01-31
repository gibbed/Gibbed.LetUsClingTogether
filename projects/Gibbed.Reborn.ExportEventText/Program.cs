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
using System.Linq;
using System.Text;
using Gibbed.Reborn.FileFormats;
using NDesk.Options;

namespace Gibbed.Reborn.ExportEventText
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;

            var options = new OptionSet()
            {
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_etxt+", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPaths = new List<string>();
            foreach (var inputPath in extras)
            {
                if (Directory.Exists(inputPath) == true)
                {
                    inputPaths.AddRange(Directory.GetFiles(inputPath, "*.etxt", SearchOption.AllDirectories));
                }
                else
                {
                    inputPaths.Add(inputPath);
                }
            }

            foreach (var inputPath in inputPaths.OrderBy(v => v))
            {
                EventTextFile text = new();
                using (var input = File.OpenRead(inputPath))
                {
                    text.Deserialize(input);
                }

                string outputPath = Path.ChangeExtension(inputPath, null) + ".etxt.toml";
                Export(text, outputPath);
            }
        }

        private static void Export(EventTextFile text, string outputPath)
        {
            var textArray = new Tommy.TomlArray()
            {
                IsTableArray = true,
            };

            foreach (var entry in text.Entries)
            {
                var messageTable = new Tommy.TomlTable();
                messageTable["id"] = entry.Id;
                if (string.IsNullOrEmpty(entry.Value) == false)
                {
                    messageTable["text"] = CreateTomlString(entry.Value);
                }
                if (string.IsNullOrEmpty(entry.Unknown10) == false)
                {
                    messageTable["u10"] = CreateTomlString(entry.Unknown10);
                }
                if (string.IsNullOrEmpty(entry.Unknown18) == false)
                {
                    messageTable["u18"] = CreateTomlString(entry.Unknown18);
                }
                textArray.Add(messageTable);
            }

            var rootTable = new Tommy.TomlTable();

            rootTable["text"] = textArray;

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                rootTable.WriteTo(writer);
                writer.Flush();
            }
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        private static Tommy.TomlString CreateTomlString(string s)
        {
            var tomlString = new Tommy.TomlString();
            if (s.Contains("\r") == false && s.Contains("\n") == false)
            {
                tomlString.Value = s;
                if (s.Contains("'") == false)
                {
                    tomlString.PreferLiteral = true;
                }
            }
            else
            {
                tomlString.Value = s;
                tomlString.MultilineTrimFirstLine = true;
                tomlString.IsMultiline = true;
                if (s.Contains("'''") == false)
                {
                    tomlString.PreferLiteral = true;
                }
            }
            return tomlString;
        }
    }
}
