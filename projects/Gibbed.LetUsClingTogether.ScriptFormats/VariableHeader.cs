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

namespace Gibbed.LetUsClingTogether.ScriptFormats
{
    public struct VariableHeader
    {
        public uint Flags;
        public uint Offset;

        public static VariableHeader Read(Stream input, Endian endian)
        {
            VariableHeader instance;
            instance.Flags = input.ReadValueU32(endian);
            instance.Offset = input.ReadValueU32(endian);

            if (false)
            {
                var idOrOffset = instance.Flags & 0xFFFFFF;
                var type = (instance.Flags >> 24) & 0x3F;
                var isLocal = (instance.Flags & 0x40000000) != 0;
                var unknown = (instance.Flags & 0x80000000) != 0;

                if (unknown == true)
                {
                    throw new NotSupportedException();
                }

                switch (type)
                {
                    case 1:
                    case 2:
                    case 7: // array
                    {
                        break;
                    }

                    default:
                    {
                        throw new NotSupportedException();
                    }
                }
            }

            return instance;
        }

        public override string ToString()
        {
            return $"{this.Flags:X8} {this.Offset:X8}";
        }
    }
}
