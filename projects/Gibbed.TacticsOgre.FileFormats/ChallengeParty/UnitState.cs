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

using System.IO;
using Gibbed.IO;

namespace Gibbed.TacticsOgre.FileFormats.ChallengeParty
{
    public struct UnitState
    {
        public ushort BaseCharacter;
        public ushort Unknown002;
        public ushort Portrait;
        public ushort Unknown006;
        public byte Unknown008;
        public byte Unknown009;
        public byte Unknown00A;
        public byte Life;
        public uint Unknown00C;
        public UnitPoints CurrentPoints;
        public UnitPoints MaximumPoints;
        public uint Unknown01C;
        public ushort Unknown020;
        public ushort Unknown022;
        public UnitStats BaseStats;
        public UnitStats Stats;
        public byte Luck;
        public byte Loyalty;
        public byte Unknown046;
        public byte Unknown047;
        public byte Unknown048; // condition(?)
        public byte ClassId;
        public byte ClassLevel;
        public byte MovementType;
        public uint Flags;
        public byte Unknown050;
        public byte Unknown051;
        public byte Allegiance;
        public byte AIModifier; 
        public const int EquipmentCount = 5;
        public ushort[] EquipmentIds;
        public ushort[] EquipmentUnknowns;
        public const int ConditionCount = 46;
        public byte[] ConditionBits;
        public byte Unknown096; 
        public const int Unknown097Count = 13;
        public byte[] Unknown097;
        public byte AssignedSkillActiveCount;
        public byte Unknown0A5;
        public const int AssignedSkillCount = 10;
        public UnitAssignedSkill[] AssignedSkills;
        public const int SpellCount = 33;
        public byte[] SpellBits;
        public byte Unknown103;
        public byte Unknown104;
        public byte Unknown105;
        public byte Unknown106;
        public byte Unknown107;

        public static UnitState Read(Stream input, Endian endian)
        {
            UnitState instance;
            instance.BaseCharacter = input.ReadValueU16(endian);
            instance.Unknown002 = input.ReadValueU16(endian);
            instance.Portrait = input.ReadValueU16(endian);
            instance.Unknown006 = input.ReadValueU16(endian);
            instance.Unknown008 = input.ReadValueU8();
            instance.Unknown009 = input.ReadValueU8();
            instance.Unknown00A = input.ReadValueU8();
            instance.Life = input.ReadValueU8();
            instance.Unknown00C = input.ReadValueU32(endian);
            instance.CurrentPoints = UnitPoints.Read(input, endian);
            instance.MaximumPoints = UnitPoints.Read(input, endian);
            instance.Unknown01C = input.ReadValueU32(endian);
            instance.Unknown020 = input.ReadValueU16(endian);
            instance.Unknown022 = input.ReadValueU16(endian);
            instance.BaseStats = UnitStats.Read(input, endian);
            instance.Stats = UnitStats.Read(input, endian);
            instance.Luck = input.ReadValueU8();
            instance.Loyalty = input.ReadValueU8();
            instance.Unknown046 = input.ReadValueU8();
            instance.Unknown047 = input.ReadValueU8();
            instance.Unknown048 = input.ReadValueU8();
            instance.ClassId = input.ReadValueU8();
            instance.ClassLevel = input.ReadValueU8();
            instance.MovementType = input.ReadValueU8();
            instance.Flags = input.ReadValueU32(endian);
            instance.Unknown050 = input.ReadValueU8();
            // ???
            // "Share skills and SP (unit will have the learned skills and SP of the unit number in this field)
            // By default the value should be the same as the unit id)."
            instance.Unknown051 = input.ReadValueU8();
            instance.Allegiance = input.ReadValueU8();
            instance.AIModifier = input.ReadValueU8();
            instance.EquipmentIds = new ushort[EquipmentCount];
            for (int i = 0; i < EquipmentCount; i++)
            {
                instance.EquipmentIds[i] = input.ReadValueU16(endian);
            }
            instance.EquipmentUnknowns = new ushort[EquipmentCount];
            for (int i = 0; i < EquipmentCount; i++)
            {
                instance.EquipmentUnknowns[i] = input.ReadValueU16(endian);
            }
            instance.ConditionBits = input.ReadBytes(ConditionCount);
            instance.Unknown096 = input.ReadValueU8();
            instance.Unknown097 = input.ReadBytes(Unknown097Count);
            instance.AssignedSkillActiveCount = input.ReadValueU8();
            instance.Unknown0A5 = input.ReadValueU8();
            instance.AssignedSkills = new UnitAssignedSkill[AssignedSkillCount];
            for (int i = 0; i < AssignedSkillCount; i++)
            {
                instance.AssignedSkills[i] = UnitAssignedSkill.Read(input, endian);
            }
            instance.SpellBits = input.ReadBytes(SpellCount);
            instance.Unknown103 = input.ReadValueU8();
            instance.Unknown104 = input.ReadValueU8();
            instance.Unknown105 = input.ReadValueU8();
            instance.Unknown106 = input.ReadValueU8();
            instance.Unknown107 = input.ReadValueU8();
            return instance;
        }

