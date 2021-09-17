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
    public struct UnitPoints
    {
        public ushort Health;
        public ushort Magic;
        public ushort Tactical;

        public static UnitPoints Read(Stream input, Endian endian)
        {
            UnitPoints instance;
            instance.Health = input.ReadValueU16(endian);
            instance.Magic = input.ReadValueU16(endian);
            instance.Tactical = input.ReadValueU16(endian);
            return instance;
        }

        public static void Write(Stream output, UnitPoints instance, Endian endian)
        {
            output.WriteValueU16(instance.Health, endian);
            output.WriteValueU16(instance.Magic, endian);
            output.WriteValueU16(instance.Tactical, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }

        public override string ToString()
        {
            return $"hp={Health} mp={Magic} tp={Tactical}";
        }
    }
}
