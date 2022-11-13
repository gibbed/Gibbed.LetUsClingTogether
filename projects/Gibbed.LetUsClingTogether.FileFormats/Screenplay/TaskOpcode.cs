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

namespace Gibbed.LetUsClingTogether.FileFormats.Screenplay
{
    public enum TaskOpcode : ushort
    {
        End = 0xFFFF,
        
        // Processing tasks
        NoOperation0 = 0, // 00
        SetGlobalFlag = 1, // 01
        SetLocalFlag = 2, // 02
        ModifyApproval = 3, // 03  chaos frame
        PlatoonNow = 4, // 04
        ModifyGoth = 5, // 05
        ModifyDays = 6, // 06
        ModifyLoyaltyForBattlePartyLawfulUnits = 7, // 07
        ModifyLoyaltyForBattlePartyNeutralUnits = 8, // 08
        ModifyLoyaltyForBattlePartyChaosUnits = 9, // 09
        ModifyLoyaltyForBattlePartyAndRosterLawfulUnits = 10, // 0A
        ModifyLoyaltyForBattlePartyAndRosterNeutralUnits = 11, // 0B
        ModifyLoyaltyForBattlePartyAndRosterChaosUnits = 12, // 0C
        SetHeroAlignment = 13, // 0D
        ResetGlobalFlagsInCategory = 14, // 0E  resets the value of all global flags in a given category
        SetPartyCharacterClan = 15, // 0F
        ModifyRosterUnitsInClanLoyalty = 16, // 10
        SetPartyCharacterLoyalty = 17, // 11
        SetStrongpointChild = 18, // 12
        SetHeroClan = 19, // 13
        GetItem = 20, // 14
        Unknown15 = 21,
        Unknown16 = 22,
        Unknown17 = 23,
        NoOperation24 = 24,
        Unknown19 = 25, // 19
        SetPartyAllegiance = 26, // 1A  Changes Hero's and any unit that matches Hero's previous allegiance to the new allegiance
        Unknown1B = 27, // 1B
        Unknown1C = 28, // 1C  something to do with battle text
        SetUnknownFlag = 29, // 1D
        ResetUnknownFlags = 30, // 1E
        SetPartyCharacterClass = 31, // 1F
        RemoveItem = 32, // 20
        Unknown21 = 33, // 21
        Unknown22 = 34, // 22  sets global flag 1131 if current value is less
        Unknown23 = 35, // 23  sets 0x800000 to flags2 on specified character
        Unknown24 = 36, // 24

        NoOperation37 = 37, // 25
        NoOperation38 = 38, // 26
        NoOperation39 = 39, // 27
        NoOperation40 = 40, // 28
        NoOperation41 = 41, // 29
        NoOperation42 = 42, // 2A
        NoOperation43 = 43, // 2B
        NoOperation44 = 44, // 2C
        NoOperation45 = 45, // 2D
        NoOperation46 = 46, // 2E
        NoOperation47 = 47, // 2F

        // Invocations
        CompareGlobalFlag = 48, // 30
        CompareLocalFlag = 49, // 31
        CompareStrongpoint = 50, // 32  check_strongpoint
        CompareBattleActiveCharacter = 51, // 33
        Unknown34 = 52, // 34  count of units alive?
        Unknown35 = 53,
        Unknown36 = 54,
        CompareBattleCharacterHealthLessThanOrEqual = 55, // 37
        CompareBattleCharacterHealthPercentLessThanOrEqual = 56, // 38
        CompareBattleCharacterMagicLessThanOrEqual = 57, // 39
        CompareBattleCharacterMagicPercentLessThanOrEqual = 58, // 3A
        CompareBattleCharacterHealthPercentInRange = 59, // 3B
        CompareBattleCharacterHealthGreaterThanOrEqual = 60, // 3C
        CompareBattleCharacterHealthPercentGreaterThanOrEqual = 61, // 3D
        CompareStrongpointChild = 62, // 3E
        CompareBattlePartyIsOneUnit = 63, // 3F
        CompareHeroIsNaked = 64, // 40
        Unknown41 = 65,
        CompareBattleCharacterHeartCountGreaterThanOrEqual = 66, // 42
        CompareBattleCharacterHeartCountLessThanOrEqual = 67, // 43
        Unknown44 = 68, // 44  something to do with screenplays\global\2.xlc
        CompareUnknownFlag = 69, // 45
        CompareWeather = 70, // 46
        Unknown47 = 71, // 47  something to do with screenplays\global\2.xlc
        CompareApproval = 72, // 48  chaos frame
        CompareBattleCharacterLoyaltyGreaterThanOrEqual = 73, // 49
        CompareBattleCharacterLoyaltyLessThanOrEqual = 74, // 4A
        Unknown4B = 75,
        Unknown4C = 76,
        CompareBattlePartyIsThreeUnits = 77, // 4D
        CompareHasItem = 78, // 4E
        Unknown4F = 79,
        Unknown50 = 80,
        Unknown51 = 81,
        Unknown52 = 82,
        CompareHasCharacter = 83, // 53

        NoOperation84 = 84, // 54
        NoOperation85 = 85, // 55
        NoOperation86 = 86, // 56
        NoOperation87 = 87, // 57
        NoOperation88 = 88, // 58
        NoOperation89 = 89, // 59
        NoOperation90 = 90, // 5A
        NoOperation91 = 91, // 5B
        NoOperation92 = 92, // 5C
        NoOperation93 = 93, // 5D
        NoOperation94 = 94, // 5E
        NoOperation95 = 95, // 5F

        // Expressions
        AndCount = 96, // 60  A && ...
        AndOrAnd = 97, // 61  A && (B || C) && D
        AndAndOrAnd = 98, // 62  A && B && (C || D) && E
        AndAndAndOr = 99, // 63  A && B && C && (D || E)
        AndOr = 100, // 64  A && (B || C)
        OrOrOrOrOrOrAnd = 101, // 65  A || B || C || D || E || F || (G && H)
        OrAndOrAndOrAndOr = 102, // 66  (A || B) && (C || D) && (E || F) && (G || H)
        Or = 103, // 67  A || B
        OrOr = 104, // 68  A || B || C
        AndAndAndOrOr = 105, // 69  A && B && C && D && (E || F || G)
        AndAndAndAndAndAndOr = 106, // 6A  A && B && C && D && E && F && (G || H)
        AndAndAndAndAndAndAndAndAndAndAndOr = 107, // 6B  A && B && C && D && E && F && G && H && I && J && K && (L || M)
        AndAndAndAndAndAndAndAndAndOr = 108, // 6C  A && B && C && D && E && F && G && H && I && (J || K)
        AndAndAndOrOrOr = 109, // 6D  A && B && C && (D || E || F || G)
        AndAndAndAndAndOr = 110, // 6E  A && B && C && D && E && (F || G)
        AndAndAndAndAndAndOrAnd = 111, // 6F  A && B && C && D && E && F && (G || H) && I
    }
}
