/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
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
using System.Globalization;
using System.IO;
using Gibbed.IO;
using Gibbed.LetUsClingTogether.FileFormats;
using NDesk.Options;

namespace Gibbed.LetUsClingTogether.LookupFILETABLE
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
                return;
            }

            if (extras.Count < 3 || extras.Count > 3 || showHelp == true ||
                ParseArgument(extras[1], out uint directoryId) == false ||
                ParseArgument(extras[2], out long offset) == false)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_FILETABLE <dir> <offset>", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extras[0];

            FileTableFile table;
            using (var input = File.OpenRead(inputPath))
            {
                table = new FileTableFile();
                table.Deserialize(input);
            }

            var directoryIndex = table.Directories.FindIndex(de => de.Id == directoryId);
            if (directoryIndex < 0)
            {
                Console.WriteLine($"Directory ID {directoryId} not found.");
                return;
            }

            var directory = table.Directories[directoryIndex];
            var blockSize = FileTableFile.BaseDataBlockSize << directory.DataBlockSize;

            var fileIndex = directory.Files.FindIndex(
                fe => offset >= fe.DataBlockOffset * blockSize &&
                      offset < (fe.DataBlockOffset * blockSize) + fe.DataSize);
            if (fileIndex < 0)
            {
                Console.WriteLine($"Offset does not appear to correspond to file within directory ID {directoryId}.");
                return;
            }

            var inputBasePath = Path.GetDirectoryName(inputPath);
            var binPath = Path.Combine(inputBasePath, $"{directory.Id:X4}.BIN");

            var file = directory.Files[fileIndex];
            Console.WriteLine($"Directory ID: {directory.Id}");
            Console.WriteLine($" File ID: {file.Id}");

            if (file.DataSize < 8)
            {
                return;
            }

            long dataPosition;
            int level = 2;
            using (var input = File.OpenRead(binPath))
            {
                dataPosition = file.DataBlockOffset * blockSize;
                while (true)
                {
                    input.Position = dataPosition;
                    var fileMagic = input.ReadValueU32(Endian.Little);
                    if (fileMagic != PackFile.Signature && fileMagic.Swap() != PackFile.Signature)
                    {
                        break;
                    }

                    input.Position = dataPosition;
                    var packFile = new PackFile();
                    packFile.Deserialize(input);

                    int i;
                    long foundPosition = -1;
                    for (i = 0; i < packFile.Entries.Count; i++)
                    {
                        var entryOffset = packFile.Entries[i].Offset;
                        var nextEntryOffset = i + 1 < packFile.Entries.Count
                            ? packFile.Entries[i + 1].Offset
                            : packFile.TotalSize;
                        var startPosition = dataPosition + entryOffset;
                        var endPosition = dataPosition + nextEntryOffset;
                        if (offset >= startPosition && offset < endPosition)
                        {
                            foundPosition = startPosition;
                            break;
                        }
                    }

                    if (foundPosition < 0)
                    {
                        break;
                    }

                    Console.WriteLine($"{"".PadLeft(level)}Pack file index: {i}");
                    dataPosition = foundPosition;
                    level++;
                }
            }

            var relativePosition = offset - dataPosition;
            Console.WriteLine($"{"".PadLeft(level)}Offset: 0x{relativePosition:X} ({relativePosition})");
        }

        private static bool ParseArgument(string text, out long value)
        {
            if (text == null)
            {
                value = default;
                return false;
            }
            var numberStyles = NumberStyles.None;
            if (text.StartsWith("0x") == true)
            {
                text = text.Substring(2);
                numberStyles |= NumberStyles.AllowHexSpecifier;
            }
            return long.TryParse(text, numberStyles, CultureInfo.InvariantCulture, out value);
        }

        private static bool ParseArgument(string text, out uint value)
        {
            if (text == null)
            {
                value = default;
                return false;
            }
            var numberStyles = NumberStyles.None;
            if (text.StartsWith("0x") == true)
            {
                text = text.Substring(2);
                numberStyles |= NumberStyles.AllowHexSpecifier;
            }
            return uint.TryParse(text, numberStyles, CultureInfo.InvariantCulture, out value);
        }
    }
}
