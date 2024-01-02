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
using Gibbed.IO;
using Gibbed.TacticsOgre.ScriptFormats;
using NDesk.Options;

namespace DumpSceneNames
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

            if (extras.Count != 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ base_path", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var basePath = extras[0];
            var screenplaysPath = Path.Combine(basePath, "screenplays");

            Dictionary<string, (int id, string path)> chapterInfos = new()
            {
                { "C1", (901, "chapter_1") },
                { "C2a", (902, Path.Combine("chapter_2", "chaos")) },
                { "C2b", (903, Path.Combine("chapter_2", "law")) },
                { "C3a", (904, Path.Combine("chapter_3", "neutral")) },
                { "C3b", (905, Path.Combine("chapter_3", "chaos")) },
                { "C3c", (906, Path.Combine("chapter_3", "law")) },
                { "C4", (907, "chapter_4") },
                { "EP", (908, "epilogue") },
                { "DLC", (909, "dlc") },
            };

            var screenplayBasePaths = Directory.GetDirectories(screenplaysPath, "*", SearchOption.AllDirectories);
            foreach (var screenplayBasePath in screenplayBasePaths.OrderBy(v => v))
            {
                var screenplayPath = Directory.GetFiles(screenplayBasePath, "*.pgrs").SingleOrDefault();
                if (screenplayPath == null)
                {
                    continue;
                }

                var screenplayName = Path.GetFileNameWithoutExtension(screenplayPath);
                if (chapterInfos.TryGetValue(screenplayName, out var chapterInfo) == false)
                {
                    Console.WriteLine($"# skipping {screenplayName}");
                    continue;
                }

                var (chapterId, chapterName) = chapterInfo;

                Console.WriteLine($"# {chapterId} {chapterName}");

                var chapterPath = Path.Combine(basePath, "scenarios", chapterName);
                chapterName = chapterName.Replace('\\', '/');

                List<(ushort stageId, ushort mapId, ushort sceneId, ushort entryUnitsId, ushort eventEntryId)> screenplayEntries = new();
                using (var input = File.OpenRead(screenplayPath))
                {
                    const Endian endian = Endian.Little;
                    input.Position = 6;
                    var count = input.ReadValueU16(endian);
                    for (int i = 0; i < count; i++)
                    {
                        var stageId = input.ReadValueU16(endian);
                        var mapId = input.ReadValueU16(endian);
                        var sceneId = input.ReadValueU16(endian);
                        input.Position += 14;
                        var eventEntryId = input.ReadValueU16(endian);
                        input.Position += 9;
                        var menuTaskId = input.ReadValueU8();
                        var menuTaskParam1 = input.ReadValueU16(endian);
                        input.Position += 8;

                        ushort entryUnitsId = 0;
                        if (menuTaskId == 1 || menuTaskId == 2 || menuTaskId == 3 || menuTaskId == 13)
                        {
                            entryUnitsId = menuTaskParam1;
                        }

                        screenplayEntries.Add((stageId, mapId, sceneId, entryUnitsId, eventEntryId));
                    }
                }

                foreach (var group in screenplayEntries.GroupBy(v => v.stageId).OrderBy(g => g.Key))
                {
                    var stageId = group.Key;

                    Console.WriteLine();
                    Console.WriteLine($"[{chapterId}.{stageId}]");
                    Console.WriteLine($"path = \"{screenplayName}_ST_{stageId:D3}\"");
                    Console.WriteLine();

                    Console.WriteLine($"pack_file_type = \"scenario\"");

                    var sceneIds = group.Select(v => v.sceneId).Where(v => v != 0).Distinct().OrderBy(v => v).ToArray();
                    var entryUnitIds = group.Select(v => v.entryUnitsId).Where(v => v != 0).Distinct().OrderBy(v => v).ToArray();
                    var eventEntryIds = group.Select(v => v.eventEntryId).Where(v => v != 0).Distinct().OrderBy(v => v).ToArray();

                    if (sceneIds.Length > 0)
                    {
                        Console.WriteLine($"scene_ids = [ {string.Join(", ", sceneIds.Select(v => v.ToString()))} ]");
                    }

                    if (eventEntryIds.Length > 0)
                    {
                        Console.WriteLine($"event_entry_ids = [ {string.Join(", ", eventEntryIds.Select(v => v.ToString()))} ]");
                    }

                    Console.WriteLine();

                    List<string> indexPaths = new();
                    foreach (var sceneId in sceneIds)
                    {
                        var scriptPath = Path.Combine(chapterPath, $"{stageId}", $"{sceneId}.script");
                        if (File.Exists(scriptPath) == false)
                        {
                            continue;
                        }

                        var sceneName = ReadScriptName(scriptPath);
                        {
                            var fileKey = $"0_{sceneId}.path".PadRight(12);
                            Console.WriteLine($"{fileKey} = \"scene_{sceneName}.script\"");
                        }

                        var indexId = 0x8000 + sceneId;
                        var indexPath = Path.Combine(chapterPath, $"{stageId}", $"{indexId}.idx");
                        if (File.Exists(indexPath) == true)
                        {
                            var fileKey = $"1_{sceneId}.path".PadRight(12);
                            Console.WriteLine($"{fileKey} = \"scene_{sceneName}.idx\"");
                            indexPaths.Add(indexPath);
                        }

                        var messageId = 0x10000 + sceneId;
                        var messagePath = Path.Combine(chapterPath, $"{stageId}", $"{messageId}.msg");
                        if (File.Exists(messagePath) == true)
                        {
                            var fileKey = $"2_{sceneId}.path".PadRight(12);
                            Console.WriteLine($"{fileKey} = \"scene_{sceneName}.msg\"");
                        }

                        Console.WriteLine();
                    }

                    List<string> actorPaths = new();
                    foreach (var eventEntryId in eventEntryIds)
                    {
                        var fileId = 0x0A000 + eventEntryId;
                        var filePath = Path.Combine(chapterPath, $"{stageId}", $"{fileId}.xlc");
                        if (File.Exists(filePath) == false)
                        {
                            continue;
                        }
                        var fileKey = $"6_{eventEntryId}.path".PadRight(12);
                        Console.WriteLine($"{fileKey} = \"actors_{eventEntryId}.xlc\"");
                        actorPaths.Add(filePath);
                    }

                    foreach (var entryUnitId in entryUnitIds)
                    {
                        var fileId = 0x0D000 + entryUnitId;
                        var filePath = Path.Combine(chapterPath, $"{stageId}", $"{fileId}.xlc");
                        if (File.Exists(filePath) == false)
                        {
                            throw new InvalidOperationException();
                        }
                        var fileKey = $"7_{entryUnitId}.path".PadRight(12);
                        Console.WriteLine($"{fileKey} = \"entry_units_{entryUnitId}.xlc\"");
                    }

                    Dictionary<ushort, List<ushort>> resourceMap = new();
                    foreach (var actorPath in actorPaths)
                    {
                        ReadActors(actorPath, resourceMap);
                    }
                    foreach (var indexPath in indexPaths)
                    {
                        ReadIndex(indexPath, resourceMap);
                    }

                    bool hadResource = false;
                    foreach (var kv in resourceMap.OrderBy(kv => kv.Key))
                    {
                        var typeId = kv.Key;
                        var (fileIdBase, extension, namePrefix) = GetIndexTypeInfo(typeId);
                        foreach (var resourceId in kv.Value.OrderBy(v => v).Distinct())
                        {
                            if (kv.Key == 3 && resourceId == 0xFFFF)
                            {
                                continue;
                            }
                            var fileId = fileIdBase + resourceId;
                            var filePath = Path.Combine(chapterPath, $"{stageId}", $"{fileId}{extension}");
                            if (File.Exists(filePath) == false &&
                                Directory.Exists(filePath) == false)
                            {
                                throw new InvalidOperationException();
                            }
                            var fileKey = $"{typeId}_{resourceId}.path".PadRight(12);
                            Console.WriteLine($"{fileKey} = \"{namePrefix}{resourceId}{extension}\"");
                            hadResource = true;
                        }
                    }

                    if (hadResource == true)
                    {
                        Console.WriteLine();
                    }
                }
            }
        }

        private static (uint fileIdBase, string extension, string namePrefix) GetIndexTypeInfo(ushort key) => key switch
        {
            3 => (0x11000, ".ashg", "portrait_"),
            4 => (0x12000, null, "animation_"),
            5 => (0x50000, null, "unit_"),
            8 => (0x13000, ".scd", "sound_"),
            _ => throw new NotSupportedException(),
        };

        private static void ReadActors(string inputPath, Dictionary<ushort, List<ushort>> map)
        {
            var inputBytes = File.ReadAllBytes(inputPath);
            using MemoryStream input = new(inputBytes, false);

            const Endian endian = Endian.Little;

            var magic = input.ReadValueU32(endian);
            if (magic != 0x65636C78) // 'xlce'
            {
                throw new FormatException();
            }

            var entryCount = input.ReadValueU32(endian);
            var dataOffset = input.ReadValueU32(endian);
            var entrySize = input.ReadValueU32(endian);

            if (dataOffset != 16 || entrySize != 12)
            {
                throw new FormatException();
            }

            for (int i = 0; i < entryCount; i++)
            {
                var id = input.ReadValueU16(endian);
                var portraitId = input.ReadValueU16(endian);
                var animationId = input.ReadValueU16(endian);
                var spriteId = input.ReadValueU16(endian);
                var unknown2 = input.ReadValueU8();
                var unknown3 = input.ReadValueU8();
                var unknown4 = input.ReadValueU8();
                var unknown5 = input.ReadValueU8();

                if (portraitId != 0)
                {
                    if (map.TryGetValue(3, out var portraitIds) == false)
                    {
                        portraitIds = map[3] = new();
                    }
                    portraitIds.Add(portraitId);
                }

                if (animationId != 0)
                {
                    if (map.TryGetValue(4, out var animationIds) == false)
                    {
                        animationIds = map[4] = new();
                    }
                    animationIds.Add(animationId);
                }

                if (spriteId != 0)
                {
                    if (map.TryGetValue(5, out var spriteIds) == false)
                    {
                        spriteIds = map[5] = new();
                    }
                    spriteIds.Add(spriteId);
                }
            }
        }

        private static void ReadIndex(string inputPath, Dictionary<ushort, List<ushort>> map)
        {
            var inputBytes = File.ReadAllBytes(inputPath);
            using MemoryStream input = new(inputBytes, false);

            const Endian endian = Endian.Little;

            var magic = input.ReadValueU32(endian);
            if (magic != 0x30584449) // 'IDX0'
            {
                throw new FormatException();
            }

            var typeCount = input.ReadValueU16(endian);

            var padding = input.ReadValueU16(endian);
            if (padding != 0xFFFF)
            {
                throw new FormatException();
            }

            var typeOffsets = new uint[typeCount];
            for (int i = 0; i < typeCount; i++)
            {
                typeOffsets[i] = input.ReadValueU32(endian);
            }

            for (int i = 0; i < typeCount; i++)
            {
                input.Position = typeOffsets[i];

                var typeId = input.ReadValueU16(endian);
                var actualId = GetResourceTypeFromIndexType(typeId);

                if (map.TryGetValue(actualId, out var resourceIds) == false)
                {
                    resourceIds = map[actualId] = new();
                }

                var resourceCount = input.ReadValueU16(endian);
                for (int j = 0; j < resourceCount; j++)
                {
                    resourceIds.Add(input.ReadValueU16(endian));
                }
            }
        }

        private static ushort GetResourceTypeFromIndexType(ushort id) => id switch
        {
            1 => 3,
            2 => 4,
            3 => 8,
            _ => throw new NotSupportedException(),
        };

        private static string ReadScriptName(string scriptPath)
        {
            string name;
            ScriptFile script = new();
            using (var input = File.OpenRead(scriptPath))
            {
                script.Deserialize(input);
            }
            name = script.SourceName;
            if (name.EndsWith(".src") == true)
            {
                name = name[0..^4];
            }
            return name;
        }
    }
}
