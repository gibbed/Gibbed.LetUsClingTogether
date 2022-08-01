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

namespace Gibbed.LetUsClingTogether.FileFormats.Sprite
{
    public struct Palette
    {
        public uint[] Colors;
        public GEFormats.Command[] GECommands;

        public static Palette Read(Stream input, Endian endian)
        {
            var basePosition = input.Position;

            var totalSize = input.ReadValueU32(endian);
            if (basePosition + totalSize > input.Length)
            {
                throw new EndOfStreamException();
            }

            var dataSize = input.ReadValueU32(endian);
            var dataOffset = input.ReadValueU32(endian);
            var commandOffset = input.ReadValueU32(endian);
            const int commandSize = 32;

            if (dataOffset != 0x10)
            {
                throw new FormatException();
            }
            else if (dataOffset + dataSize != commandOffset)
            {
                throw new FormatException();
            }

            if (dataOffset + dataSize + commandSize != totalSize)
            {
                throw new InvalidOperationException();
            }

            if (dataSize % 4 != 0)
            {
                throw new FormatException();
            }

            var colors = new uint[dataSize / 4];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = input.ReadValueU32(endian);
            }

            var geCommands = new GEFormats.Command[8];
            for (int i = 0; i < geCommands.Length; i++)
            {
                geCommands[i] = new GEFormats.Command(input.ReadValueU32(endian));
            }

            Palette instance;
            instance.Colors = colors;
            instance.GECommands = geCommands;
            instance.ValidateGECommands();
            return instance;
        }

        private void ValidateGECommands()
        {
            var cloadArgument = this.Colors.Length / 8;
            var commands = this.GECommands;
            if (commands[0] != new GEFormats.Command(GEFormats.Operation.CMODE, 65283) ||
                commands[1] != new GEFormats.Command(GEFormats.Operation.CBPH, 0) ||
                commands[2] != new GEFormats.Command(GEFormats.Operation.CBP, 0) ||
                commands[3] != GEFormats.Operation.CLOAD || commands[3].Argument != cloadArgument ||
                commands[4] != new GEFormats.Command(GEFormats.Operation.RET) ||
                commands[5] != new GEFormats.Command(GEFormats.Operation.NOP) ||
                commands[6] != new GEFormats.Command(GEFormats.Operation.NOP) ||
                commands[7] != new GEFormats.Command(GEFormats.Operation.NOP))
            {
                throw new FormatException();
            }
        }
    }
}
