/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
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
using Gibbed.LetUsClingTogether.FileFormats;
using NDesk.Options;
using Text = Gibbed.TacticsOgre.FileFormats.Text;

namespace Gibbed.LetUsClingTogether.ExportEventMessages
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        private enum LanguageOption
        {
            Invalid = 0,
            EN,
            JP,
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_toml+", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPath = extras[0];
            string outputPath;
            if (extras.Count > 1)
            {
                outputPath = extras[1];
            }
            else if (inputPath.EndsWith(".emes.toml") == true)
            {
                outputPath = inputPath.Substring(0, inputPath.Length - 5);
            }
            else
            {
                outputPath = Path.ChangeExtension(inputPath, ".emes");
            }

            Tommy.TomlTable rootTable;
            var inputBytes = File.ReadAllBytes(inputPath);
            using (var input = new MemoryStream(inputBytes, false))
            using (var reader = new StreamReader(input, true))
            {
                rootTable = Tommy.TOML.Parse(reader);
            }

            var language = LanguageOption.EN;
            if (rootTable["language"] is Tommy.TomlString languageName)
            {
                if (Enum.TryParse(languageName, out language) == false ||
                    language == LanguageOption.Invalid)
                {
                    Console.WriteLine("Invalid language '{0}' specified.", languageName.Value);
                    return;
                }
            }

            var messages = new EventMessagesFile();

            foreach (Tommy.TomlTable messageTable in rootTable["message"])
            {
                var id = messageTable["id"].AsInteger.Value;
                var nameId = messageTable["name_id"].AsInteger.Value;
                var text = messageTable["text"].AsString.Value;

                if (id < ushort.MinValue || id > ushort.MaxValue)
                {
                    Console.WriteLine("Message ID '{0}' is invalid.", id);
                    return;
                }

                if (nameId < ushort.MinValue || nameId > ushort.MaxValue)
                {
                    Console.WriteLine("Name ID '{1}' for message ID '{0}' is invalid.", id, nameId);
                    return;
                }

                if (messages.Entries.ContainsKey((ushort)id) == true)
                {
                    Console.WriteLine("Duplicate message ID '{0}'", id);
                    return;
                }

                text = text.Replace("\r\n", "\n").Replace("\r", "");
                messages.Entries[(ushort)id] = ((ushort)nameId, text);
            }

            var formatter = language switch
            {
                LanguageOption.EN => Text.Formatter.ForEN(),
                LanguageOption.JP => Text.Formatter.ForJP(),
                _ => throw new NotSupportedException(),
            };
            using (var output = File.Create(outputPath))
            {
                messages.Serialize(output, formatter);
            }
        }
    }
}
