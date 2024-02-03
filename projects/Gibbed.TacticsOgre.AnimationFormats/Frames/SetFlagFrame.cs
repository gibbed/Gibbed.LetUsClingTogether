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

namespace Gibbed.TacticsOgre.AnimationFormats.Frames
{
    public struct SetFlagFrame : IFrame
    {
        FrameType IFrame.Type => FrameType.SetFlag;
        ushort IFrame.Time { get => this.Time; set => this.Time = value; }

        public ushort Time;
        public ushort FlagIndex;

        internal SetFlagFrame(FrameData data)
        {
            this.Time = default;
            this.FlagIndex = data.FlagIndex;

            if (data.Y1 != 0 ||
                data.Unknown2 != 0 ||
                data.X2 != 0 || data.Y2 != 0 ||
                data.Unknown5 != 0)
            {
                throw new ArgumentException("invalid data", nameof(data));
            }
        }
    }
}
