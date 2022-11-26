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
    public struct BattleCry
    {
        public bool IsEnabled;
        public byte Type;
        public const int MessageLength = 55;
        public byte[] MessageBytes;

        public static BattleCry Read(Stream input)
        {
            BattleCry instance;
            instance.IsEnabled = input.ReadValueB8();
            instance.Type = input.ReadValueU8();
            instance.MessageBytes = input.ReadBytes(55);
            return instance;
        }

        public static void Write(Stream output, BattleCry instance)
        {
            output.WriteValueB8(instance.IsEnabled);
            output.WriteValueU8(instance.Type);
            output.WriteBytes(instance.MessageBytes);
        }

        public void Write(Stream output)
        {
            Write(output, this);
        }
    }
}
