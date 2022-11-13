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

namespace Gibbed.LetUsClingTogether.FileFormats.Screenplay
{
    public struct InvocationInstruction
    {
        public TaskOpcode Opcode;
        public int Target;
        public LogicalExpression Expression;
        public int Value;

        public override string ToString()
        {
            if (this.Opcode.GetTaskType() == TaskType.Expression)
            {
                return $"{this.Opcode} {this.Value}";
            }
            var (targetType, valueType) = this.Opcode.GetArguments();
            if (targetType == TargetType.None)
            {
                return valueType == ValueType.None
                    ? $"{this.Opcode}"
                    : $"{this.Opcode} {this.Expression} {this.Value}";
            }
            else if (valueType == ValueType.None)
            {
                return targetType == TargetType.None
                    ? $"{this.Opcode}"
                    : $"{this.Opcode} {this.Target} {this.Expression}";
            }
            return $"{this.Opcode} {this.Target} {this.Expression} {this.Value}";
        }
    }
}
