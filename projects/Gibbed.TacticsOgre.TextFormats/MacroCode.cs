/* Copyright (c) 2026 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.TacticsOgre.TextFormats;

public enum MacroCode : byte
{
    InsertNewline = 0x80,
    IndicateWrapArea = 0x81, // word that must fit on the line
    SetTextColor = 0x82,
    InsertStringVariable = 0x83,
    InsertNumberVariable = 0x84,
    SetTextSpeed = 0x85,
    InsertPageBreak = 0x86,
    StyleSpan = 0x87,
    InsertPixelSpacing = 0x88,
    Pause = 0x89,
    InsertIcon = 0x8A,
    Unknown8B = 0x8B,
    InsertSelect = 0x8C,
    Center = 0x8D,
    Unknown8E = 0x8E,
    Unknown8F = 0x8F,
    Unknown90 = 0x90,
    Unknown91 = 0x91,
    VoiceMark = 0x92,
    InsertHeroName = 0x93,
    InsertOrderName = 0x94,
    Conditional = 0x95,
    ConditionalEnd = 0x96,
    Unknown97 = 0x97,
    AutoAdvanceSeconds = 0x98,
    AutoAdvanceFrames = 0x99,
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
    EnableWordWrap = 0xA9,

    // Reborn
    Emphasis = 0xAA,
    IfSingular = 0xAB,
    Else = 0xAC,
    EndIf = 0xAD,
    IfMasculine = 0xAE,
    ElseIfFeminine = 0xAF,
    RebornUnknownB0 = 0xB0,
    RebornUnknownB1 = 0xB1,
    InsertSymbol = 0xB2,
    InsertButton = 0xB3,
    IfOrderNameStartsWithVowel = 0xB4,
    IfHeroNameStartsWithVowel = 0xB5,
}
