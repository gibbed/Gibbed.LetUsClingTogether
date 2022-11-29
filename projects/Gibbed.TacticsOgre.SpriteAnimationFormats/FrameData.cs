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

using System.IO;
using Gibbed.IO;

namespace Gibbed.TacticsOgre.SpriteAnimationFormats
{
    internal struct FrameData
    {
        public byte SpriteIndex;
        public sbyte X1;
        public sbyte Y1;
        public byte Unknown2;
        public short X2;
        public short Y2;
        public short Unknown5;

        public ushort FrameIndex
        {
            get
            {
                ushort value = (byte)this.X1;
                value <<= 8;
                value |= this.SpriteIndex;
                return value;
            }
        }

        public ushort FlagIndex
        {
            get
            {
                ushort value = (byte)this.X1;
                value <<= 8;
                value |= this.SpriteIndex;
                return value;
            }
        }

        public static FrameData ReadV0(Stream input, Endian endian)
        {
            var spriteIndex = input.ReadValueU8(); // 4 : 8
            var x1 = input.ReadValueS8(); // 5 : 9
            var y1 = input.ReadValueS8(); // 6 : A
            var unknown5 = input.ReadValueU8(); // 7 : B
            var x2 = input.ReadValueU8(); // 8 : C
            var y2 = input.ReadValueU8(); // 9 : D
            var unknown2 = input.ReadValueU8(); // A : E
            input.SkipZeroes(1);

            FrameData instance;
            instance.SpriteIndex = spriteIndex;
            instance.X1 = x1;
            instance.Y1 = y1;
            instance.X2 = x2;
            instance.Y2 = y2;
            instance.Unknown2 = unknown2;
            instance.Unknown5 = unknown5;
            return instance;
        }

        public static FrameData ReadV3(Stream input, Endian endian)
        {
            var spriteIndex = input.ReadValueU8(); // 4
            var x1 = input.ReadValueS8(); // 5
            var y1 = input.ReadValueS8(); // 6
            var unknown2 = input.ReadValueU8(); // 7
            var x2 = input.ReadValueS16(endian); // 8
            var y2 = input.ReadValueS16(endian); // A
            var unknown5 = input.ReadValueS16(endian); // C

            FrameData instance;
            instance.SpriteIndex = spriteIndex;
            instance.X1 = x1;
            instance.Y1 = y1;
            instance.X2 = x2;
            instance.Y2 = y2;
            instance.Unknown2 = unknown2;
            instance.Unknown5 = unknown5;
            return instance;
        }
    }
}
