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
        Invalid = 0, // 00
        SetGlobalFlag = 1, // 01
        SetLocalFlag = 2, // 02
        ModifyApproval = 3, // 03  chaos frame
        MoveToStrongpoint = 4, // 04
        ModifyGoth = 5, // 05
        ModifyDays = 6, // 06
        ModifyLoyaltyForBattlePartyLawfulUnits = 7, // 07
        ModifyLoyaltyForBattlePartyNeutralUnits = 8, // 08
        ModifyLoyaltyForBattlePartyChaosUnits = 9, // 09
        ModifyLoyaltyForBattlePartyAndRosterLawfulUnits = 10, // 0A
        ModifyLoyaltyForBattlePartyAndRosterNeutralUnits = 11, // 0B
        ModifyLoyaltyForBattlePartyAndRosterChaosUnits = 12, // 0C
        SetAlignmentForHero = 13, // 0D
        ResetGlobalFlagsInCategory = 14, // 0E  resets the value of all global flags in a given category
        SetClanForPartyCharacter = 15, // 0F
        ModifyLoyaltyForRosterUnitsInClan = 16, // 10
        SetLoyaltyForPartyCharacter = 17, // 11
        MoveToStrongpointChild = 18, // 12
        SetClanForHero = 19, // 13
        AddItemToParty = 20, // 14
        SetActionStrategyForBattleUnit = 21, // 15
        SuppressDeathSceneForBattleUnit = 22, // 16
        SetClearConditionMessageId = 23, // 17
        NoOperation24 = 24, // 18
        SetClearConditionUnitId = 25, // 19
        SetAllegianceForHeroAndParty = 26, // 1A  Changes Hero's and any unit that matches Hero's previous allegiance to the new allegiance
        SetFactionForCharacterInBattle = 27, // 1B
        SetExtraGameOverConditionTextId = 28, // 1C  something to do with battle text
        SetDungeonFlag = 29, // 1D
        ResetDungeonFlags = 30, // 1E
        SetPartyCharacterClass = 31, // 1F
        RemoveItemFromParty = 32, // 20
        GiveAppellation = 33, // 21
        IncreaseShopLevel = 34, // 22  sets global flag 1131 if current value is less
        SetTargetFlagForCharacterInBattle = 35, // 23  sets 0x800000 to flags2 on specified character
        RemoveConsumableFromBattleParty = 36, // 24

        [Reborn] SetUnionLevel = 37, // 25
        [Reborn] SetChariotTurnLimit = 38, // 26
        [Reborn] Unknown27 = 39, // 27
        [Reborn] Unknown28 = 40, // 28

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
        ComparePrimaryEnemyCount = 52, // 34  count of units alive?
        ComparePrimaryEnemyCountLessThanOrEqualExcept = 53, // 35
        ComparePrimaryEnemyCountGreaterThanOrEqualExcept = 54, // 36
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
        CompareBattleUnitAtPositionIsPlayerFaction = 65, // 41  compare to screenplays/global/2.xlc[x]
        CompareBattleCharacterHeartCountGreaterThanOrEqual = 66, // 42
        CompareBattleCharacterHeartCountLessThanOrEqual = 67, // 43
        CompareBattleUnitAtPositionCharacterEqual = 68, // 44  something to do with screenplays\global\2.xlc
        CompareDungeonFlag = 69, // 45
        CompareWeather = 70, // 46
        CompareBattleUnitAtPositionIsEntryUnit = 71, // 47  something to do with screenplays\global\2.xlc
        CompareApproval = 72, // 48  chaos frame
        CompareCharacterLoyaltyGreaterThanOrEqual = 73, // 49
        CompareCharacterLoyaltyLessThanOrEqual = 74, // 4A
        CompareBattleUnitReceivedAttack = 75,
        CompareBattleUnitReceivedSupport = 76,
        CompareBattlePartyIsThreeUnits = 77, // 4D
        CompareHasItem = 78, // 4E
        CompareCurrentShop = 79, // 4F
        CompareOverallDeceasedUnitsCount = 80, // 50
        CompareOverallIncapacitatedUnitsCount = 81, // 51
        CompareOverallChariotAndRetreatingNotUsed = 82, // 52
        CompareHasCharacter = 83, // 53

        [Reborn] Unknown54 = 84, // 54
        [Reborn] Unknown55 = 85, // 55
        [Reborn] Unknown56 = 86, // 56

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

        [Reborn] AndAndAndAndAndAndAndOr = 112, // 70  A && B && C && D && E && F && G && (H || I)
        [Reborn] AndAndAndAndOrOrOr = 113, // 71  A && B && C && D && (E || F || G || H)
    }
}
