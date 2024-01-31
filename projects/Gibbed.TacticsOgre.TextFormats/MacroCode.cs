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

namespace Gibbed.TacticsOgre.TextFormats
{
    public enum MacroCode : byte
    {
        InsertNewline = 0x80,
        IndicateWrapArea = 0x81,
        SetTextColor = 0x82,
        Unknown83 = 0x83,
        Unknown84 = 0x84,
        Unknown85 = 0x85,
        InsertPageBreak = 0x86,
        Unknown87 = 0x87,
        InsertPixelSpacing = 0x88,
        Unknown89 = 0x89,
        InsertIcon = 0x8A,
        Unknown8B = 0x8B,
        InsertSelect = 0x8C,
        Unknown8D = 0x8D,
        Unknown8E = 0x8E,
        Unknown8F = 0x8F,
        Unknown90 = 0x90,
        Unknown91 = 0x91,
        Unknown92 = 0x92,
        InsertHeroName = 0x93,
        InsertOrderName = 0x94,
        ZombieCondition = 0x95,
        ZombieConditionEnd = 0x96,
        Unknown97 = 0x97,
        Unknown98 = 0x98,
        Unknown99 = 0x99,
        Unknown9A = 0x9A,
        ResetTextColor = 0x9B,
        Unknown9C = 0x9C,
        SetFontPalette = 0x9D,
        ResetFontPalette = 0x9E,
        UppercaseFollowingCharacter = 0x9F,
        UnknownA0 = 0xA0,
        UnknownA1 = 0xA1,
        UnknownA2 = 0xA2,
        UnknownA3 = 0xA3,
        UnknownA4 = 0xA4,
        UnknownA5 = 0xA5,
        UnknownA6 = 0xA6,
        UnknownA7 = 0xA7,
        UnknownA8 = 0xA8,
        MaybeNewPage = 0xA9,

        RebornUnknownAA = 0xAA,
        RebornUnknownAB = 0xAB,
        RebornUnknownAC = 0xAC,
        RebornUnknownAD = 0xAD,
        RebornUnknownAE = 0xAE,
        RebornUnknownAF = 0xAF,
        RebornUnknownB0 = 0xB0,
        RebornUnknownB1 = 0xB1,
        RebornUnknownB2 = 0xB2,
        RebornUnknownB3 = 0xB3,
        RebornUnknownB4 = 0xB4,
        RebornUnknownB5 = 0xB5,
    }
}
