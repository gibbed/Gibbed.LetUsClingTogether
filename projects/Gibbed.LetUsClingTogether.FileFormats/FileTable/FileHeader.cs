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
using System.IO;
using Gibbed.IO;

namespace Gibbed.LetUsClingTogether.FileFormats.FileTable
{
    internal struct FileHeader
    {
        public uint DataBlockOffset;
        public uint DataSize;

        public static FileHeader Read2(Stream input, Endian endian)
        {
            FileHeader instance;
            instance.DataBlockOffset = input.ReadValueU16(endian);
            instance.DataSize = input.ReadValueU16(endian);
            return instance;
        }

        public static FileHeader Read3(Stream input, Endian endian)
        {
            FileHeader instance;
            instance.DataBlockOffset = input.ReadValueU16(endian);
            instance.DataSize = input.ReadValueU32(endian);
            return instance;
        }

        public static FileHeader Read4(Stream input, Endian endian)
        {
            FileHeader instance;
            instance.DataBlockOffset = input.ReadValueU32(endian);
            instance.DataSize = input.ReadValueU16(endian);
            return instance;
        }

        public static FileHeader Read5(Stream input, Endian endian)
        {
            FileHeader instance;
            instance.DataBlockOffset = input.ReadValueU32(endian);
            instance.DataSize = input.ReadValueU32(endian);
            return instance;
        }

        public static void Write2(Stream output, FileHeader instance, Endian endian)
        {
            if (instance.DataBlockOffset > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("data offset too large", nameof(instance));
            }

            if (instance.DataSize > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("data offset too large", nameof(instance));
            }

            output.WriteValueU16((ushort)instance.DataBlockOffset, endian);
            output.WriteValueU16((ushort)instance.DataSize, endian);
        }

        public static void Write3(Stream output, FileHeader instance, Endian endian)
        {
            if (instance.DataBlockOffset > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("data offset too large", nameof(instance));
            }

            output.WriteValueU16((ushort)instance.DataBlockOffset, endian);
            output.WriteValueU32(instance.DataSize, endian);
        }

        public static void Write4(Stream output, FileHeader instance, Endian endian)
        {
            if (instance.DataSize > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("data offset too large", nameof(instance));
            }

            output.WriteValueU32(instance.DataBlockOffset, endian);
            output.WriteValueU16((ushort)instance.DataSize, endian);
        }

        public static void Write5(Stream output, FileHeader instance, Endian endian)
        {
            output.WriteValueU32(instance.DataBlockOffset, endian);
            output.WriteValueU32(instance.DataSize, endian);
        }
    }
}