        public static void Write(Stream output, UnitState instance, Endian endian)
        {
            output.WriteValueU16(instance.BaseCharacter, endian);
            output.WriteValueU16(instance.Unknown002, endian);
            output.WriteValueU16(instance.Portrait, endian);
            output.WriteValueU16(instance.Unknown006, endian);
            output.WriteValueU8(instance.Unknown008);
            output.WriteValueU8(instance.Unknown009);
            output.WriteValueU8(instance.Unknown00A);
            output.WriteValueU8(instance.Life);
            output.WriteValueU32(instance.Unknown00C, endian);
            instance.CurrentPoints.Write(output, endian);
            instance.MaximumPoints.Write(output, endian);
            output.WriteValueU32(instance.Unknown01C, endian);
            output.WriteValueU16(instance.Unknown020, endian);
            output.WriteValueU16(instance.Unknown022, endian);
            instance.BaseStats.Write(output, endian);
            instance.Stats.Write(output, endian);
            output.WriteValueU8(instance.Luck);
            output.WriteValueU8(instance.Loyalty);
            output.WriteValueU8(instance.Unknown046);
            output.WriteValueU8(instance.Unknown047);
            output.WriteValueU8(instance.Unknown048);
            output.WriteValueU8(instance.ClassId);
            output.WriteValueU8(instance.ClassLevel);
            output.WriteValueU8(instance.MovementType);
            output.WriteValueU32(instance.Flags, endian);
            output.WriteValueU8(instance.Unknown050);
            output.WriteValueU8(instance.Unknown051);
            output.WriteValueU8(instance.Allegiance);
            output.WriteValueU8(instance.AIModifier);
            ArrayHelper.ForEach(instance.EquipmentIds, v => output.WriteValueU16(v, endian), EquipmentCount);
            ArrayHelper.ForEach(instance.EquipmentUnknowns, v => output.WriteValueU16(v, endian), EquipmentCount);
            ArrayHelper.ForEach(instance.ConditionBits, v => output.WriteValueU8(v), ConditionCount);
            output.WriteValueU8(instance.Unknown096);
            ArrayHelper.ForEach(instance.Unknown097, v => output.WriteValueU8(v), Unknown097Count);
            output.WriteValueU8(instance.AssignedSkillActiveCount);
            output.WriteValueU8(instance.Unknown0A5);
            ArrayHelper.ForEach(instance.AssignedSkills, e => UnitAssignedSkill.Write(output, e, endian), AssignedSkillCount);
            ArrayHelper.ForEach(instance.SpellBits, v => output.WriteValueU8(v), SpellCount);
            output.WriteValueU8(instance.Unknown103);
            output.WriteValueU8(instance.Unknown104);
            output.WriteValueU8(instance.Unknown105);
            output.WriteValueU8(instance.Unknown106);
            output.WriteValueU8(instance.Unknown107);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }
    }
}
