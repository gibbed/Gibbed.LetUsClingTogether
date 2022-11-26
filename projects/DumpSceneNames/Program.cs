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

            var options = new OptionSet()
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

            var chapterNames = new Dictionary<string, string>()
            {
                { "C1", "chapter_1" },
                { "C2a", Path.Combine("chapter_2", "chaos") },
                { "C2b", Path.Combine("chapter_2", "law") },
                { "C3a", Path.Combine("chapter_3", "neutral") },
                { "C3b", Path.Combine("chapter_3", "chaos") },
                { "C3c", Path.Combine("chapter_3", "law") },
                { "C4", "chapter_4" },
                { "EP", "epilogue" },
                { "DLC", "dlc" },
            };

            var chapterIds = new Dictionary<string, int>()
            {
                { "C1", 901 },
                { "C2a", 902 },
                { "C2b", 903 },
                { "C3a", 904 },
                { "C3b", 905 },
                { "C3c", 906 },
                { "C4", 907 },
                { "EP", 908 },
                { "DLC", 909 },
            };

            foreach (var screenplayBasePath in Directory.GetDirectories(screenplaysPath, "*", SearchOption.AllDirectories))
            {
                var screenplayPath = Directory.GetFiles(screenplayBasePath, "*.progress").SingleOrDefault();
                if (screenplayPath == null)
                {
                    continue;
                }

                var screenplayName = Path.GetFileNameWithoutExtension(screenplayPath);

                var chapterName = chapterNames[screenplayName];
                var chapterId = chapterIds[screenplayName];
                var chapterPath = Path.Combine(basePath, "stages", chapterName);
                chapterName = chapterName.Replace('\\', '/');

                var screenplayEntries = new List<(ushort stageId, ushort mapId, ushort sceneId)>();
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
                        var otherBytes = input.ReadBytes(42 - 6);
                        screenplayEntries.Add((stageId, mapId, sceneId));
                    }
                }

                var sceneIds = new List<ushort>();
                var lastStageId = 0;
                foreach (var (stageId, mapId, sceneId) in screenplayEntries)
                {
                    if (stageId != lastStageId)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"[{chapterId}.{stageId}]");
                        Console.WriteLine($"path = \"{screenplayName}_ST_{stageId:D3}\"");
                        Console.WriteLine();
                        lastStageId = stageId;
                    }

                    if (sceneIds.Contains(sceneId) == true)
                    {
                        continue;
                    }
                    sceneIds.Add(sceneId);

                    var scenePath = Path.Combine(chapterPath, $"{stageId}", $"0_{sceneId}.scene");
                    var resourcesPath = Path.Combine(chapterPath, $"{stageId}", $"0_{sceneId | 0x8000}.scene_resources");
                    var messagesPath = Path.Combine(chapterPath, $"{stageId}", $"1_{sceneId}.event_messages");

                    bool hadFile = false;
                    if (File.Exists(scenePath) == true)
                    {
                        var script = new ScriptFile();
                        using (var input = File.OpenRead(scenePath))
                        {
                            script.Deserialize(input);
                        }

                        var sceneName = script.SourceName;
                        if (sceneName.EndsWith(".src") == true)
                        {
                            sceneName = sceneName[0..^4];
                        }

                        {
                            var fileKey = $"0_{sceneId}.path".PadRight(12);
                            Console.WriteLine($"{fileKey} = \"scene_{sceneName}.scene\"");
                            hadFile = true;
                        }

                        if (File.Exists(resourcesPath) == true)
                        {
                            var fileKey = $"0_{sceneId | 0x8000}.path".PadRight(12);
                            Console.WriteLine($"{fileKey} = \"scene_{sceneName}.resources\"");
                            hadFile = true;
                        }

                        if (File.Exists(messagesPath) == true)
                        {
                            var fileKey = $"1_{sceneId}.path".PadRight(12);
                            Console.WriteLine($"{fileKey} = \"scene_{sceneName}.messages\"");
                            hadFile = true;
                        }
                    }

                    if (hadFile == true)
                    {
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
