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
using System.Linq;
using Gibbed.IO;
using Gibbed.LetUsClingTogether.FileFormats.Screenplay;
using ValueType = Gibbed.LetUsClingTogether.FileFormats.Screenplay.ValueType;

namespace Gibbed.LetUsClingTogether.FileFormats
{
    public abstract class BaseTaskFile
    {
        protected static int ReadTaskTarget(TargetType type, Stream input, Endian endian)
        {
            return type switch
            {
                TargetType.None => default,
                TargetType.GlobalFlag => input.ReadValueU16(endian),
                TargetType.LocalFlag => input.ReadValueU16(endian),
                TargetType.DungeonFlag => input.ReadValueU16(endian),
                TargetType.SystemSaveFlag => input.ReadValueU16(endian),
                TargetType.ApprovalRate => input.ReadValueU8(),
                TargetType.SystemPlatform => input.ReadValueU8(),
                TargetType.UnknownUInt8 => input.ReadValueU8(),
                TargetType.UnknownUInt16 => input.ReadValueU16(endian),
                _ => throw new NotImplementedException(),
            };
        }

        protected static int ReadTaskValue(ValueType type, Stream input, Endian endian)
        {
            return type switch
            {
                ValueType.None => default,
                ValueType.Bool => input.ReadValueU8(),
                ValueType.Int8 => input.ReadValueS8(),
                ValueType.UInt8 => input.ReadValueU8(),
                ValueType.Int16 => input.ReadValueS16(endian),
                ValueType.UInt16 => input.ReadValueU16(endian),
                ValueType.Int32 => input.ReadValueS32(endian),
                _ => throw new NotImplementedException(),
            };
        }

        protected static void CheckAndSkipPadding(Stream input, int size)
        {
            var paddingSize = (int)input.Position.Padding(4);
            if (paddingSize == 0)
            {
                return;
            }

            var paddingBytes = input.ReadBytes(paddingSize);
            if (paddingBytes.Any(b => b != 0xFF) == true)
            {
                throw new FormatException();
            }
        }
    }
}
