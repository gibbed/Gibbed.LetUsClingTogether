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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;

namespace Gibbed.TacticsOgre.AnimationFormats
{
    public class SequenceFile
    {
        private const uint SignatureV0 = 0x00534153; // 'SAS\0' (PSP)
        private const uint SignatureV3 = 0x33534153; // 'SAS3'  (Reborn)

        private readonly List<Sprite> _Sprites = new();

        public byte Version { get; set; }
        public sbyte Scale { get; set; }
        public List<Sprite> Sprites => this._Sprites;
        public Direction[,] Directions { get; private set; }

        public void Serialize(Stream output, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input, Endian endian)
        {
            long basePosition = input.Position;

            var magic = input.ReadValueU32(endian);
            byte version = magic switch
            {
                SignatureV0 => 0,
                SignatureV3 => 3,
                _ => throw new NotSupportedException(),
            };

            sbyte scale;
            int headerSize;
            if (version < 3)
            {
                headerSize = 12;
                scale = 1;
            }
            else
            {
                headerSize = 16;
                scale = input.ReadValueS8();
                input.SkipZeroes(3);
            }

            var spriteTableOffset = input.ReadValueU32(endian);
            var unknownCount = input.ReadValueU32(endian);

            var animationCount = (spriteTableOffset - headerSize) >> 4;
            var animationOffsetTable = new uint[animationCount, 4];
            var animationOffsets = new int[(spriteTableOffset - headerSize) >> 2];
            for (int i = 0, o = headerSize; o < spriteTableOffset; i++, o += 16)
            {
                for (int j = 0; j < 4; j++)
                {
                    var offset = input.ReadValueU32(endian);
                    animationOffsetTable[i, j] = offset;
                    animationOffsets[(i * 4) + j] = (int)(offset >> 1);
                }
            }

            if (input.Position != basePosition + spriteTableOffset)
            {
                throw new FormatException();
            }

            input.Position = basePosition + spriteTableOffset;
            var spriteCount = input.ReadValueU32(endian);
            var spriteOffsets = new uint[spriteCount];
            for (uint i = 0; i < spriteCount; i++)
            {
                spriteOffsets[i] = input.ReadValueU32(endian);
            }
            var sprites = new Sprite[spriteCount];
            for (uint i = 0; i < spriteCount; i++)
            {
                input.Position = basePosition + spriteOffsets[i];
                sprites[i] = version switch
                {
                    0 => Sprite.ReadV0(input, endian),
                    3 => Sprite.ReadV3(input, endian),
                    _ => throw new NotSupportedException(),
                };
                if (i + 1 < spriteCount)
                {
                    var nextPosition = basePosition + spriteOffsets[i + 1];
                    if (input.Position != nextPosition)
                    {
                        throw new FormatException();
                    }
                }
            }

            animationOffsets = animationOffsets
                .OrderBy(v => v)
                .Where(v => v != 0)
                .Distinct()
                .ToArray();

            Dictionary<int, Animation> animations = new();
            foreach (var animationOffset in animationOffsets)
            {
                input.Position = basePosition + animationOffset;

                var frameCount = input.ReadValueU16(endian);
                var duration = input.ReadValueU16(endian);

                var frameSize = version < 3 ? 12 : 14;
                var frameBytes = input.ReadBytes(frameCount * frameSize);

                var frames = new IFrame[frameCount];
                for (ushort i = 0; i < frameCount; i++)
                {
                    using (MemoryStream data = new(frameBytes, i * frameSize, frameSize, false))
                    {
                        frames[i] = version switch
                        {
                            0 => Frame.ReadV0(data, endian),
                            3 => Frame.ReadV3(data, endian),
                            _ => throw new NotSupportedException(),
                        };
                        if (data.Position != data.Length)
                        {
                            throw new FormatException();
                        }
                    }
                }

                Animation animation = new();
                animation.TotalFrameCount = duration;
                animation.Frames.AddRange(frames);
                animations.Add(animationOffset, animation);
            }

            this.Version = version;
            this.Scale = scale;
            this.Directions = new Direction[animationCount, 4];
            for (int i = 0; i < animationCount; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var animationFlags = animationOffsetTable[i, j];
                    var animationOffset = (int)(animationFlags >> 1);
                    Direction direction;
                    direction.Animation = animationOffset != 0
                        ? animations[animationOffset]
                        : null;
                    direction.IsFlipped = (animationFlags & 1) != 0;
                    this.Directions[i, j] = direction;
                }
            }
            this.Sprites.AddRange(sprites);
        }
    }
}
