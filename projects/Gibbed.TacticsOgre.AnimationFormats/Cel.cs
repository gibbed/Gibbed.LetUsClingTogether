/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.TacticsOgre.AnimationFormats
{
    public struct Cel
    {
        public short RelativeX;
        public short RelativeY;
        public ushort SheetWidth;
        public ushort SheetHeight;
        public ushort SheetX;
        public ushort SheetY;

        public static Cel ReadV0(Stream input, Endian endian)
        {
            Cel instance;
            instance.RelativeX = input.ReadValueS8();
            instance.RelativeY = input.ReadValueS8();
            instance.SheetWidth = input.ReadValueU8();
            instance.SheetHeight = input.ReadValueU8();
            instance.SheetX = input.ReadValueU16(endian);
            instance.SheetY = input.ReadValueU16(endian);
            return instance;
        }

        public static Cel ReadV3(Stream input, Endian endian)
        {
            Cel instance;
            instance.RelativeX = input.ReadValueS16(endian);
            instance.RelativeY = input.ReadValueS16(endian);
            instance.SheetWidth = input.ReadValueU16(endian);
            instance.SheetHeight = input.ReadValueU16(endian);
            instance.SheetX = input.ReadValueU16(endian);
            instance.SheetY = input.ReadValueU16(endian);
            return instance;
        }

        public static void WriteV0(Stream output, Cel instance, Endian endian)
        {
            output.WriteValueS8((sbyte)instance.RelativeX);
            output.WriteValueS8((sbyte)instance.RelativeY);
            output.WriteValueU8((byte)instance.SheetWidth);
            output.WriteValueU8((byte)instance.SheetHeight);
            output.WriteValueU16(instance.SheetX, endian);
            output.WriteValueU16(instance.SheetY, endian);
        }

        public void WriteV0(Stream output, Endian endian)
        {
            WriteV0(output, this, endian);
        }

        public override string ToString()
        {
            return $"{this.RelativeX},{this.RelativeY} @ {this.SheetWidth}x{this.SheetHeight} {this.SheetX},{this.SheetY}";
        }
    }
}
