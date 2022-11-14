﻿/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.LetUsClingTogether.FileFormats.Screenplay
{
    public static class TaskOpcodeHelpers
    {
        public static bool IsValid(this TaskOpcode opcode) => opcode switch
        {
            TaskOpcode.Invalid => false,
            TaskOpcode.SetGlobalFlag => true,
            TaskOpcode.SetLocalFlag => true,
            TaskOpcode.ModifyApproval => true,
            TaskOpcode.MoveToStrongpoint => true,
            TaskOpcode.ModifyGoth => true,
            TaskOpcode.ModifyDays => true,
            TaskOpcode.ModifyLoyaltyForBattlePartyLawfulUnits => true,
            TaskOpcode.ModifyLoyaltyForBattlePartyNeutralUnits => true,
            TaskOpcode.ModifyLoyaltyForBattlePartyChaosUnits => true,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterLawfulUnits => true,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterNeutralUnits => true,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterChaosUnits => true,
            TaskOpcode.SetAlignmentForHero => true,
            TaskOpcode.ResetGlobalFlagsInCategory => true,
            TaskOpcode.SetClanForPartyCharacter => true,
            TaskOpcode.ModifyLoyaltyForRosterUnitsInClan => true,
            TaskOpcode.SetLoyaltyForPartyCharacter => true,
            TaskOpcode.MoveToStrongpointChild => true,
            TaskOpcode.SetClanForHero => true,
            TaskOpcode.AddItemToParty => true,
            TaskOpcode.SetActionStrategyForBattleUnit => true,
            TaskOpcode.SuppressDeathSceneForBattleUnit => true,
            TaskOpcode.SetClearConditionMessageId => true,
            TaskOpcode.NoOperation24 => true,
            TaskOpcode.SetClearConditionUnitId => true,
            TaskOpcode.SetAllegianceForHeroAndParty => true,
            TaskOpcode.SetFactionForCharacterInBattle => true,
            TaskOpcode.SetExtraGameOverConditionTextId => true,
            TaskOpcode.SetDungeonFlag => true,
            TaskOpcode.ResetDungeonFlags => true,
            TaskOpcode.SetPartyCharacterClass => true,
            TaskOpcode.RemoveItemFromParty => true,
            TaskOpcode.GiveAppellation => true,
            TaskOpcode.IncreaseShopLevel => true,
            TaskOpcode.SetTargetFlagForCharacterInBattle => true,
            TaskOpcode.RemoveConsumableFromBattleParty => true,
            TaskOpcode.SetUnionLevel => true, // Reborn
            TaskOpcode.SetChariotTurnLimit => true, // Reborn
            TaskOpcode.Unknown27 => true, // Reborn
            TaskOpcode.Unknown28 => true, // Reborn
            TaskOpcode.NoOperation41 => false,
            TaskOpcode.NoOperation42 => false,
            TaskOpcode.NoOperation43 => false,
            TaskOpcode.NoOperation44 => false,
            TaskOpcode.NoOperation45 => false,
            TaskOpcode.NoOperation46 => false,
            TaskOpcode.NoOperation47 => false,
            TaskOpcode.CompareGlobalFlag => true,
            TaskOpcode.CompareLocalFlag => true,
            TaskOpcode.CompareStrongpoint => true,
            TaskOpcode.CompareBattleActiveCharacter => true,
            TaskOpcode.ComparePrimaryEnemyCount => true,
            TaskOpcode.ComparePrimaryEnemyCountLessThanOrEqualExcept => true,
            TaskOpcode.ComparePrimaryEnemyCountGreaterThanOrEqualExcept => true,
            TaskOpcode.CompareBattleCharacterHealthLessThanOrEqual => true,
            TaskOpcode.CompareBattleCharacterHealthPercentLessThanOrEqual => true,
            TaskOpcode.CompareBattleCharacterMagicLessThanOrEqual => true,
            TaskOpcode.CompareBattleCharacterMagicPercentLessThanOrEqual => true,
            TaskOpcode.CompareBattleCharacterHealthPercentInRange => true,
            TaskOpcode.CompareBattleCharacterHealthGreaterThanOrEqual => true,
            TaskOpcode.CompareBattleCharacterHealthPercentGreaterThanOrEqual => true,
            TaskOpcode.CompareStrongpointChild => true,
            TaskOpcode.CompareBattlePartyIsOneUnit => true,
            TaskOpcode.CompareHeroIsNaked => true,
            TaskOpcode.CompareBattleUnitAtPositionIsPlayerFaction => true,
            TaskOpcode.CompareBattleCharacterHeartCountGreaterThanOrEqual => true,
            TaskOpcode.CompareBattleCharacterHeartCountLessThanOrEqual => true,
            TaskOpcode.CompareBattleUnitAtPositionCharacterEqual => true,
            TaskOpcode.CompareDungeonFlag => true,
            TaskOpcode.CompareWeather => true,
            TaskOpcode.CompareBattleUnitAtPositionIsEntryUnit => true,
            TaskOpcode.CompareApproval => true,
            TaskOpcode.CompareCharacterLoyaltyGreaterThanOrEqual => true,
            TaskOpcode.CompareCharacterLoyaltyLessThanOrEqual => true,
            TaskOpcode.CompareBattleUnitReceivedAttack => true,
            TaskOpcode.CompareBattleUnitReceivedSupport => true,
            TaskOpcode.CompareBattlePartyIsThreeUnits => true,
            TaskOpcode.CompareHasItem => true,
            TaskOpcode.CompareCurrentShop => true,
            TaskOpcode.CompareOverallDeceasedUnitsCount => true,
            TaskOpcode.CompareOverallIncapacitatedUnitsCount => true,
            TaskOpcode.CompareOverallChariotAndRetreatingNotUsed => true,
            TaskOpcode.CompareHasCharacter => true,
            TaskOpcode.Unknown54 => true, // Reborn
            TaskOpcode.Unknown55 => true, // Reborn
            TaskOpcode.Unknown56 => true, // Reborn
            TaskOpcode.NoOperation87 => false,
            TaskOpcode.NoOperation88 => false,
            TaskOpcode.NoOperation89 => false,
            TaskOpcode.NoOperation90 => false,
            TaskOpcode.NoOperation91 => false,
            TaskOpcode.NoOperation92 => false,
            TaskOpcode.NoOperation93 => false,
            TaskOpcode.NoOperation94 => false,
            TaskOpcode.NoOperation95 => false,
            TaskOpcode.AndCount => true,
            TaskOpcode.AndOrAnd => true,
            TaskOpcode.AndAndOrAnd => true,
            TaskOpcode.AndAndAndOr => true,
            TaskOpcode.AndOr => true,
            TaskOpcode.OrOrOrOrOrOrAnd => true,
            TaskOpcode.OrAndOrAndOrAndOr => true,
            TaskOpcode.Or => true,
            TaskOpcode.OrOr => true,
            TaskOpcode.AndAndAndOrOr => true,
            TaskOpcode.AndAndAndAndAndAndOr => true,
            TaskOpcode.AndAndAndAndAndAndAndAndAndAndAndOr => true,
            TaskOpcode.AndAndAndAndAndAndAndAndAndOr => true,
            TaskOpcode.AndAndAndOrOrOr => true,
            TaskOpcode.AndAndAndAndAndOr => true,
            TaskOpcode.AndAndAndAndAndAndOrAnd => true,
            TaskOpcode.AndAndAndAndAndAndAndOr => true, // Reborn
            TaskOpcode.AndAndAndAndOrOrOr => true, // Reborn
            _ => throw new ArgumentOutOfRangeException(nameof(opcode)),
        };

        public static TaskType GetTaskType(this TaskOpcode opcode) => opcode switch
        {
            TaskOpcode.Invalid => TaskType.ProcessingTask,
            TaskOpcode.SetGlobalFlag => TaskType.ProcessingTask,
            TaskOpcode.SetLocalFlag => TaskType.ProcessingTask,
            TaskOpcode.ModifyApproval => TaskType.ProcessingTask,
            TaskOpcode.MoveToStrongpoint => TaskType.ProcessingTask,
            TaskOpcode.ModifyGoth => TaskType.ProcessingTask,
            TaskOpcode.ModifyDays => TaskType.ProcessingTask,
            TaskOpcode.ModifyLoyaltyForBattlePartyLawfulUnits => TaskType.ProcessingTask,
            TaskOpcode.ModifyLoyaltyForBattlePartyNeutralUnits => TaskType.ProcessingTask,
            TaskOpcode.ModifyLoyaltyForBattlePartyChaosUnits => TaskType.ProcessingTask,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterLawfulUnits => TaskType.ProcessingTask,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterNeutralUnits => TaskType.ProcessingTask,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterChaosUnits => TaskType.ProcessingTask,
            TaskOpcode.SetAlignmentForHero => TaskType.ProcessingTask,
            TaskOpcode.ResetGlobalFlagsInCategory => TaskType.ProcessingTask,
            TaskOpcode.SetClanForPartyCharacter => TaskType.ProcessingTask,
            TaskOpcode.ModifyLoyaltyForRosterUnitsInClan => TaskType.ProcessingTask,
            TaskOpcode.SetLoyaltyForPartyCharacter => TaskType.ProcessingTask,
            TaskOpcode.MoveToStrongpointChild => TaskType.ProcessingTask,
            TaskOpcode.SetClanForHero => TaskType.ProcessingTask,
            TaskOpcode.AddItemToParty => TaskType.ProcessingTask,
            TaskOpcode.SetActionStrategyForBattleUnit => TaskType.ProcessingTask,
            TaskOpcode.SuppressDeathSceneForBattleUnit => TaskType.ProcessingTask,
            TaskOpcode.SetClearConditionMessageId => TaskType.ProcessingTask,
            TaskOpcode.NoOperation24 => TaskType.ProcessingTask,
            TaskOpcode.SetClearConditionUnitId => TaskType.ProcessingTask,
            TaskOpcode.SetAllegianceForHeroAndParty => TaskType.ProcessingTask,
            TaskOpcode.SetFactionForCharacterInBattle => TaskType.ProcessingTask,
            TaskOpcode.SetExtraGameOverConditionTextId => TaskType.ProcessingTask,
            TaskOpcode.SetDungeonFlag => TaskType.ProcessingTask,
            TaskOpcode.ResetDungeonFlags => TaskType.ProcessingTask,
            TaskOpcode.SetPartyCharacterClass => TaskType.ProcessingTask,
            TaskOpcode.RemoveItemFromParty => TaskType.ProcessingTask,
            TaskOpcode.GiveAppellation => TaskType.ProcessingTask,
            TaskOpcode.IncreaseShopLevel => TaskType.ProcessingTask,
            TaskOpcode.SetTargetFlagForCharacterInBattle => TaskType.ProcessingTask,
            TaskOpcode.RemoveConsumableFromBattleParty => TaskType.ProcessingTask,
            TaskOpcode.SetUnionLevel => TaskType.ProcessingTask,
            TaskOpcode.SetChariotTurnLimit => TaskType.ProcessingTask,
            TaskOpcode.Unknown27 => TaskType.ProcessingTask,
            TaskOpcode.Unknown28 => TaskType.ProcessingTask,
            TaskOpcode.NoOperation41 => TaskType.ProcessingTask,
            TaskOpcode.NoOperation42 => TaskType.ProcessingTask,
            TaskOpcode.NoOperation43 => TaskType.ProcessingTask,
            TaskOpcode.NoOperation44 => TaskType.ProcessingTask,
            TaskOpcode.NoOperation45 => TaskType.ProcessingTask,
            TaskOpcode.NoOperation46 => TaskType.ProcessingTask,
            TaskOpcode.NoOperation47 => TaskType.ProcessingTask,
            TaskOpcode.CompareGlobalFlag => TaskType.Invocation,
            TaskOpcode.CompareLocalFlag => TaskType.Invocation,
            TaskOpcode.CompareStrongpoint => TaskType.Invocation,
            TaskOpcode.CompareBattleActiveCharacter => TaskType.Invocation,
            TaskOpcode.ComparePrimaryEnemyCount => TaskType.Invocation,
            TaskOpcode.ComparePrimaryEnemyCountLessThanOrEqualExcept => TaskType.Invocation,
            TaskOpcode.ComparePrimaryEnemyCountGreaterThanOrEqualExcept => TaskType.Invocation,
            TaskOpcode.CompareBattleCharacterHealthLessThanOrEqual => TaskType.Invocation,
            TaskOpcode.CompareBattleCharacterHealthPercentLessThanOrEqual => TaskType.Invocation,
            TaskOpcode.CompareBattleCharacterMagicLessThanOrEqual => TaskType.Invocation,
            TaskOpcode.CompareBattleCharacterMagicPercentLessThanOrEqual => TaskType.Invocation,
            TaskOpcode.CompareBattleCharacterHealthPercentInRange => TaskType.Invocation,
            TaskOpcode.CompareBattleCharacterHealthGreaterThanOrEqual => TaskType.Invocation,
            TaskOpcode.CompareBattleCharacterHealthPercentGreaterThanOrEqual => TaskType.Invocation,
            TaskOpcode.CompareStrongpointChild => TaskType.Invocation,
            TaskOpcode.CompareBattlePartyIsOneUnit => TaskType.Invocation,
            TaskOpcode.CompareHeroIsNaked => TaskType.Invocation,
            TaskOpcode.CompareBattleUnitAtPositionIsPlayerFaction => TaskType.Invocation,
            TaskOpcode.CompareBattleCharacterHeartCountGreaterThanOrEqual => TaskType.Invocation,
            TaskOpcode.CompareBattleCharacterHeartCountLessThanOrEqual => TaskType.Invocation,
            TaskOpcode.CompareBattleUnitAtPositionCharacterEqual => TaskType.Invocation,
            TaskOpcode.CompareDungeonFlag => TaskType.Invocation,
            TaskOpcode.CompareWeather => TaskType.Invocation,
            TaskOpcode.CompareBattleUnitAtPositionIsEntryUnit => TaskType.Invocation,
            TaskOpcode.CompareApproval => TaskType.Invocation,
            TaskOpcode.CompareCharacterLoyaltyGreaterThanOrEqual => TaskType.Invocation,
            TaskOpcode.CompareCharacterLoyaltyLessThanOrEqual => TaskType.Invocation,
            TaskOpcode.CompareBattleUnitReceivedAttack => TaskType.Invocation,
            TaskOpcode.CompareBattleUnitReceivedSupport => TaskType.Invocation,
            TaskOpcode.CompareBattlePartyIsThreeUnits => TaskType.Invocation,
            TaskOpcode.CompareHasItem => TaskType.Invocation,
            TaskOpcode.CompareCurrentShop => TaskType.Invocation,
            TaskOpcode.CompareOverallDeceasedUnitsCount => TaskType.Invocation,
            TaskOpcode.CompareOverallIncapacitatedUnitsCount => TaskType.Invocation,
            TaskOpcode.CompareOverallChariotAndRetreatingNotUsed => TaskType.Invocation,
            TaskOpcode.CompareHasCharacter => TaskType.Invocation,
            TaskOpcode.Unknown54 => TaskType.Invocation,
            TaskOpcode.Unknown55 => TaskType.Invocation,
            TaskOpcode.Unknown56 => TaskType.Invocation,
            TaskOpcode.NoOperation87 => TaskType.Invocation,
            TaskOpcode.NoOperation88 => TaskType.Invocation,
            TaskOpcode.NoOperation89 => TaskType.Invocation,
            TaskOpcode.NoOperation90 => TaskType.Invocation,
            TaskOpcode.NoOperation91 => TaskType.Invocation,
            TaskOpcode.NoOperation92 => TaskType.Invocation,
            TaskOpcode.NoOperation93 => TaskType.Invocation,
            TaskOpcode.NoOperation94 => TaskType.Invocation,
            TaskOpcode.NoOperation95 => TaskType.Invocation,
            TaskOpcode.AndCount => TaskType.Expression,
            TaskOpcode.AndOrAnd => TaskType.Expression,
            TaskOpcode.AndAndOrAnd => TaskType.Expression,
            TaskOpcode.AndAndAndOr => TaskType.Expression,
            TaskOpcode.AndOr => TaskType.Expression,
            TaskOpcode.OrOrOrOrOrOrAnd => TaskType.Expression,
            TaskOpcode.OrAndOrAndOrAndOr => TaskType.Expression,
            TaskOpcode.Or => TaskType.Expression,
            TaskOpcode.OrOr => TaskType.Expression,
            TaskOpcode.AndAndAndOrOr => TaskType.Expression,
            TaskOpcode.AndAndAndAndAndAndOr => TaskType.Expression,
            TaskOpcode.AndAndAndAndAndAndAndAndAndAndAndOr => TaskType.Expression,
            TaskOpcode.AndAndAndAndAndAndAndAndAndOr => TaskType.Expression,
            TaskOpcode.AndAndAndOrOrOr => TaskType.Expression,
            TaskOpcode.AndAndAndAndAndOr => TaskType.Expression,
            TaskOpcode.AndAndAndAndAndAndOrAnd => TaskType.Expression,
            TaskOpcode.AndAndAndAndAndAndAndOr => TaskType.Expression, // Reborn
            TaskOpcode.AndAndAndAndOrOrOr => TaskType.Expression, // Reborn
            _ => throw new ArgumentOutOfRangeException(nameof(opcode)),
        };

        public static int GetSize(this TaskOpcode opcode) => opcode switch
        {
            TaskOpcode.Invalid => 0,
            TaskOpcode.SetGlobalFlag => 3,
            TaskOpcode.SetLocalFlag => 3,
            TaskOpcode.ModifyApproval => 2,
            TaskOpcode.MoveToStrongpoint => 1,
            TaskOpcode.ModifyGoth => 4,
            TaskOpcode.ModifyDays => 1,
            TaskOpcode.ModifyLoyaltyForBattlePartyLawfulUnits => 1,
            TaskOpcode.ModifyLoyaltyForBattlePartyNeutralUnits => 1,
            TaskOpcode.ModifyLoyaltyForBattlePartyChaosUnits => 1,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterLawfulUnits => 1,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterNeutralUnits => 1,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterChaosUnits => 1,
            TaskOpcode.SetAlignmentForHero => 1,
            TaskOpcode.ResetGlobalFlagsInCategory => 1,
            TaskOpcode.SetClanForPartyCharacter => 3,
            TaskOpcode.ModifyLoyaltyForRosterUnitsInClan => 2,
            TaskOpcode.SetLoyaltyForPartyCharacter => 3,
            TaskOpcode.MoveToStrongpointChild => 2,
            TaskOpcode.SetClanForHero => 1,
            TaskOpcode.AddItemToParty => 2,
            TaskOpcode.SetActionStrategyForBattleUnit => 4,
            TaskOpcode.SuppressDeathSceneForBattleUnit => 2,
            TaskOpcode.SetClearConditionMessageId => 1,
            TaskOpcode.NoOperation24 => 2,
            TaskOpcode.SetClearConditionUnitId => 1,
            TaskOpcode.SetAllegianceForHeroAndParty => 1,
            TaskOpcode.SetFactionForCharacterInBattle => 3,
            TaskOpcode.SetExtraGameOverConditionTextId => 1,
            TaskOpcode.SetDungeonFlag => 3,
            TaskOpcode.ResetDungeonFlags => 0,
            TaskOpcode.SetPartyCharacterClass => 3,
            TaskOpcode.RemoveItemFromParty => 2,
            TaskOpcode.GiveAppellation => 1,
            TaskOpcode.IncreaseShopLevel => 1,
            TaskOpcode.SetTargetFlagForCharacterInBattle => 2,
            TaskOpcode.RemoveConsumableFromBattleParty => 2,
            TaskOpcode.SetUnionLevel => 1, // Reborn
            TaskOpcode.SetChariotTurnLimit => 1, // Reborn
            TaskOpcode.Unknown27 => 3, // Reborn
            TaskOpcode.Unknown28 => 3, // Reborn
            TaskOpcode.NoOperation41 => 0,
            TaskOpcode.NoOperation42 => 0,
            TaskOpcode.NoOperation43 => 0,
            TaskOpcode.NoOperation44 => 0,
            TaskOpcode.NoOperation45 => 0,
            TaskOpcode.NoOperation46 => 0,
            TaskOpcode.NoOperation47 => 0,
            TaskOpcode.CompareGlobalFlag => 4,
            TaskOpcode.CompareLocalFlag => 4,
            TaskOpcode.CompareStrongpoint => 2,
            TaskOpcode.CompareBattleActiveCharacter => 3,
            TaskOpcode.ComparePrimaryEnemyCount => 2,
            TaskOpcode.ComparePrimaryEnemyCountLessThanOrEqualExcept => 4,
            TaskOpcode.ComparePrimaryEnemyCountGreaterThanOrEqualExcept => 4,
            TaskOpcode.CompareBattleCharacterHealthLessThanOrEqual => 4,
            TaskOpcode.CompareBattleCharacterHealthPercentLessThanOrEqual => 4,
            TaskOpcode.CompareBattleCharacterMagicLessThanOrEqual => 4,
            TaskOpcode.CompareBattleCharacterMagicPercentLessThanOrEqual => 4,
            TaskOpcode.CompareBattleCharacterHealthPercentInRange => 5,
            TaskOpcode.CompareBattleCharacterHealthGreaterThanOrEqual => 4,
            TaskOpcode.CompareBattleCharacterHealthPercentGreaterThanOrEqual => 4,
            TaskOpcode.CompareStrongpointChild => 3,
            TaskOpcode.CompareBattlePartyIsOneUnit => 2,
            TaskOpcode.CompareHeroIsNaked => 2,
            TaskOpcode.CompareBattleUnitAtPositionIsPlayerFaction => 2,
            TaskOpcode.CompareBattleCharacterHeartCountGreaterThanOrEqual => 4,
            TaskOpcode.CompareBattleCharacterHeartCountLessThanOrEqual => 4,
            TaskOpcode.CompareBattleUnitAtPositionCharacterEqual => 4,
            TaskOpcode.CompareDungeonFlag => 4,
            TaskOpcode.CompareWeather => 2,
            TaskOpcode.CompareBattleUnitAtPositionIsEntryUnit => 3,
            TaskOpcode.CompareApproval => 3,
            TaskOpcode.CompareCharacterLoyaltyGreaterThanOrEqual => 4,
            TaskOpcode.CompareCharacterLoyaltyLessThanOrEqual => 4,
            TaskOpcode.CompareBattleUnitReceivedAttack => 3,
            TaskOpcode.CompareBattleUnitReceivedSupport => 3,
            TaskOpcode.CompareBattlePartyIsThreeUnits => 2,
            TaskOpcode.CompareHasItem => 3,
            TaskOpcode.CompareCurrentShop => 2,
            TaskOpcode.CompareOverallDeceasedUnitsCount => 3,
            TaskOpcode.CompareOverallIncapacitatedUnitsCount => 3,
            TaskOpcode.CompareOverallChariotAndRetreatingNotUsed => 2,
            TaskOpcode.CompareHasCharacter => 3,
            TaskOpcode.Unknown54 => 2, // Reborn
            TaskOpcode.Unknown55 => 4, // Reborn
            TaskOpcode.Unknown56 => 2, // Reborn
            TaskOpcode.NoOperation87 => 0,
            TaskOpcode.NoOperation88 => 0,
            TaskOpcode.NoOperation89 => 0,
            TaskOpcode.NoOperation90 => 0,
            TaskOpcode.NoOperation91 => 0,
            TaskOpcode.NoOperation92 => 0,
            TaskOpcode.NoOperation93 => 0,
            TaskOpcode.NoOperation94 => 0,
            TaskOpcode.NoOperation95 => 0,
            TaskOpcode.AndCount => 1,
            TaskOpcode.AndOrAnd => 1,
            TaskOpcode.AndAndOrAnd => 1,
            TaskOpcode.AndAndAndOr => 1,
            TaskOpcode.AndOr => 1,
            TaskOpcode.OrOrOrOrOrOrAnd => 1,
            TaskOpcode.OrAndOrAndOrAndOr => 1,
            TaskOpcode.Or => 1,
            TaskOpcode.OrOr => 1,
            TaskOpcode.AndAndAndOrOr => 1,
            TaskOpcode.AndAndAndAndAndAndOr => 1,
            TaskOpcode.AndAndAndAndAndAndAndAndAndAndAndOr => 1,
            TaskOpcode.AndAndAndAndAndAndAndAndAndOr => 1,
            TaskOpcode.AndAndAndOrOrOr => 1,
            TaskOpcode.AndAndAndAndAndOr => 1,
            TaskOpcode.AndAndAndAndAndAndOrAnd => 1,
            TaskOpcode.AndAndAndAndAndAndAndOr => 1, // Reborn
            TaskOpcode.AndAndAndAndOrOrOr => 1, // Reborn
            _ => throw new ArgumentOutOfRangeException(nameof(opcode)),
        };

        public static (TargetType targetType, ValueType valueType) GetArguments(this TaskOpcode opcode) => opcode switch
        {
            TaskOpcode.SetGlobalFlag => (TargetType.GlobalFlag, ValueType.Int8),
            TaskOpcode.SetLocalFlag => (TargetType.LocalFlag, ValueType.Bool),
            TaskOpcode.ModifyApproval => (TargetType.ApprovalRate, ValueType.Int8),
            TaskOpcode.MoveToStrongpoint => (TargetType.None, ValueType.Int8),
            TaskOpcode.ModifyGoth => (TargetType.None, ValueType.Int32),
            TaskOpcode.ModifyDays => (TargetType.None, ValueType.Int8),
            TaskOpcode.ModifyLoyaltyForBattlePartyLawfulUnits => (TargetType.None, ValueType.Int8),
            TaskOpcode.ModifyLoyaltyForBattlePartyNeutralUnits => (TargetType.None, ValueType.Int8),
            TaskOpcode.ModifyLoyaltyForBattlePartyChaosUnits => (TargetType.None, ValueType.Int8),
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterLawfulUnits => (TargetType.None, ValueType.Int8),
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterNeutralUnits => (TargetType.None, ValueType.Int8),
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterChaosUnits => (TargetType.None, ValueType.Int8),
            TaskOpcode.SetAlignmentForHero => (TargetType.None, ValueType.Int8),
            TaskOpcode.ResetGlobalFlagsInCategory => (TargetType.None, ValueType.Int8),
            TaskOpcode.SetClanForPartyCharacter => (TargetType.UnknownUInt16, ValueType.Int8),
            TaskOpcode.ModifyLoyaltyForRosterUnitsInClan => (TargetType.UnknownUInt8, ValueType.Int8),
            TaskOpcode.SetLoyaltyForPartyCharacter => (TargetType.UnknownUInt16, ValueType.Int8),
            TaskOpcode.MoveToStrongpointChild => (TargetType.None, ValueType.UInt16),
            TaskOpcode.SetClanForHero => (TargetType.UnknownUInt8, ValueType.None),
            TaskOpcode.AddItemToParty => (TargetType.None, ValueType.UInt16),
            TaskOpcode.SetActionStrategyForBattleUnit => (TargetType.UnknownUInt16, ValueType.Int8),
            TaskOpcode.SuppressDeathSceneForBattleUnit => (TargetType.None, ValueType.UInt16),
            TaskOpcode.SetClearConditionMessageId => (TargetType.None, ValueType.Int8),

            TaskOpcode.SetClearConditionUnitId => (TargetType.None, ValueType.Int8),
            TaskOpcode.SetAllegianceForHeroAndParty => (TargetType.None, ValueType.UInt8),

            TaskOpcode.SetExtraGameOverConditionTextId => (TargetType.None, ValueType.UInt8),

            TaskOpcode.ResetDungeonFlags => (TargetType.None, ValueType.None),

            TaskOpcode.RemoveItemFromParty => (TargetType.None, ValueType.UInt16),
            TaskOpcode.GiveAppellation => (TargetType.None, ValueType.UInt8),
            TaskOpcode.IncreaseShopLevel => (TargetType.None, ValueType.Int8),
            TaskOpcode.SetTargetFlagForCharacterInBattle => (TargetType.UnknownUInt16, ValueType.None),
            TaskOpcode.RemoveConsumableFromBattleParty => (TargetType.UnknownUInt16, ValueType.None),
            TaskOpcode.SetUnionLevel => (TargetType.None, ValueType.UInt8), // Reborn
            TaskOpcode.SetChariotTurnLimit => (TargetType.None, ValueType.UInt8), // Reborn
            TaskOpcode.Unknown27 => (TargetType.UnknownUInt16, ValueType.UInt8), // Reborn
            TaskOpcode.Unknown28 => (TargetType.UnknownUInt16, ValueType.UInt8), // Reborn

            TaskOpcode.CompareGlobalFlag => (TargetType.GlobalFlag, ValueType.UInt8),
            TaskOpcode.CompareLocalFlag => (TargetType.LocalFlag, ValueType.UInt8),
            TaskOpcode.CompareStrongpoint => (TargetType.None, ValueType.Int8),

            TaskOpcode.CompareBattleActiveCharacter => (TargetType.None, ValueType.UInt16),

            TaskOpcode.ComparePrimaryEnemyCount => (TargetType.None, ValueType.Int8),
            TaskOpcode.ComparePrimaryEnemyCountLessThanOrEqualExcept => (TargetType.UnknownUInt16, ValueType.Int8),
            TaskOpcode.ComparePrimaryEnemyCountGreaterThanOrEqualExcept => (TargetType.UnknownUInt16, ValueType.Int8),
            TaskOpcode.CompareBattleCharacterHealthLessThanOrEqual => (TargetType.UnknownUInt16, ValueType.Int8),
            TaskOpcode.CompareBattleCharacterHealthPercentLessThanOrEqual => (TargetType.UnknownUInt16, ValueType.Int8),

            TaskOpcode.CompareBattleCharacterHealthGreaterThanOrEqual => (TargetType.UnknownUInt16, ValueType.Int8),
            TaskOpcode.CompareBattleCharacterHealthPercentGreaterThanOrEqual => (TargetType.UnknownUInt16, ValueType.Int8),
            TaskOpcode.CompareStrongpointChild => (TargetType.None, ValueType.Int16),
            TaskOpcode.CompareBattlePartyIsOneUnit => (TargetType.None, ValueType.Int8),
            TaskOpcode.CompareHeroIsNaked => (TargetType.None, ValueType.Int8),
            TaskOpcode.CompareBattleUnitAtPositionIsPlayerFaction => (TargetType.UnknownUInt8, ValueType.None),
            TaskOpcode.CompareBattleCharacterHeartCountGreaterThanOrEqual => (TargetType.UnknownUInt16, ValueType.UInt8),
            TaskOpcode.CompareBattleCharacterHeartCountLessThanOrEqual => (TargetType.UnknownUInt16, ValueType.UInt8),
            TaskOpcode.CompareBattleUnitAtPositionCharacterEqual => (TargetType.UnknownUInt16, ValueType.Int8),
            TaskOpcode.CompareDungeonFlag => (TargetType.UnknownUInt16, ValueType.Int8),
            TaskOpcode.CompareWeather => (TargetType.None, ValueType.Int8),
            TaskOpcode.CompareBattleUnitAtPositionIsEntryUnit => (TargetType.UnknownUInt8, ValueType.Int8),
            TaskOpcode.CompareApproval => (TargetType.ApprovalRate, ValueType.UInt8),
            TaskOpcode.CompareCharacterLoyaltyGreaterThanOrEqual => (TargetType.UnknownUInt16, ValueType.UInt8),

            TaskOpcode.CompareBattleUnitReceivedAttack => (TargetType.UnknownUInt16, ValueType.None),
            TaskOpcode.CompareBattleUnitReceivedSupport => (TargetType.UnknownUInt16, ValueType.None),
            TaskOpcode.CompareBattlePartyIsThreeUnits => (TargetType.None, ValueType.UInt8),
            TaskOpcode.CompareHasItem => (TargetType.UnknownUInt16, ValueType.None),
            TaskOpcode.CompareCurrentShop => (TargetType.UnknownUInt8, ValueType.None),
            TaskOpcode.CompareOverallDeceasedUnitsCount => (TargetType.None, ValueType.UInt16),
            TaskOpcode.CompareOverallIncapacitatedUnitsCount => (TargetType.None, ValueType.UInt16),
            TaskOpcode.CompareOverallChariotAndRetreatingNotUsed => (TargetType.None, ValueType.UInt8),
            TaskOpcode.CompareHasCharacter => (TargetType.UnknownUInt16, ValueType.None),
            TaskOpcode.Unknown54 => (TargetType.None, ValueType.UInt8), // Reborn
            TaskOpcode.Unknown55 => (TargetType.UnknownUInt16, ValueType.UInt8), // Reborn
            TaskOpcode.Unknown56 => (TargetType.None, ValueType.UInt8), // Reborn
            _ => throw new NotSupportedException(),
        };

        internal static InvocationArgumentOrder GetArgumentOrder(this TaskOpcode opcode) => opcode switch
        {
            TaskOpcode.ComparePrimaryEnemyCountLessThanOrEqualExcept => InvocationArgumentOrder.ValueExpressionTarget,
            TaskOpcode.ComparePrimaryEnemyCountGreaterThanOrEqualExcept => InvocationArgumentOrder.ValueExpressionTarget,
            TaskOpcode.CompareBattleCharacterHealthLessThanOrEqual => InvocationArgumentOrder.ValueExpressionTarget,
            TaskOpcode.CompareBattleCharacterHealthPercentLessThanOrEqual => InvocationArgumentOrder.ValueExpressionTarget,

            TaskOpcode.CompareBattleCharacterHealthGreaterThanOrEqual => InvocationArgumentOrder.ValueExpressionTarget,
            TaskOpcode.CompareBattleCharacterHealthPercentGreaterThanOrEqual => InvocationArgumentOrder.ValueExpressionTarget,

            TaskOpcode.CompareBattleCharacterHeartCountGreaterThanOrEqual => InvocationArgumentOrder.ValueExpressionTarget,
            TaskOpcode.CompareBattleCharacterHeartCountLessThanOrEqual => InvocationArgumentOrder.ValueExpressionTarget,
            TaskOpcode.CompareBattleUnitAtPositionCharacterEqual => InvocationArgumentOrder.ValueExpressionTarget,

            TaskOpcode.CompareBattleUnitAtPositionIsEntryUnit => InvocationArgumentOrder.ValueExpressionTarget,
            TaskOpcode.CompareApproval => InvocationArgumentOrder.ExpressionTargetValue,
            TaskOpcode.CompareCharacterLoyaltyGreaterThanOrEqual => InvocationArgumentOrder.ValueExpressionTarget,

            TaskOpcode.CompareBattleUnitReceivedAttack => InvocationArgumentOrder.ValueExpressionTarget,
            TaskOpcode.CompareBattleUnitReceivedSupport => InvocationArgumentOrder.ValueExpressionTarget,

            TaskOpcode.CompareHasItem => InvocationArgumentOrder.ValueExpressionTarget,
            TaskOpcode.CompareCurrentShop => InvocationArgumentOrder.ValueExpressionTarget,

            TaskOpcode.CompareHasCharacter => InvocationArgumentOrder.ValueExpressionTarget,

            _ => InvocationArgumentOrder.TargetExpressionValue,
        };

        public static int GetCallbackAddressPSP(this TaskOpcode opcode) => opcode switch
        {
            TaskOpcode.Invalid => 0x088B6C08,
            TaskOpcode.SetGlobalFlag => 0x088B6C10,
            TaskOpcode.SetLocalFlag => 0x088B6C4C,
            TaskOpcode.ModifyApproval => 0x088B6D04,
            TaskOpcode.MoveToStrongpoint => 0x088B6D7C,
            TaskOpcode.ModifyGoth => 0x088B6E04,
            TaskOpcode.ModifyDays => 0x088B6E80,
            TaskOpcode.ModifyLoyaltyForBattlePartyLawfulUnits => 0x088B707C,
            TaskOpcode.ModifyLoyaltyForBattlePartyNeutralUnits => 0x088B70AC,
            TaskOpcode.ModifyLoyaltyForBattlePartyChaosUnits => 0x088B70DC,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterLawfulUnits => 0x088B71C0,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterNeutralUnits => 0x088B7210,
            TaskOpcode.ModifyLoyaltyForBattlePartyAndRosterChaosUnits => 0x088B7260,
            TaskOpcode.SetAlignmentForHero => 0x088B72B0,
            TaskOpcode.ResetGlobalFlagsInCategory => 0x088B739C,
            TaskOpcode.SetClanForPartyCharacter => 0x088B7460,
            TaskOpcode.ModifyLoyaltyForRosterUnitsInClan => 0x088B7524,
            TaskOpcode.SetLoyaltyForPartyCharacter => 0x088B75DC,
            TaskOpcode.MoveToStrongpointChild => 0x088B6DD4,
            TaskOpcode.SetClanForHero => 0x088B7668,
            TaskOpcode.AddItemToParty => 0x088B7718,
            TaskOpcode.SetActionStrategyForBattleUnit => 0x088B7984,
            TaskOpcode.SuppressDeathSceneForBattleUnit => 0x088B79E8,
            TaskOpcode.SetClearConditionMessageId => 0x088B7A20,
            TaskOpcode.NoOperation24 => 0x088B7A4C,
            TaskOpcode.SetClearConditionUnitId => 0x088B7A54,
            TaskOpcode.SetAllegianceForHeroAndParty => 0x088B7A80,
            TaskOpcode.SetFactionForCharacterInBattle => 0x088B7AA0,
            TaskOpcode.SetExtraGameOverConditionTextId => 0x088B7AF8,
            TaskOpcode.SetDungeonFlag => 0x088B6CC8,
            TaskOpcode.ResetDungeonFlags => 0x088B7B14,
            TaskOpcode.SetPartyCharacterClass => 0x088B7B48,
            TaskOpcode.RemoveItemFromParty => 0x088B7750,
            TaskOpcode.GiveAppellation => 0x088B7C20,
            TaskOpcode.IncreaseShopLevel => 0x088B7C60,
            TaskOpcode.SetTargetFlagForCharacterInBattle => 0x088B7C98,
            TaskOpcode.RemoveConsumableFromBattleParty => 0x088B7910,
            TaskOpcode.SetUnionLevel => 0x088B6C08,
            TaskOpcode.SetChariotTurnLimit => 0x088B6C08,
            TaskOpcode.Unknown27 => 0x088B6C08,
            TaskOpcode.Unknown28 => 0x088B6C08,
            TaskOpcode.NoOperation41 => 0x088B6C08,
            TaskOpcode.NoOperation42 => 0x088B6C08,
            TaskOpcode.NoOperation43 => 0x088B6C08,
            TaskOpcode.NoOperation44 => 0x088B6C08,
            TaskOpcode.NoOperation45 => 0x088B6C08,
            TaskOpcode.NoOperation46 => 0x088B6C08,
            TaskOpcode.NoOperation47 => 0x088B6C08,
            TaskOpcode.CompareGlobalFlag => 0x088B7CE8,
            TaskOpcode.CompareLocalFlag => 0x088B7D68,
            TaskOpcode.CompareStrongpoint => 0x088B7E74,
            TaskOpcode.CompareBattleActiveCharacter => 0x088B7F34,
            TaskOpcode.ComparePrimaryEnemyCount => 0x088B7FC0,
            TaskOpcode.ComparePrimaryEnemyCountLessThanOrEqualExcept => 0x088B8024,
            TaskOpcode.ComparePrimaryEnemyCountGreaterThanOrEqualExcept => 0x088B80C0,
            TaskOpcode.CompareBattleCharacterHealthLessThanOrEqual => 0x088B815C,
            TaskOpcode.CompareBattleCharacterHealthPercentLessThanOrEqual => 0x088B81D8,
            TaskOpcode.CompareBattleCharacterMagicLessThanOrEqual => 0x088B827C,
            TaskOpcode.CompareBattleCharacterMagicPercentLessThanOrEqual => 0x088B82F8,
            TaskOpcode.CompareBattleCharacterHealthPercentInRange => 0x088B839C,
            TaskOpcode.CompareBattleCharacterHealthGreaterThanOrEqual => 0x088B8498,
            TaskOpcode.CompareBattleCharacterHealthPercentGreaterThanOrEqual => 0x088B8514,
            TaskOpcode.CompareStrongpointChild => 0x088B7ECC,
            TaskOpcode.CompareBattlePartyIsOneUnit => 0x088B85B8,
            TaskOpcode.CompareHeroIsNaked => 0x088B8630,
            TaskOpcode.CompareBattleUnitAtPositionIsPlayerFaction => 0x088B86D0,
            TaskOpcode.CompareBattleCharacterHeartCountGreaterThanOrEqual => 0x088B874C,
            TaskOpcode.CompareBattleCharacterHeartCountLessThanOrEqual => 0x088B87CC,
            TaskOpcode.CompareBattleUnitAtPositionCharacterEqual => 0x088B884C,
            TaskOpcode.CompareDungeonFlag => 0x088B7DF4,
            TaskOpcode.CompareWeather => 0x088B88E4,
            TaskOpcode.CompareBattleUnitAtPositionIsEntryUnit => 0x088B893C,
            TaskOpcode.CompareApproval => 0x088B89FC,
            TaskOpcode.CompareCharacterLoyaltyGreaterThanOrEqual => 0x088B8A9C,
            TaskOpcode.CompareCharacterLoyaltyLessThanOrEqual => 0x088B8B28,
            TaskOpcode.CompareBattleUnitReceivedAttack => 0x088B8BB4,
            TaskOpcode.CompareBattleUnitReceivedSupport => 0x088B8C64,
            TaskOpcode.CompareBattlePartyIsThreeUnits => 0x088B8D14,
            TaskOpcode.CompareHasItem => 0x088B8D8C,
            TaskOpcode.CompareCurrentShop => 0x088B8E18,
            TaskOpcode.CompareOverallDeceasedUnitsCount => 0x088B8E68,
            TaskOpcode.CompareOverallIncapacitatedUnitsCount => 0x088B8EF8,
            TaskOpcode.CompareOverallChariotAndRetreatingNotUsed => 0x088B8F88,
            TaskOpcode.CompareHasCharacter => 0x088B9078,
            TaskOpcode.Unknown54 => 0x088B6C08,
            TaskOpcode.Unknown55 => 0x088B6C08,
            TaskOpcode.Unknown56 => 0x088B6C08,
            TaskOpcode.NoOperation87 => 0x088B6C08,
            TaskOpcode.NoOperation88 => 0x088B6C08,
            TaskOpcode.NoOperation89 => 0x088B6C08,
            TaskOpcode.NoOperation90 => 0x088B6C08,
            TaskOpcode.NoOperation91 => 0x088B6C08,
            TaskOpcode.NoOperation92 => 0x088B6C08,
            TaskOpcode.NoOperation93 => 0x088B6C08,
            TaskOpcode.NoOperation94 => 0x088B6C08,
            TaskOpcode.NoOperation95 => 0x088B6C08,
            TaskOpcode.AndCount => 0x088B90FC,
            TaskOpcode.AndOrAnd => 0x088B91C4,
            TaskOpcode.AndAndOrAnd => 0x088B9258,
            TaskOpcode.AndAndAndOr => 0x088B92F8,
            TaskOpcode.AndOr => 0x088B9398,
            TaskOpcode.OrOrOrOrOrOrAnd => 0x088B9420,
            TaskOpcode.OrAndOrAndOrAndOr => 0x088B94E4,
            TaskOpcode.Or => 0x088B95A4,
            TaskOpcode.OrOr => 0x088B9618,
            TaskOpcode.AndAndAndOrOr => 0x088B969C,
            TaskOpcode.AndAndAndAndAndAndOr => 0x088B9754,
            TaskOpcode.AndAndAndAndAndAndAndAndAndAndAndOr => 0x088B9800,
            TaskOpcode.AndAndAndAndAndAndAndAndAndOr => 0x088B98AC,
            TaskOpcode.AndAndAndOrOrOr => 0x088B9958,
            TaskOpcode.AndAndAndAndAndOr => 0x088B9A1C,
            TaskOpcode.AndAndAndAndAndAndOrAnd => 0x088B9AC8,
            _ => throw new ArgumentOutOfRangeException(nameof(opcode)),
        };
    }
}
