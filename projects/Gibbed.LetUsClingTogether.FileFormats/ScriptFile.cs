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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using Gibbed.LetUsClingTogether.FileFormats.Script;

namespace Gibbed.LetUsClingTogether.FileFormats
{
    public class ScriptFile
    {
        private static readonly Encoding SJIS;

        static ScriptFile()
        {
            SJIS = Encoding.GetEncoding(932);
        }

        private readonly List<ushort> _EventCounts;
        private readonly List<Event> _Events;
        private readonly List<Unknown28Header> _Unknown28s;

        public ScriptFile()
        {
            this._EventCounts = new List<ushort>();
            this._Events = new List<Event>();
            this._Unknown28s = new List<Unknown28Header>();
        }

        public Endian Endian { get; set; }
        public string AuthorName { get; set; }
        public string SourceName { get; set; }
        public string MaybeSourceVersion { get; set; }
        public List<ushort> EventCounts { get { return this._EventCounts; } }
        public List<Event> Events { get { return this._Events; } }
        public List<Unknown28Header> Unknown28s { get { return this._Unknown28s; } }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            var header = FileHeader.Read(input);
            var endian = header.Endian;

            input.Position = header.AuthorNameOffset;
            var authorName = input.ReadStringZ(SJIS);

            input.Position = header.SourceNameOffset;
            var sourceName = input.ReadStringZ(SJIS);

            input.Position = header.MaybeSourceVersionOffset;
            var maybeSourceVersion = input.ReadStringZ(SJIS);

            input.Position = header.EventCountTableOffset;
            var eventCountCount = input.ReadValueU16(endian);
            var eventCounts = new ushort[eventCountCount];
            for (int i = 0; i < eventCountCount; i++)
            {
                eventCounts[i] = input.ReadValueU16(endian);
            }

            input.Position = header.EventTableOffset;
            var eventCount = input.ReadValueS32(endian);
            var eventHeaders = new EventHeader[eventCount];
            for (int i = 0; i < eventCount; i++)
            {
                eventHeaders[i] = EventHeader.Read(input, endian);
            }

            input.Position = header.Unknown28Offset;
            var unknown28Count = input.ReadValueS32(endian);
            var unknown28s = new Unknown28Header[unknown28Count];
            for (int i = 0; i < unknown28Count; i++)
            {
                unknown28s[i] = Unknown28Header.Read(input, endian);
            }

            input.Position = header.Unknown2COffset;
            var unknown2CCount = input.ReadValueS32(endian);
            for (int i = 0; i < unknown2CCount; i++)
            {
                throw new NotImplementedException();
            }

            input.Position = header.Unknown30Offset;
            var unknown30Count = input.ReadValueS32(endian);
            var unknown30Offsets = new uint[unknown30Count];
            for (int i = 0; i < unknown30Count; i++)
            {
                unknown30Offsets[i] = input.ReadValueU32(endian);
            }

            var events = new Event[eventCount];
            for (int eventIndex = 0; eventIndex < eventCount; eventIndex++)
            {
                var eventHeader = eventHeaders[eventIndex];

                input.Position = header.StringTableOffset + eventHeader.NameOffset;
                var eventName = input.ReadStringZ(SJIS);

                input.Position = eventHeader.JumpTableOffset;
                var jumpCount = input.ReadValueS32(endian);
                var jumpOffsets = new uint[jumpCount];
                for (int i = 0; i < jumpCount; i++)
                {
                    jumpOffsets[i] = input.ReadValueU32(endian);
                }

                input.Position = eventHeader.FunctionTableOffset;
                var functionCount = input.ReadValueS32(endian);
                var functionHeaders = new FunctionHeader[functionCount];
                for (int i = 0; i < functionCount; i++)
                {
                    functionHeaders[i] = FunctionHeader.Read(input, endian);
                }

                input.Position = eventHeader.Unknown18Offset;
                var unknown18Count = input.ReadValueU16(endian);
                var unknown18s = new short[unknown18Count];
                for (int i = 0; i < unknown18Count; i++)
                {
                    unknown18s[i] = input.ReadValueS16(endian);
                }

                var codeOffsetsUnique = jumpOffsets
                    .Concat(functionHeaders.Select(fh => fh.CodeOffset))
                    .Distinct()
                    .OrderBy(v => v).ToArray();

                var codeOffsetsToIndices = new Dictionary<uint, int>();
                uint codeOffset = 0;
                var code = new Instruction[eventHeader.CodeCount];
                input.Position = eventHeader.CodeOffset;
                for (int i = 0; i < eventHeader.CodeCount; i++)
                {
                    if (codeOffsetsUnique.Contains(codeOffset) == true)
                    {
                        codeOffsetsToIndices.Add(codeOffset, i);
                    }

                    var opcode = (Opcode)input.ReadValueU8();
                    codeOffset++;

                    Instruction instruction;
                    if (opcode.HasArgument() == false)
                    {
                        instruction = new Instruction(opcode);
                    }
                    else
                    {
                        var argument = input.ReadValueS16(endian);
                        instruction = new Instruction(opcode, argument);
                        codeOffset += 2;
                    }
                    code[i] = instruction;
                }

                var codeSize = codeOffset;
                codeOffsetsToIndices.Add(codeSize, code.Length);

                var jumps = new int[jumpCount];
                for (int i = 0; i < jumpCount; i++)
                {
                    jumps[i] = codeOffsetsToIndices[jumpOffsets[i]];
                }

                var functions = new Function[functionCount];
                for (int functionIndex = 0; functionIndex < functionCount; functionIndex++)
                {
                    var functionHeader = functionHeaders[functionIndex];

                    input.Position = header.StringTableOffset + functionHeader.NameOffset;
                    var functionName = input.ReadStringZ(SJIS);

                    var nextFunctionOffset = functionIndex + 1 < functionCount
                        ? functionHeaders[functionIndex + 1].CodeOffset
                        : codeOffset;
                    var functionSize = nextFunctionOffset - functionHeader.CodeOffset;
                    if (functionSize < 1)
                    {
                        //throw new InvalidOperationException();
                    }

                    var bodyStart = codeOffsetsToIndices[functionHeader.CodeOffset];
                    var bodyEnd = codeOffsetsToIndices[nextFunctionOffset];

                    functions[functionIndex] = new Function()
                    {
                        Name = functionName,
                        BodyStart = bodyStart,
                        BodyEnd = bodyEnd,
                    };
                }

                var ev = new Event()
                {
                    Name = eventName,
                    TableIndex = eventHeader.TableIndex,
                    Unknown06 = eventHeader.Unknown06,
                    Unknown1C = eventHeader.Unknown1C,
                    Unknown20 = eventHeader.Unknown20,
                    Index = eventHeader.Index,
                    Unknown24 = eventHeader.Unknown24,
                    Unknown28 = eventHeader.Unknown28,
                    Unknown2C = eventHeader.Unknown2C,
                };
                ev.Code.AddRange(code);
                ev.Jumps.AddRange(jumps);
                ev.Functions.AddRange(functions);
                ev.Unknown18s.AddRange(unknown18s);
                events[eventIndex] = ev;
            }

            this.EventCounts.Clear();
            this.Events.Clear();
            this.Unknown28s.Clear();

            this.Endian = endian;
            this.AuthorName = authorName;
            this.SourceName = sourceName;
            this.MaybeSourceVersion = maybeSourceVersion;
            this.EventCounts.AddRange(eventCounts);
            this.Events.AddRange(events);
            this.Unknown28s.AddRange(unknown28s);
        }
    }
}
