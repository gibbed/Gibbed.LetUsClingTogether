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

using System.Collections.Generic;
using Gibbed.IO;

namespace Gibbed.LetUsClingTogether.UnpackFILETABLE
{
    public class FileTableManifest
    {
        public FileTableManifest()
        {
            this.Directories = new List<Directory>();
        }

        public Endian Endian { get; set; }
        public string TitleId1 { get; set; }
        public string TitleId2 { get; set; }
        public byte Unknown32 { get; set; }
        public byte ParentalLevel { get; set; }
        public byte[] InstallDataCryptoKey { get; set; }
        public bool IsInInstallDataDefault { get; set; }
        public List<Directory> Directories { get; }

        public class Directory
        {
            public ushort Id { get; set; }
            public byte DataBlockSize { get; set; }
            public bool IsInInstallData { get; set; }
            public string FileManifest { get; set; }
        }

        public class File
        {
            public int? Id { get; set; }
            public uint? NameHash { get; set; }
            public string Name { get; set; }
            public PackId? PackId { get; set; }
            public bool IsZip { get; set; }
            public string ZipName { get; set; }
            public bool IsPack { get; set; }
            public string Path { get; set; }
        }

        public struct PackId
        {
            public ushort FileId { get; set; }
            public ushort DirectoryId { get; set; }

            public uint RawId { get { return (((uint)this.DirectoryId) << 16) | this.FileId; } }

            public PackId(uint rawId)
            {
                this.FileId = (ushort)(rawId & 0xFFFFu);
                this.DirectoryId = (ushort)((rawId >> 16) & 0xFFFFu);
            }

            public PackId(ushort directoryId, ushort fileId)
            {
                this.DirectoryId = directoryId;
                this.FileId = fileId;
            }

            public static PackId Create(uint rawId)
            {
                return new PackId(rawId);
            }

            public static PackId? Create(uint? rawId)
            {
                return rawId == null ? null : new PackId(rawId.Value);
            }

            public static PackId Create(ushort directoryId, ushort fileId)
            {
                return new PackId(directoryId, fileId);
            }
        }
    }
}
