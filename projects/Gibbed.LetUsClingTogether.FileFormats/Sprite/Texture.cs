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
using GEFormats = Gibbed.PSP.GEFormats;

namespace Gibbed.LetUsClingTogether.FileFormats.Sprite
{
    public struct Texture
    {
        public bool IsReborn;
        public ushort TotalWidth;
        public ushort TotalHeight;
        public ushort BlockWidth;
        public ushort BlockHeight;
        public ushort BitsPerPixel;
        public byte[] Data;
        public GEFormats.Command[] GECommands;

        public static Texture Read(Stream input, Endian endian)
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

            if (dataOffset != 0x20 && dataOffset != 0x30)
            {
                throw new FormatException();
            }
            else if (dataOffset + dataSize != commandOffset)
            {
                throw new FormatException();
            }

            var isReborn = dataOffset == 0x30 ? true : false;

            var blockWidth = input.ReadValueU16(endian);
            var blockHeight = input.ReadValueU16(endian);
            var totalWidth = input.ReadValueU16(endian);
            var totalHeight = input.ReadValueU16(endian);
            var bpp = input.ReadValueU16(endian);
            var unknown1A = input.ReadValueU16(endian);
            var unknown1C = input.ReadValueU32(endian);

            byte[] unknown20;
            if (isReborn == true)
            {
                // TODO(gibbed): seems to be extended texture info
                // first byte matches texture format seen in .btx files
                unknown20 = input.ReadBytes(16);
            }
            else
            {
                unknown20 = null;
            }

            if (unknown1A != 0 || unknown1C != 0)
            {
                throw new InvalidOperationException();
            }

            if (dataOffset + dataSize + commandSize != totalSize)
            {
                throw new InvalidOperationException();
            }

            if (input.Position != basePosition + dataOffset)
            {
                throw new InvalidOperationException();
            }

            var dataBytes = input.ReadBytes((int)dataSize);

            var gpuCommands = new GEFormats.Command[8];
            for (int i = 0; i < gpuCommands.Length; i++)
            {
                gpuCommands[i] = new GEFormats.Command(input.ReadValueU32(endian));
            }

            Texture instance;
            instance.IsReborn = isReborn;
            instance.TotalWidth = totalWidth;
            instance.TotalHeight = totalHeight;
            instance.BlockWidth = blockWidth;
            instance.BlockHeight = blockHeight;
            instance.BitsPerPixel = bpp;
            instance.Data = dataBytes;
            instance.GECommands = gpuCommands;
            instance.ValidateGECommands();
            return instance;
        }

        private void ValidateGECommands()
        {
            if (ValidateGECommandsPSP() == false &&
                ValidateGECommandsReborn() == false)
            {
                throw new FormatException();
            }
        }

        private bool ValidateGECommandsPSP()
        {
            var tpsmArgument = this.BitsPerPixel switch
            {
                4 => 4,
                8 => 5,
                _ => 0,
            };
            if (tpsmArgument == 0)
            {
                return false;
            }

            int tsizeArgument = 0;
            tsizeArgument |= (byte)Math.Log(this.TotalWidth, 2);
            tsizeArgument |= ((byte)Math.Log(this.TotalHeight, 2)) << 8;

            var commands = this.GECommands;
            if (commands[0] != new GEFormats.Command(GEFormats.Operation.TMODE, 1) ||
                commands[1] != GEFormats.Operation.TPSM || commands[1].Argument != tpsmArgument ||
                commands[2] != GEFormats.Operation.TBW0 || commands[2].Argument != this.TotalWidth ||
                commands[3] != new GEFormats.Command(GEFormats.Operation.TBP0, 0) ||
                commands[4] != GEFormats.Operation.TSIZE0 || commands[4].Argument != tsizeArgument ||
                commands[5] != new GEFormats.Command(GEFormats.Operation.TFLUSH) ||
                commands[6] != new GEFormats.Command(GEFormats.Operation.RET) ||
                commands[7] != new GEFormats.Command(GEFormats.Operation.NOP))
            {
                return false;
            }
            return true;
        }

        private bool ValidateGECommandsReborn()
        {
            var commands = this.GECommands;
            if (commands[0] != new GEFormats.Command(GEFormats.Operation.TMODE) ||
                commands[1] != new GEFormats.Command(GEFormats.Operation.NOP) ||
                commands[2] != new GEFormats.Command(GEFormats.Operation.NOP) ||
                commands[3] != new GEFormats.Command(GEFormats.Operation.NOP) ||
                commands[4] != new GEFormats.Command(GEFormats.Operation.NOP) ||
                commands[5] != new GEFormats.Command(GEFormats.Operation.NOP) ||
                commands[6] != new GEFormats.Command(GEFormats.Operation.NOP) ||
                commands[7] != new GEFormats.Command(GEFormats.Operation.NOP))
            {
                return false;
            }
            return true;
        }
    }
}
