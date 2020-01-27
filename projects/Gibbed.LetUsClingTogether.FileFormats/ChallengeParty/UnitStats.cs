/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
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
    public struct UnitStats
    {
        public ushort Vitality;
        public ushort Strength;
        public ushort Agility;
        public ushort Dexterity;
        public ushort Avoidance;
        public ushort Intelligence;
        public ushort Mind;
        public ushort Resistance;

        public static UnitStats Read(Stream input, Endian endian)
        {
            UnitStats instance;
            instance.Vitality = input.ReadValueU16(endian);
            instance.Strength = input.ReadValueU16(endian);
            instance.Agility = input.ReadValueU16(endian);
            instance.Dexterity = input.ReadValueU16(endian);
            instance.Avoidance = input.ReadValueU16(endian);
            instance.Intelligence = input.ReadValueU16(endian);
            instance.Mind = input.ReadValueU16(endian);
            instance.Resistance = input.ReadValueU16(endian);
            return instance;
        }

        public static void Write(Stream output, UnitStats instance, Endian endian)
        {
            output.WriteValueU16(instance.Vitality, endian);
            output.WriteValueU16(instance.Strength, endian);
            output.WriteValueU16(instance.Agility, endian);
            output.WriteValueU16(instance.Dexterity, endian);
            output.WriteValueU16(instance.Avoidance, endian);
            output.WriteValueU16(instance.Intelligence, endian);
            output.WriteValueU16(instance.Mind, endian);
            output.WriteValueU16(instance.Resistance, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }
    }
}
