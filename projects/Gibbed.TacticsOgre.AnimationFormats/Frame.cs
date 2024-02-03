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
using System.IO;
using Gibbed.IO;

namespace Gibbed.TacticsOgre.AnimationFormats
{
    public sealed class Frame
    {
        public static IFrame ReadV0(Stream input, Endian endian)
        {
            var time = input.ReadValueU16(endian);
            var type = (FrameType)input.ReadValueU8();
            var unknown = input.ReadValueU8();
            if (unknown != 0)
            {
                throw new FormatException();
            }
            var data = FrameData.ReadV0(input, endian);
            IFrame instance = Create(type, data);
            instance.Time = time;
            return instance;
        }

        public static IFrame ReadV3(Stream input, Endian endian)
        {
            var time = input.ReadValueU16(endian);
            var type = (FrameType)input.ReadValueU8();
            var unknown = input.ReadValueU8();
            if (unknown != 0)
            {
                throw new FormatException();
            }
            var data = FrameData.ReadV3(input, endian);
            IFrame instance = Create(type, data);
            instance.Time = time;
            return instance;
        }

        private static IFrame Create(FrameType type, FrameData data) => type switch
        {
            FrameType.Sprite => new Frames.SpriteFrame(data),
            FrameType.Position => new Frames.PositionFrame(data),
            FrameType.Unknown2 => new Frames.Unknown2Frame(data),
            FrameType.Unknown3 => new Frames.Unknown3Frame(data),
            FrameType.LoopEnd => new Frames.LoopEndFrame(data),
            FrameType.LoopStart => new Frames.LoopStartFrame(data),
            FrameType.SetFlag => new Frames.SetFlagFrame(data),
            FrameType.Unknown10 => new Frames.Unknown10Frame(data),
            FrameType.GoTo => new Frames.GoToFrame(data),
            FrameType.Unknown12 => new Frames.Unknown12Frame(data),
            FrameType.Unknown14 => new Frames.Unknown14Frame(data),
            _ => throw new NotSupportedException(),
        };
    }
}
