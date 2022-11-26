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

namespace Gibbed.LetUsClingTogether.FileFormats.ChallengeParty
{
    public struct CursedWeapon
    {
        public byte Unknown00;
        public const int NameLength = 37;
        public byte[] NameBytes;
        public byte RecoveryTimeBonus;
        public byte Attack;
        public byte Defense;
        public byte HealthPoints;
        public byte MagicPoints;
        public byte Strength;
        public byte Vitality;
        public byte Dexterity;
        public byte Agility;
        public byte Avoidance;
        public byte Intelligence;
        public byte Mind;
        public byte Resistance;
        public byte Unknown33; // physical type(?)
        public byte Unknown34; // physical damage bonus(?)
        public byte Unknown35;
        public byte Unknown36; // race type(?)
        public byte Unknown37; // race damage bonus(?)
        public byte Unknown38; // element type(?)
        public byte Unknown39; // element damage bonus(?)

        public static CursedWeapon Read(Stream input)
        {
            CursedWeapon instance;
            instance.Unknown00 = input.ReadValueU8();
            instance.NameBytes = input.ReadBytes(NameLength);
            instance.RecoveryTimeBonus = input.ReadValueU8();
            instance.Attack = input.ReadValueU8();
            instance.Defense = input.ReadValueU8();
            instance.HealthPoints = input.ReadValueU8();
            instance.MagicPoints = input.ReadValueU8();
            instance.Strength = input.ReadValueU8();
            instance.Vitality = input.ReadValueU8();
            instance.Dexterity = input.ReadValueU8();
            instance.Agility = input.ReadValueU8();
            instance.Avoidance = input.ReadValueU8();
            instance.Intelligence = input.ReadValueU8();
            instance.Mind = input.ReadValueU8();
            instance.Resistance = input.ReadValueU8();
            instance.Unknown33 = input.ReadValueU8();
            instance.Unknown34 = input.ReadValueU8();
            instance.Unknown35 = input.ReadValueU8();
            instance.Unknown36 = input.ReadValueU8();
            instance.Unknown37 = input.ReadValueU8();
            instance.Unknown38 = input.ReadValueU8();
            instance.Unknown39 = input.ReadValueU8();
            return instance;
        }

        public static void Write(Stream output, CursedWeapon instance)
        {
            output.WriteValueU8(instance.Unknown00);
            output.WriteBytes(instance.NameBytes);
            output.WriteValueU8(instance.RecoveryTimeBonus);
            output.WriteValueU8(instance.Attack);
            output.WriteValueU8(instance.Defense);
            output.WriteValueU8(instance.HealthPoints);
            output.WriteValueU8(instance.MagicPoints);
            output.WriteValueU8(instance.Strength);
            output.WriteValueU8(instance.Vitality);
            output.WriteValueU8(instance.Dexterity);
            output.WriteValueU8(instance.Agility);
            output.WriteValueU8(instance.Avoidance);
            output.WriteValueU8(instance.Intelligence);
            output.WriteValueU8(instance.Mind);
            output.WriteValueU8(instance.Resistance);
            output.WriteValueU8(instance.Unknown33);
            output.WriteValueU8(instance.Unknown34);
            output.WriteValueU8(instance.Unknown35);
            output.WriteValueU8(instance.Unknown36);
            output.WriteValueU8(instance.Unknown37);
            output.WriteValueU8(instance.Unknown38);
            output.WriteValueU8(instance.Unknown39);
        }

        public void Write(Stream output)
        {
            Write(output, this);
        }
    }
}
