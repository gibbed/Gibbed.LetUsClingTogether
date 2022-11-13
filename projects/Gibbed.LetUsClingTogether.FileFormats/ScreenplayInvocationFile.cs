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
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using Gibbed.LetUsClingTogether.FileFormats.Screenplay;

namespace Gibbed.LetUsClingTogether.FileFormats
{
    public class ScreenplayInvocationFile : BaseTaskFile
    {
        public const uint Signature = 0x4B564E49; // 'INVK'

        // TODO(gibbed): make the tuple an actual type.
        private readonly Dictionary<InvocationType, List<(ushort id, List<InvocationInstruction> entries)>> _Sections = new();

        public Endian Endian { get; set; }
        public Dictionary<InvocationType, List<(ushort id, List<InvocationInstruction> entries)>> Sections => this._Sections;

        public ScreenplayInvocationFile()
        {
            this.Endian = Endian.Little;
            this._Sections = new();
        }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            var count = input.ReadValueU16(endian);
            CheckAndSkipPadding(input, 4);

            var offsets = new uint[count];
            for (int i = 0; i < count; i++)
            {
                offsets[i] = input.ReadValueU32(endian);
            }

            Dictionary<InvocationType, List<(ushort id, List<InvocationInstruction>)>> sections = new();
            for (int i = 0; i < count; i++)
            {
                input.Position = offsets[i];
                var childCount = input.ReadValueU16(endian);

                List<(ushort id, List<InvocationInstruction>)> children;
                if (childCount == 0)
                {
                    var taskId = input.ReadValueU16(endian);
                    if (taskId != 0xFFFF)
                    {
                        throw new FormatException();
                    }

                    if (i + 1 < count && input.Position != offsets[i + 1])
                    {
                        throw new FormatException();
                    }

                    children = new();
                }
                else
                {
                    children = new();
                    for (int j = 0; j < childCount; j++)
                    {
                        var taskId = input.ReadValueU16(endian);
                        List<InvocationInstruction> instructions = new();
                        while (true)
                        {
                            var taskOpcode = (TaskOpcode)input.ReadValueU16(endian);
                            if (taskOpcode == TaskOpcode.End)
                            {
                                break;
                            }
                            InvocationInstruction instruction;
                            instruction.Opcode = taskOpcode;
                            var size = taskOpcode.GetSize();
                            var extraBytes = input.ReadBytes(size);
                            using (MemoryStream extra = new(extraBytes, false))
                            {
                                if (taskOpcode.GetTaskType() == TaskType.Expression)
                                {
                                    instruction.Target = default;
                                    instruction.Expression = default;
                                    instruction.Value = extra.ReadValueU8();
                                }
                                else
                                {
                                    var (targetType, valueType) = taskOpcode.GetArguments();
                                    switch (taskOpcode.GetArgumentOrder())
                                    {
                                        case InvocationArgumentOrder.TargetExpressionValue:
                                        {
                                            instruction.Target = ReadTaskTarget(targetType, extra, endian);
                                            instruction.Expression = (LogicalExpression)extra.ReadValueU8();
                                            instruction.Value = ReadTaskValue(valueType, extra, endian);
                                            break;
                                        }
                                        case InvocationArgumentOrder.ValueExpressionTarget:
                                        {
                                            instruction.Value = ReadTaskValue(valueType, extra, endian);
                                            instruction.Expression = (LogicalExpression)extra.ReadValueU8();
                                            instruction.Target = ReadTaskTarget(targetType, extra, endian);
                                            break;
                                        }
                                        case InvocationArgumentOrder.ExpressionTargetValue:
                                        {
                                            instruction.Expression = (LogicalExpression)extra.ReadValueU8();
                                            instruction.Target = ReadTaskTarget(targetType, extra, endian);
                                            instruction.Value = ReadTaskValue(valueType, extra, endian);
                                            break;
                                        }
                                        default:
                                        {
                                            throw new NotImplementedException();
                                        }
                                    }
                                }
                                if (extra.Position != extra.Length)
                                {
                                    throw new FormatException();
                                }
                            }
                            instructions.Add(instruction);
                        }
                        children.Add((taskId, instructions));
                    }
                }
                sections.Add((InvocationType)i, children);

                if (i + 1 < count && input.Position != offsets[i + 1])
                {
                    throw new FormatException();
                }
            }

            this.Endian = endian;
            this.Sections.Clear();
            foreach (var kv in sections)
            {
                this.Sections.Add(kv.Key, kv.Value);
            }
        }
    }
}
