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

namespace Gibbed.TacticsOgre.TextFormats
{
    public static class MacroCodeHelpers
    {
        public static int GetSize(this MacroCode opcode, bool isReborn) => opcode switch
        {
            MacroCode.InsertNewline => 1,
            MacroCode.IndicateWrapArea => 3,
            MacroCode.SetTextColor => 5,
            MacroCode.Unknown83 => 6,
            MacroCode.Unknown84 => isReborn == true ? 5 : 4,
            MacroCode.Unknown85 => 3,
            MacroCode.InsertPageBreak => 1,
            MacroCode.Unknown87 => 2,
            MacroCode.InsertPixelSpacing => 2,
            MacroCode.Unknown89 => 2,
            MacroCode.InsertIcon => 2,
            MacroCode.Unknown8B => 3,
            MacroCode.InsertSelect => 2,
            MacroCode.Unknown8D => 3,
            MacroCode.Unknown8E => 3,
            MacroCode.Unknown8F => 3,
            MacroCode.Unknown90 => 3,
            MacroCode.Unknown91 => 1,
            MacroCode.Unknown92 => 1,
            MacroCode.InsertHeroName => 1,
            MacroCode.InsertOrderName => 1,
            MacroCode.ZombieCondition => 4,
            MacroCode.ZombieConditionEnd => 1,
            MacroCode.Unknown97 => 1,
            MacroCode.Unknown98 => 3,
            MacroCode.Unknown99 => 3,
            MacroCode.Unknown9A => 2,
            MacroCode.ResetTextColor => 1,
            MacroCode.Unknown9C => 3,
            MacroCode.SetFontPalette => 2,
            MacroCode.ResetFontPalette => 1,
            MacroCode.UppercaseFollowingCharacter => 1,
            MacroCode.UnknownA0 => 1,
            MacroCode.UnknownA1 => 1,
            MacroCode.UnknownA2 => 1,
            MacroCode.UnknownA3 => 1,
            MacroCode.UnknownA4 => 1,
            MacroCode.UnknownA5 => 1,
            MacroCode.UnknownA6 => 1,
            MacroCode.UnknownA7 => 1,
            MacroCode.UnknownA8 => 1,
            MacroCode.MaybeNewPage => 1,
            MacroCode.RebornUnknownAA => 2, // Reborn
            MacroCode.RebornUnknownAB => 3, // Reborn
            MacroCode.RebornUnknownAC => 3, // Reborn
            MacroCode.RebornUnknownAD => 1, // Reborn
            MacroCode.RebornUnknownAE => 3, // Reborn
            MacroCode.RebornUnknownAF => 3, // Reborn
            MacroCode.RebornUnknownB0 => 1, // Reborn
            MacroCode.RebornUnknownB1 => 1, // Reborn
            MacroCode.RebornUnknownB2 => 4, // Reborn
            MacroCode.RebornUnknownB3 => 4, // Reborn
            _ => throw new NotSupportedException(),
        };
    }
}
