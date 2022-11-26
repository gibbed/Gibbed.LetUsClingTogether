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

using System;
using System.IO;
using Gibbed.IO;
using Gibbed.TacticsOgre.FileFormats.ChallengeParty;

namespace Gibbed.TacticsOgre.FileFormats
{
    public class ChallengePartyFile
    {
        public Endian Endian { get; set; }
        public ushort BattlefieldId { get; set; }
        public bool RankLimitEnabled { get; set; }
        public ushort MapNameId { get; set; }
        public Party Party { get; set; }

        public void Serialize(Stream output)
        {
            var endian = this.Endian;
            const uint signature = 0x81000012;
            output.WriteValueU32(signature, endian);
            output.WriteValueU32(0, endian);
            output.WriteValueU32(0x14EF9, endian);
            output.WriteValueU32(0, endian);
            output.WriteValueU32(0x80, endian);
            output.WriteValueU32(11472, endian);
            output.WriteValueU16(this.BattlefieldId, endian);
            output.WriteValueU16(0, endian);
            output.WriteValueB8(this.RankLimitEnabled);
            output.WriteValueU8(0);
            output.WriteValueU16(this.MapNameId, endian);
            output.WriteValueU32(0, endian);
            output.WriteValueU32(signature, endian);
            this.Party.Write(output, endian);
            output.WriteValueU32(0xFFFF, endian);
            output.WriteValueU32(0, endian);
        }

        public void Deserialize(Stream input)
        {
            const uint signature = 0x81000012;

            var unknown0000 = input.ReadValueU32(Endian.Little);
            if (unknown0000 != signature && unknown0000.Swap() != signature)
            {
                throw new FormatException();
            }
            var endian = unknown0000 == signature ? Endian.Little : Endian.Big;

            var unknown0004 = input.ReadValueU32(endian);
            var unknown0008 = input.ReadValueU32(endian);
            var unknown000C = input.ReadValueU32(endian);
            var unknown0010 = input.ReadValueU32(endian);
            var dataSize = input.ReadValueU32(endian);
            var battlefieldId = input.ReadValueU16(endian);
            var unknown001A = input.ReadValueU16(endian);
            var rankLimitEnabled = input.ReadValueB8();
            var unknown001D = input.ReadValueU8();
            var mapNameId = input.ReadValueU16(endian);
            var unknown0020 = input.ReadValueU32(endian);
            var unknown0024 = input.ReadValueU32(endian);

            if (unknown0004 != 0 ||
                unknown0008 != 0x14EF9 ||
                unknown000C != 0 ||
                unknown0010 != 0x80 ||
                dataSize != 11472 ||
                unknown001A != 0 ||
                unknown001D != 0 ||
                unknown0020 != 0)
            {
                throw new FormatException();
            }

            if (unknown0024 != unknown0000)
            {
                throw new FormatException();
            }

            var party = Party.Read(input, endian);

            var unknown2CE8 = input.ReadValueU32(endian);
            var unknown2CEC = input.ReadValueU32(endian);
            if (unknown2CE8 != 0xFFFF || unknown2CEC != 0)
            {
                throw new FormatException();
            }

            this.Endian = endian;
            this.BattlefieldId = battlefieldId;
            this.RankLimitEnabled = rankLimitEnabled;
            this.MapNameId = mapNameId;
            this.Party = party;
        }
    }
}
