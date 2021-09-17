/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
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
    public struct UnitAssignedSkill
    {
        public ushort Id;
        public byte Rank;
        public byte Unknown;
        public ushort Experience;

        public static UnitAssignedSkill Read(Stream input, Endian endian)
        {
            UnitAssignedSkill instance;
            instance.Id = input.ReadValueU16(endian);
            instance.Rank = input.ReadValueU8();
            instance.Unknown = input.ReadValueU8();
            instance.Experience = input.ReadValueU16(endian);
            return instance;
        }

        public static void Write(Stream output, UnitAssignedSkill instance, Endian endian)
        {
            output.WriteValueU16(instance.Id, endian);
            output.WriteValueU8(instance.Rank);
            output.WriteValueU8(instance.Unknown);
            output.WriteValueU16(instance.Experience, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }
    }
}
