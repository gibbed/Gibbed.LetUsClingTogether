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

namespace Gibbed.TacticsOgre.FileFormats.Sprite
{
    public struct Sprite
    {
        public uint FrameWidth;
        public uint FrameHeight;
        public Texture? Texture;
        public Palette[] Palettes;

        public static Sprite Read(Stream input, Endian endian)
        {
            var basePosition = input.Position;

            var totalSize = input.ReadValueU32(endian);
            if (basePosition + totalSize > input.Length)
            {
                throw new FormatException();
            }

            var textureOffset = input.ReadValueU32(endian);
            var paletteBaseOffset = input.ReadValueU32(endian);
            var paletteOffsetTableOffset = input.ReadValueU32(endian);

            if (textureOffset != 0x20)
            {
                throw new FormatException();
            }

            var paletteCount = input.ReadValueU32(endian);
            var frameWidth = input.ReadValueU32(endian);
            var frameHeight = input.ReadValueU32(endian);

            // probably padding
            {
                var zero1C = input.ReadValueU32(endian);
                if (zero1C != 0)
                {
                    throw new FormatException();
                }
            }

            Texture? texture;
            if (textureOffset != paletteBaseOffset)
            {
                var textureSize = input.ReadValueS32(endian);
                input.Seek(-4, SeekOrigin.Current);
                if (textureSize < 4 || input.Position + textureSize != basePosition + paletteBaseOffset)
                {
                    throw new FormatException();
                }
                texture = FileFormats.Sprite.Texture.Read(input, endian);
            }
            else
            {
                texture = null;
            }

            var palettes = new Palette[paletteCount];
            if (paletteCount != 0)
            {
                if (paletteOffsetTableOffset == totalSize)
                {
                    throw new FormatException();
                }

                input.Position = basePosition + paletteOffsetTableOffset;
                var paletteOffsets = new uint[paletteCount];
                for (uint i = 0; i < paletteCount; i++)
                {
                    paletteOffsets[i] = input.ReadValueU32(endian);
                }

                for (uint i = 0; i < paletteCount; i++)
                {
                    var paletteOffset = paletteOffsets[i];
                    var endOffset = i + 1 < paletteCount
                        ? paletteBaseOffset + paletteOffsets[i + 1]
                        : totalSize;
                    input.Position = basePosition + paletteBaseOffset + paletteOffset;
                    palettes[i] = Palette.Read(input, endian);
                    if (input.Position > basePosition + endOffset)
                    {
                        throw new FormatException();
                    }
                }
            }
            else
            {
                if (paletteBaseOffset != totalSize ||
                    paletteOffsetTableOffset != totalSize)
                {
                    throw new FormatException();
                }

                // TODO(gibbed): remove this and make any tools using this
                // set up a default palette.
                Palette palette;
                palette.GECommands = null;
                using (MemoryStream data = new(texture?.IsReborn == false
                    ? FileFormats.Sprite.Palettes.DefaultPSP
                    : FileFormats.Sprite.Palettes.DefaultReborn))
                {
                    var colors = new uint[data.Length / 4];
                    for (int i = 0, o = 0; o < data.Length; i++, o += 4)
                    {
                        colors[i] = data.ReadValueU32(Endian.Little);
                    }
                    palette.Colors = colors;
                }
                palettes = new Palette[1];
                palettes[0] = palette;
            }

            Sprite instance;
            instance.FrameWidth = frameWidth;
            instance.FrameHeight = frameHeight;
            instance.Texture = texture;
            instance.Palettes = palettes;
            return instance;
        }
    }
}
