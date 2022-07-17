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

namespace Gibbed.LetUsClingTogether.ScriptFormats
{
    public struct VariableArrayHeader
    {
        public VariableFlags Flags;
        public int Rank;
        public int[] Lengths;

        public static VariableArrayHeader Read(Stream input, Endian endian)
        {
            var flags = VariableFlags.Read(input, endian);
            if (flags.Scope == VariableScope.Array)
            {
                throw new InvalidOperationException();
            }
            VariableArrayHeader instance;
            instance.Flags = flags;
            instance.Rank = input.ReadValueS32(endian);
            instance.Lengths = new int[2];
            instance.Lengths[0] = input.ReadValueS32(endian);
            instance.Lengths[1] = input.ReadValueS32(endian);
            return instance;
        }

        public override string ToString()
        {
            return this.Rank switch
            {
                1 => $"{this.Flags} [{this.Lengths[0]}]",
                2 => $"{this.Flags} [{this.Lengths[0]}, {this.Lengths[1]}]",
                _ => $"{this.Flags} {this.Rank} ???",
            };
        }
    }
}
