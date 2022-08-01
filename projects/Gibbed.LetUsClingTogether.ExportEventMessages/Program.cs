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
using System.Linq;
using System.Text;
using Gibbed.LetUsClingTogether.FileFormats;
using NDesk.Options;

namespace Gibbed.LetUsClingTogether.ExportSprite
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
            English = EN,
            Japanese = JP,
            Default = EN,
        }

        private static LanguageOption ParseEncodingOption(string v)
        {
            if (Enum.TryParse<LanguageOption>(v, out var result) == false)
            {
                result = LanguageOption.Invalid;
            }
            return result;
        }

        public static void Main(string[] args)
        {
            var encoding = LanguageOption.Invalid;
            bool showHelp = false;

            var options = new OptionSet()
            {
                { "e|encoding=", "set character encoding", v => ParseEncodingOption(v) },
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_msg+", GetExecutableName());
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
                    inputPaths.AddRange(Directory.GetFiles(inputPath, "*.emes", SearchOption.AllDirectories));
                }
                else
                {
                    inputPaths.Add(inputPath);
                }
            }

            FileFormats.Text.Formatter englishFormatter = null;
            FileFormats.Text.Formatter japaneseFormatter = null;
            foreach (var inputPath in inputPaths.OrderBy(v => v))
            {
                var inputEncoding = encoding != LanguageOption.Invalid
                    ? encoding
                    : GetLanguageFromManifest(inputPath);
                var formatter = encoding switch
                {
                    LanguageOption.EN => englishFormatter ??= FileFormats.Text.Formatter.ForEN(),
                    LanguageOption.JP => japaneseFormatter ??= FileFormats.Text.Formatter.ForJP(),
                    _ => throw new NotSupportedException(),
                };

                var messages = new EventMessagesFile();
                using (var input = File.OpenRead(inputPath))
                {
                    messages.Deserialize(input, formatter);
                }

                string outputPath = Path.ChangeExtension(inputPath, null) + ".emsg.toml";
                Export(messages, encoding, outputPath);
            }
        }

        private static LanguageOption GetLanguageFromManifest(string path)
        {
            const LanguageOption defaultValue = LanguageOption.Default;

            var parentPath = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parentPath) == false)
            {
                return defaultValue;
            }

            var manifestPath = Path.Combine(parentPath, "@manifest.toml");
            if (File.Exists(manifestPath) == false)
            {
                return defaultValue;
            }

            Tommy.TomlTable rootTable;
            var inputBytes = File.ReadAllBytes(path);
            using (MemoryStream input = new(inputBytes, false))
            using (StreamReader reader = new(input, true))
            {
                rootTable = Tommy.TOML.Parse(reader);
            }

            var name = rootTable["language"].AsString?.Value;
            if (Enum.TryParse<LanguageOption>(name, true, out var result) == false)
            {
                return defaultValue;
            }
            return result;
        }

        private static void Export(
            EventMessagesFile messages,
            LanguageOption language,
            string outputPath)
        {
            var messagesArray = new Tommy.TomlArray()
            {
                IsTableArray = true,
            };

            foreach (var kv in messages.Entries)
            {
                var id = kv.Key;
                var (nameId, text) = kv.Value;

                var messageTable = new Tommy.TomlTable();
                messageTable["id"] = id;
                messageTable["name_id"] = nameId;
                messageTable["text"] = CreateTomlString(text);
                messagesArray.Add(messageTable);
            }

            var rootTable = new Tommy.TomlTable();

            if (language != LanguageOption.Default)
            {
                rootTable["language"] = language.ToString().ToLowerInvariant();
            }
            else
            {
                throw new NotSupportedException();
            }

            rootTable["message"] = messagesArray;

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
                tomlString.Value = "\n" + s;
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
