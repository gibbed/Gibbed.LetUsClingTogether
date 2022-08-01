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

namespace Gibbed.LetUsClingTogether.FileFormats.Text
{
    public static class FormatOpcodeHelpers
    {
        public static int GetSize(this FormatOpcode opcode) => opcode switch
        {
            FormatOpcode.InsertNewline => 1,
            FormatOpcode.IndicateWrapArea => 3,
            FormatOpcode.SetTextColor => 5,
            FormatOpcode.Unknown83 => 6,
            FormatOpcode.Unknown84 => 4,
            FormatOpcode.Unknown85 => 3,
            FormatOpcode.InsertPageBreak => 1,
            FormatOpcode.Unknown87 => 2,
            FormatOpcode.InsertPixelSpacing => 2,
            FormatOpcode.Unknown89 => 2,
            FormatOpcode.InsertIcon => 2,
            FormatOpcode.Unknown8B => 3,
            FormatOpcode.InsertSelect => 2,
            FormatOpcode.Unknown8D => 3,
            FormatOpcode.Unknown8E => 3,
            FormatOpcode.Unknown8F => 3,
            FormatOpcode.Unknown90 => 3,
            FormatOpcode.Unknown91 => 1,
            FormatOpcode.Unknown92 => 1,
            FormatOpcode.InsertHeroName => 1,
            FormatOpcode.InsertOrderName => 1,
            FormatOpcode.ZombieCondition => 4,
            FormatOpcode.ZombieConditionEnd => 1,
            FormatOpcode.Unknown97 => 1,
            FormatOpcode.Unknown98 => 3,
            FormatOpcode.Unknown99 => 3,
            FormatOpcode.Unknown9A => 2,
            FormatOpcode.ResetTextColor => 1,
            FormatOpcode.Unknown9C => 3,
            FormatOpcode.SetFontPalette => 2,
            FormatOpcode.ResetFontPalette => 1,
            FormatOpcode.UppercaseFollowingCharacter => 1,
            FormatOpcode.UnknownA0 => 1,
            FormatOpcode.UnknownA1 => 1,
            FormatOpcode.UnknownA2 => 1,
            FormatOpcode.UnknownA3 => 1,
            FormatOpcode.UnknownA4 => 1,
            FormatOpcode.UnknownA5 => 1,
            FormatOpcode.UnknownA6 => 1,
            FormatOpcode.UnknownA7 => 1,
            FormatOpcode.UnknownA8 => 1,
            FormatOpcode.MaybeNewPage => 1,
            _ => throw new NotSupportedException(),
        };
    }
}
