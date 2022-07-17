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
    public struct VariableFlags
    {
        public int Id;
        public VariableType Type;
        public bool Unknown;
        public VariableScope Scope;

        public int Offset => this.Id;

        public static VariableFlags Read(Stream input, Endian endian)
        {
            var flags = input.ReadValueU32(endian);

            var id = (int)(flags & 0xFFFFFF);
            var type = (VariableType)((flags >> 28) & 0xF);
            var unknown = ((flags >> 27) & 0x1) != 0;
            var scope = (VariableScope)((flags >> 24) & 0x7);

            if (unknown == true)
            {
            }

            if (IsKnown(type) == false || IsKnown(scope) == false)
            {
                throw new NotImplementedException();
            }

            VariableFlags instance;
            instance.Id = id;
            instance.Type = type;
            instance.Unknown = unknown;
            instance.Scope = scope;
            return instance;
        }

        private static bool IsKnown(VariableType type) => type switch
        {
            VariableType.Byte => true,
            VariableType.Integer => true,
            VariableType.Float => true,
            _ => throw new NotSupportedException(),
        };

        private static bool IsKnown(VariableScope scope) => scope switch
        {
            VariableScope.Global => true,
            VariableScope.File => true,
            VariableScope.Script => true,
            VariableScope.Array => true,
            _ => throw new NotSupportedException(),
        };

        public override string ToString()
        {
            return $"{this.Id:X6} {this.Type} {this.Unknown} {this.Scope}";
        }
    }
}
