﻿/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.TacticsOgre.AnimationFormats
{
    public enum FrameType : byte
    {
        Sprite = 0,
        Position = 1,
        Unknown2 = 2,
        Unknown3 = 3,
        Undefined4 = 4,
        Undefined5 = 5,
        Undefined6 = 6,
        LoopEnd = 7,
        LoopStart = 8,
        SetFlag = 9,
        Unknown10 = 10,
        GoTo = 11,
        Unknown12 = 12,
        Undefined13 = 13,
        Unknown14 = 14,
    }
}
