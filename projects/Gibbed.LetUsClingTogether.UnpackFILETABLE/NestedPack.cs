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

using System.Collections.Generic;
using System.IO;

namespace Gibbed.LetUsClingTogether.UnpackFILETABLE
{
    internal class NestedPack : IFileContainer
    {
        public NestedPack()
        {
            this.IdCounts = new();
            this.FileManifests = new();
        }

        public int Id { get; set; }
        public string BasePath { get; set; }
        public string ManifestPath { get { return Path.Combine(this.BasePath, "@manifest.toml"); } }
        public IFileContainer Parent { get; set; }
        public Dictionary<long, int> IdCounts { get; }
        public List<FileTableManifest.File> FileManifests { get; }
        public Tommy.TomlNode Lookup { get; set; }
        public bool IsObfuscated { get { return false; } }
        public string PackFileType { get; set; }
    }
}
