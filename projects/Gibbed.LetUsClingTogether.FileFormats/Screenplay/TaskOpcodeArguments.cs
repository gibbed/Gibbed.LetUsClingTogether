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

namespace Gibbed.LetUsClingTogether.FileFormats.Screenplay
{
    public struct TaskOpcodeArguments : IEquatable<TaskOpcodeArguments>
    {
        public readonly TargetType TargetType;
        public readonly ValueType ValueType;

        public TaskOpcodeArguments(TargetType targetType, ValueType valueType)
        {
            this.TargetType = targetType;
            this.ValueType = valueType;
        }

        public bool Equals(TaskOpcodeArguments other)
        {
            return
                this.TargetType == other.TargetType &&
                this.ValueType == other.ValueType;
        }

        public override bool Equals(object obj)
        {
            return obj is TaskOpcodeArguments other && this.Equals(other) == true;
        }

        public override int GetHashCode()
        {
            int hashCode = -1723330934;
            hashCode = hashCode * -1521134295 + this.TargetType.GetHashCode();
            hashCode = hashCode * -1521134295 + this.ValueType.GetHashCode();
            return hashCode;
        }

        public void Deconstruct(out TargetType targetType, out ValueType valueType)
        {
            targetType = this.TargetType;
            valueType = this.ValueType;
        }

        public static implicit operator (TargetType targetType, ValueType valueType)(TaskOpcodeArguments value)
        {
            return (value.TargetType, value.ValueType);
        }

        public static implicit operator TaskOpcodeArguments((TargetType targetType, ValueType valueType) value)
        {
            return new(value.targetType, value.valueType);
        }
    }
}
