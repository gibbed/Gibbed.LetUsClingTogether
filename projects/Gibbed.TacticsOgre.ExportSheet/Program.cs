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
using Gibbed.IO;
using Gibbed.TacticsOgre.FileFormats.Text;
using Gibbed.TacticsOgre.SheetFormats;
using NDesk.Options;

namespace Gibbed.TacticsOgre.ExportSheet
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

        private static LanguageOption ParseLanguageOption(string v)
        {
            if (Enum.TryParse<LanguageOption>(v, out var result) == false)
            {
                result = LanguageOption.Invalid;
            }
            return result;
        }

        public static void Main(string[] args)
        {
            var endian = Endian.Little;
            bool? isReborn = null;
            var language = LanguageOption.Invalid;
            string serializerName = null;
            bool verbose = false;
            bool showHelp = false;

            var options = new OptionSet()
            {
                { "r|reborn", "set is reborn", v => isReborn = v != null },
                { "e|language=", "set language", v => ParseLanguageOption(v) },
                { "s|serializer=", "specify serializer to use", v => serializerName = v },
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_xlc [output_toml]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPath = Path.GetFullPath(extras[0]);
            string outputPath = extras.Count > 1
                ? Path.GetFullPath(extras[1])
                : Path.ChangeExtension(inputPath, ".xlc.toml");

            if (isReborn == null || language == LanguageOption.Invalid)
            {
                var manifestPath = GetManifestPath(inputPath);
                if (string.IsNullOrEmpty(manifestPath) == false)
                {
                    var (manifestIsReborn, manifestLanguage) = GetOptionsFromManifest(manifestPath);
                    if (isReborn == null)
                    {
                        isReborn = manifestIsReborn;
                    }
                    if (language == LanguageOption.Invalid)
                    {
                        language = manifestLanguage;
                    }
                }
            }

            var formatter = language switch
            {
                LanguageOption.EN => Formatter.ForEN(),
                LanguageOption.JP => Formatter.ForJP(),
                _ => throw new NotSupportedException(),
            };

            var descriptorFactory = DescriptorLoader.Load(isReborn == false ? "psp" : "reborn");

            Tommy.TomlArray rowsArray;

            var inputBytes = File.ReadAllBytes(inputPath);
            using (MemoryStream input = new(inputBytes, false))
            {
                var basePosition = input.Position;

                var signature = new byte[] { 0x78, 0x6C, 0x63 }; // 'xlc'
                var magic = input.ReadBytes(3);
                if (magic.SequenceEqual(signature) == false)
                {
                    throw new FormatException();
                }
                
                var version = input.ReadValueU8();
                if (version != 101)
                {
                    throw new FormatException();
                }

                var entryCount = input.ReadValueS32(endian);
                var dataOffset = input.ReadValueS32(endian);
                var entrySize = input.ReadValueS32(endian);

                if (entryCount < 0)
                {
                    throw new FormatException();
                }

                if (dataOffset != 16)
                {
                    throw new FormatException();
                }

                if (entrySize < 0)
                {
                    throw new FormatException();
                }

                var totalEntrySize = entrySize * entryCount;
                var totalHeaderSize = (dataOffset + totalEntrySize).Align(16);
                var totalFileSize = input.Length - basePosition;
                var hasExtraData = totalFileSize > totalHeaderSize + 16;

                DescriptorInfo serializerInfo;
                if (string.IsNullOrEmpty(serializerName) == true)
                {
                    var serializerInfos = descriptorFactory.Get(entrySize, hasExtraData).ToArray();
                    if (serializerInfos.Length == 0)
                    {
                        Console.WriteLine($"No serializer possible for this file found. Size = {entrySize}, extra data = {hasExtraData}.");
                        return;
                    }
                    else if (serializerInfos.Length >= 2)
                    {
                        Console.WriteLine("There are multiple serializers possible for this file:");
                        foreach (var kv in serializerInfos)
                        {
                            Console.WriteLine($" {kv.Key}");
                        }
                        Console.WriteLine("Please specify one with -s.");
                        return;
                    }
                    serializerName = serializerInfos[0].Key;
                    serializerInfo = serializerInfos[0].Value;
                }
                else
                {
                    if (descriptorFactory.TryGet(serializerName, out serializerInfo) == false)
                    {
                        Console.WriteLine($"Specified serializer '{serializerName}' not found.");
                        return;
                    }

                    if (serializerInfo.EntrySize != entrySize)
                    {
                        Console.WriteLine($"Serializer size = {serializerInfo.EntrySize} does not match file size = {entrySize}.");
                        return;
                    }
                }

                var serializer = serializerInfo.Instantiate();

                rowsArray = new()
                {
                    IsMultiline = true,
                    IsTableArray = serializerInfo.RowsAsTableArray,
                };

                Dictionary<uint, List<Tommy.TomlString>> pendingStrings = new();
                using (MemoryStream data = new(inputBytes, dataOffset, totalEntrySize, false))
                {
                    for (int i = 0; i < entryCount; i++)
                    {
                        rowsArray.Add(serializer.Export(data, endian, pendingStrings));
                    }
                }

                using (MemoryStream data = new(inputBytes, (int)basePosition, (int)totalFileSize, false))
                {
                    foreach (var kv in pendingStrings)
                    {
                        data.Position = kv.Key;
                        var value = formatter.Decode(data, endian);
                        foreach (var node in kv.Value)
                        {
                            node.Value = value;
                        }
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

        private static (bool isReborn, LanguageOption language) GetOptionsFromManifest(string path)
        {
            const bool isRebornDefault = false;
            const LanguageOption languageDefault = LanguageOption.Default;

            Tommy.TomlTable rootTable;
            var inputBytes = File.ReadAllBytes(path);
            using (MemoryStream input = new(inputBytes, false))
            using (StreamReader reader = new(input, true))
            {
                rootTable = Tommy.TOML.Parse(reader);
            }

            var isReborn = rootTable["reborn"].AsBoolean?.Value ?? isRebornDefault;
            var languageName = rootTable["language"].AsString?.Value;
            LanguageOption language;
            if (Enum.TryParse(languageName, true, out language) == false)
            {
                language = languageDefault;
            }
            return (isReborn, language);
        }
    }
}
