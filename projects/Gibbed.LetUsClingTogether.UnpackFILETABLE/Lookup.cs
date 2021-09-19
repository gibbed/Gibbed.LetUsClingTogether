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

using System.Diagnostics;
using System.IO;
using Tommy;
using Tommy.Extensions;

namespace Gibbed.LetUsClingTogether.UnpackFILETABLE
{
    internal class Lookup
    {
        private static string GetExecutablePath()
        {
            using var process = Process.GetCurrentProcess();
            var path = Path.GetFullPath(process.MainModule.FileName);
            return Path.GetFullPath(path);
        }

        public static TomlTable Load()
        {
            var executablePath = GetExecutablePath();
            var binPath = Path.GetDirectoryName(executablePath);
            var filetablesPath = Path.Combine(binPath, "..", "configs", "filetables");

            var root = new TomlTable();
            if (Directory.Exists(filetablesPath) == false)
            {
                return root;
            }

            foreach (var inputPath in Directory.GetFiles(filetablesPath, "*.filetable.toml"))
            {
                TomlTable table;
                var inputBytes = File.ReadAllBytes(inputPath);
                using (var input = new MemoryStream(inputBytes, false))
                using (var reader = new StreamReader(input, true))
                {
                    table = TOML.Parse(reader);
                }

                TommyExtensionsWithBugfix.MergeWith(root, table, true);
            }

            return root;
        }
    }
}
