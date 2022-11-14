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
    public class ScreenplayTaskFile : BaseTaskFile
    {
        public const uint Signature = 0x4B534154; // 'TASK'

        private readonly Dictionary<uint, List<ProcessingTaskInstruction>> _Entries;

        public Endian Endian { get; set; }
        public Dictionary<uint, List<ProcessingTaskInstruction>> Entries => this._Entries;

        public ScreenplayTaskFile()
        {
            this.Endian = Endian.Little;
            this._Entries = new();
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

            var ids = new uint[count];
            for (int i = 0; i < count; i++)
            {
                ids[i] = input.ReadValueU16(endian);
            }
            CheckAndSkipPadding(input, 4);

            var offsets = new uint[count];
            for (int i = 0; i < count; i++)
            {
                offsets[i] = input.ReadValueU32(endian);
            }
            CheckAndSkipPadding(input, 4);

            Dictionary<uint, List<ProcessingTaskInstruction>> entries = new();
            for (int i = 0; i < count; i++)
            {
                var taskId = ids[i];
                input.Position = offsets[i];
                List<ProcessingTaskInstruction> instructions = new();
                while (true)
                {
                    var taskOpcode = (TaskOpcode)input.ReadValueU16(endian);
                    if (taskOpcode == TaskOpcode.End)
                    {
                        break;
                    }
                    ProcessingTaskInstruction task;
                    task.Opcode = taskOpcode;
                    var size = taskOpcode.GetSize();
                    var extraBytes = input.ReadBytes(size);
                    using (MemoryStream extra = new(extraBytes, false))
                    {
                        var (targetType, valueType) = taskOpcode.GetArguments();
                        task.Target = ReadTaskTarget(targetType, extra, endian);
                        task.Value = ReadTaskValue(valueType, extra, endian);
                        if (taskOpcode == TaskOpcode.SetActionStrategyForBattleUnit)
                        {
                            // SetActionStrategyForBattleUnit has an extra unused byte
                            if (extra.Position + 1 != extra.Length)
                            {
                                throw new FormatException();
                            }
                        }
                        else
                        {
                            if (extra.Position != extra.Length)
                            {
                                throw new FormatException();
                            }
                        }
                    }
                    instructions.Add(task);
                }
                entries.Add(taskId, instructions);
            }

            this.Endian = endian;
            this.Entries.Clear();
            foreach (var kv in entries)
            {
                this.Entries.Add(kv.Key, kv.Value);
            }
        }
    }
}
