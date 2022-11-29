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
    public struct Sprite
    {
        public byte SheetIndex;
        public short CenterX;
        public short CenterY;
        public Cel[] Cels;

        public static Sprite ReadV0(Stream input, Endian endian)
        {
            Sprite instance;
            instance.SheetIndex = input.ReadValueU8();
            instance.CenterX = input.ReadValueS8();
            instance.CenterY = input.ReadValueS8();
            var celCount = input.ReadValueU8();
            instance.Cels = new Cel[celCount];
            for (int i = 0; i < celCount; i++)
            {
                instance.Cels[i] = Cel.ReadV0(input, endian);
            }
            return instance;
        }

        public static Sprite ReadV3(Stream input, Endian endian)
        {
            Sprite instance;
            instance.SheetIndex = input.ReadValueU8();
            var celCount = input.ReadValueU8();
            instance.CenterX = input.ReadValueS16(endian);
            instance.CenterY = input.ReadValueS16(endian);
            instance.Cels = new Cel[celCount];
            for (int i = 0; i < celCount; i++)
            {
                instance.Cels[i] = Cel.ReadV3(input, endian);
            }
            return instance;
        }

        public static void WriteV0(Stream output, Sprite instance, Endian endian)
        {
            if (instance.Cels.Length > byte.MaxValue)
            {
                throw new InvalidDataException();
            }
            var celCount = (byte)instance.Cels.Length;
            output.WriteValueU8(instance.SheetIndex);
            output.WriteValueS8((sbyte)instance.CenterX);
            output.WriteValueS8((sbyte)instance.CenterY);
            output.WriteValueU8(celCount);
            for (int i = 0; i < celCount; i++)
            {
                instance.Cels[i].WriteV0(output, endian);
            }
        }

        public void WriteV0(Stream output, Endian endian)
        {
            WriteV0(output, this, endian);
        }
    }
}
