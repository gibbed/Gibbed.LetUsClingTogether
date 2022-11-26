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
using Gibbed.TacticsOgre.FileFormats;
using NDesk.Options;
using FileTable = Gibbed.TacticsOgre.FileFormats.FileTable;

namespace Gibbed.Reborn.RebuildFileTable
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            Environment.ExitCode = MainInternal(args);
        }

        private static int MainInternal(string[] args)
        {
            string outputTablePath = null;
            bool verbose = false;
            bool showHelp = false;

            OptionSet options = new()
            {
                { "o|output=", "set output path instead of replacing the input file", v => outputTablePath = v },
                { "v|verbose", "be verbose", v => verbose = v != null },
                { "h|help", "show this message and exit",  v => showHelp = v != null },
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
                return -1;
            }

            if (extras.Count < 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_filetable [input_file]+", GetExecutableName());
                Console.WriteLine("Unpack specified archive.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return -2;
            }

            string inputTablePath = Path.GetFullPath(extras[0]);
            string inputBasePath;
            if (File.Exists(inputTablePath) == true)
            {
                inputBasePath = Path.GetDirectoryName(inputTablePath);
            }
            else
            {
                inputBasePath = inputTablePath;
                inputTablePath = Path.Combine(inputBasePath, "FileTable.bin");
            }
            if (string.IsNullOrEmpty(outputTablePath) == true)
            {
                outputTablePath = inputTablePath;
            }
            List<string> updatePaths = new(extras.Skip(1));

            FileTableFile table;
            using (var input = File.OpenRead(inputTablePath))
            {
                table = new FileTableFile();
                table.Deserialize(input);
            }

            if (table.IsReborn == false)
            {
                Console.WriteLine("[error] Not a Reborn file table.");
                return -3;
            }

            List<FileTable.FileEntry> filesToUpdate = new();
            if (updatePaths.Count > 0)
            {
                foreach (var updatePath in updatePaths)
                {
                    string filePath;
                    string externalPath;
                    if (Path.IsPathRooted(updatePath) == true)
                    {
                        externalPath = PathHelper.GetRelativePath(inputTablePath, updatePath);
                        if (Path.IsPathRooted(externalPath) == true ||
                            externalPath.StartsWith("..") == true)
                        {
                            Console.WriteLine($"[warning] Not relative to table path: {externalPath}");
                            continue;
                        }
                        filePath = updatePath;
                    }
                    else
                    {
                        externalPath = updatePath;
                        filePath = Path.Combine(inputBasePath, updatePath);
                    }
                    externalPath = externalPath
                        .Replace(Path.DirectorySeparatorChar, '/')
                        .Replace(Path.AltDirectorySeparatorChar, '/');

                    List<(FileTable.DirectoryEntry directory, int index)> targets = new();
                    foreach (var candidateDirectory in table.Directories)
                    {
                        var candidateIndex = candidateDirectory.Files
                            .FindIndex(f => f.ExternalPath == externalPath);
                        while (candidateIndex >= 0)
                        {
                            targets.Add((candidateDirectory, candidateIndex));
                            candidateIndex = candidateDirectory.Files
                                .FindIndex(candidateIndex + 1, f => f.ExternalPath == externalPath);
                        }
                    }

                    if (targets.Count == 0)
                    {
                        Console.WriteLine($"[error] External path does not exist in file table: {externalPath}");
                        return -4;
                    }

                    FileInfo fileInfo = new(filePath);
                    uint fileSize;
                    if (fileInfo.Exists == false)
                    {
                        Console.WriteLine($"[warning] File does not exist, assuming zero size: {filePath}");
                        fileSize = 0;
                    }
                    else
                    {
                        if (fileInfo.Length > uint.MaxValue)
                        {
                            Console.WriteLine($"[error] File is too large: {filePath}");
                            return -5;
                        }
                        fileSize = (uint)fileInfo.Length;
                    }

                    foreach (var (targetDirectory, targetIndex) in targets)
                    {
                        var targetFile = targetDirectory.Files[targetIndex];
                        targetFile.DataSize = fileSize;
                        targetDirectory.Files[targetIndex] = targetFile;
                    }
                }
            }
            else
            {
                Console.WriteLine("[warning] Not updating any specific paths.");
                Console.WriteLine("[warning] Checking everything. This might take longer.");
                foreach (var targetDirectory in table.Directories)
                {
                    for (int i = 0; i < targetDirectory.Files.Count; i++)
                    {
                        var targetFile = targetDirectory.Files[i];

                        string filePath = targetFile.ExternalPath;
                        filePath = filePath.Replace('/', Path.DirectorySeparatorChar);
                        filePath = Path.Combine(inputBasePath, filePath);

                        FileInfo fileInfo = new(filePath);
                        uint fileSize;
                        if (fileInfo.Exists == false)
                        {
                            if (targetFile.DataSize != 0)
                            {
                                Console.WriteLine($"[warning] File does not exist, assuming zero size: {filePath}");
                            }
                            fileSize = 0;
                        }
                        else
                        {
                            if (fileInfo.Length > uint.MaxValue)
                            {
                                Console.WriteLine($"[error] File is too large: {filePath}");
                                return -5;
                            }
                            fileSize = (uint)fileInfo.Length;
                        }

                        targetDirectory.Files[i] = targetFile;
                    }
                }
            }

            byte[] tableBytes;
            using (MemoryStream output = new())
            {
                table.Serialize(output);
                output.Flush();
                tableBytes = output.ToArray();
            }
            File.WriteAllBytes(outputTablePath, tableBytes);
            return 0;
        }
    }
}
