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
    public struct Unit
    {
        public bool IsEnabled;
        public const int NameLength = 37;
        public byte[] NameBytes;
        public const int BattleCryCount = 8;
        public BattleCry[] BattleCries;
        public byte Unknown1EE;
        public byte IdNumber;
        public UnitState State;

        public static Unit Read(Stream input, Endian endian)
        {
            Unit instance;
            instance.IsEnabled = input.ReadValueB8();
            instance.NameBytes = input.ReadBytes(NameLength);
            instance.BattleCries = new BattleCry[BattleCryCount];
            for (int i = 0; i < BattleCryCount; i++)
            {
                instance.BattleCries[i] = BattleCry.Read(input);
            }
            instance.Unknown1EE = input.ReadValueU8();
            instance.IdNumber = input.ReadValueU8();
            instance.State = UnitState.Read(input, endian);
            return instance;
        }

        public static void Write(Stream output, Unit instance, Endian endian)
        {
            output.WriteValueB8(instance.IsEnabled);
            output.WriteBytes(instance.NameBytes);
            ArrayHelper.ForEach(instance.BattleCries, e => BattleCry.Write(output, e), BattleCryCount);
            output.WriteValueU8(instance.Unknown1EE);
            output.WriteValueU8(instance.IdNumber);
            instance.State.Write(output, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }
    }
}
