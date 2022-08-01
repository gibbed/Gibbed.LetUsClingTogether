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
using System.Linq;
using System.Text;
using Gibbed.IO;

namespace Gibbed.LetUsClingTogether.ScriptFormats
{
    public class ScriptFile
    {
        private static readonly Encoding SJIS;

        static ScriptFile()
        {
            SJIS = Encoding.GetEncoding(932);
        }

        private readonly List<ushort> _ScriptCounts;
        private readonly List<Script> _Scripts;
        private readonly List<int> _IntTable;
        private readonly List<float> _FloatTable;
        private readonly List<Variable> _Variables;
        private readonly List<RequestHeader[]> _RequestTables;

        public ScriptFile()
        {
            this._ScriptCounts = new();
            this._Scripts = new();
            this._IntTable = new();
            this._FloatTable = new();
            this._Variables = new();
            this._RequestTables = new();
        }

        public Endian Endian { get; set; }
        public string AuthorName { get; set; }
        public string SourceName { get; set; }
        public string Date { get; set; }
        public List<ushort> ScriptCounts { get { return this._ScriptCounts; } }
        public List<Script> Scripts { get { return this._Scripts; } }
        public List<int> IntTable {  get { return this._IntTable; } }
        public List<float> FloatTable { get { return this._FloatTable; } }
        public List<Variable> Variables { get { return this._Variables; } }
        public List<RequestHeader[]> RequestTables { get { return this._RequestTables; } }

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            var header = ScriptFileHeader.Read(input);
            var endian = header.Endian;

            if (header.Unknown5COffset == 0)
            {
                throw new FormatException();
            }
            input.Position = header.Unknown5COffset;
            var test = input.ReadValueU32(endian);
            if (test != 1)
            {
                throw new FormatException();
            }
            var test2 = input.ReadValueU32(endian);
            if (test2 != 0)
            {
                throw new FormatException();
            }

            input.Position = header.AuthorNameOffset;
            var authorName = input.ReadStringZ(SJIS);

            input.Position = header.SourceNameOffset;
            var sourceName = input.ReadStringZ(SJIS);

            input.Position = header.DateOffset;
            var date = input.ReadStringZ(SJIS);

            input.Position = header.ScriptCountTableOffset;
            var scriptCountCount = input.ReadValueU16(endian);
            var scriptCounts = new ushort[scriptCountCount];
            for (int i = 0; i < scriptCountCount; i++)
            {
                scriptCounts[i] = input.ReadValueU16(endian);
            }

            input.Position = header.ScriptTableOffset;
            var scriptCount = input.ReadValueS32(endian);
            var scriptHeaders = new ScriptHeader[scriptCount];
            for (int i = 0; i < scriptCount; i++)
            {
                scriptHeaders[i] = ScriptHeader.Read(input, endian);
            }

            input.Position = header.IntTableOffset;
            var intTableCount = input.ReadValueS32(endian);
            var intTable = new int[intTableCount];
            for (int i = 0; i < intTableCount; i++)
            {
                intTable[i] = input.ReadValueS32(endian);
            }

            input.Position = header.FloatTableOffset;
            var floatTableCount = input.ReadValueS32(endian);
            var floatTable = new float[floatTableCount];
            for (int i = 0; i < floatTableCount; i++)
            {
                floatTable[i] = input.ReadValueF32(endian);
            }

            input.Position = header.VariableTableOffset;
            var variableCount = input.ReadValueS32(endian);
            var variableHeaders = new VariableHeader[variableCount];
            for (int i = 0; i < variableCount; i++)
            {
                variableHeaders[i] = VariableHeader.Read(input, endian);
            }

            var variables = new Variable[variableCount];
            for (int i = 0; i < variableCount; i++)
            {
                var variableHeader = variableHeaders[i];
                Variable variable;
                if (variableHeader.Flags.Scope != VariableScope.Array)
                {
                    variable.Flags = variableHeader.Flags;
                    variable.ScopeIndex = variableHeader.Unknown;
                    variable.ArrayRank = 0;
                    variable.ArrayLengths = null;
                    variables[i] = variable;
                }
                else
                {
                    if (variableHeader.Flags.Type != VariableType.Byte)
                    {
                        throw new NotSupportedException();
                    }
                    input.Position = variableHeader.Flags.Offset;
                    var variableArrayHeader = VariableArrayHeader.Read(input, endian);
                    variable.Flags = variableArrayHeader.Flags;
                    variable.ScopeIndex = variableHeader.Unknown;
                    variable.ArrayRank = variableArrayHeader.Rank;
                    variable.ArrayLengths = new int[2];
                    variable.ArrayLengths[0] = variableArrayHeader.Lengths[0];
                    variable.ArrayLengths[1] = variableArrayHeader.Lengths[1];
                }
                variables[i] = variable;
            }

            input.Position = header.Unknown2COffset;
            var unknown2CCount = input.ReadValueS32(endian);
            for (int i = 0; i < unknown2CCount; i++)
            {
                throw new NotImplementedException();
            }

            input.Position = header.RequestTablesOffset;
            var requestTablesCount = input.ReadValueS32(endian);
            var requestTableOffsets = new uint[requestTablesCount];
            for (int i = 0; i < requestTablesCount; i++)
            {
                requestTableOffsets[i] = input.ReadValueU32(endian);
            }

            var requestTables = new RequestHeader[requestTablesCount][];
            for (int i = 0; i < requestTablesCount; i++)
            {
                input.Position = requestTableOffsets[i];
                var requestCount = input.ReadValueU32(endian);
                var requestHeaders = new RequestHeader[requestCount];
                for (int j = 0; j < requestCount; j++)
                {
                    requestHeaders[j] = RequestHeader.Read(input, endian);
                }
                requestTables[i] = requestHeaders;
            }

            var scripts = new Script[scriptCount];
            for (int scriptIndex = 0; scriptIndex < scriptCount; scriptIndex++)
            {
                var scriptHeader = scriptHeaders[scriptIndex];

                input.Position = header.StringTableOffset + scriptHeader.NameOffset;
                var scriptName = input.ReadStringZ(SJIS);

                input.Position = scriptHeader.JumpTableOffset;
                var jumpCount = input.ReadValueS32(endian);
                var jumpOffsets = new uint[jumpCount];
                for (int i = 0; i < jumpCount; i++)
                {
                    jumpOffsets[i] = input.ReadValueU32(endian);
                }

                input.Position = scriptHeader.FunctionTableOffset;
                var functionCount = input.ReadValueS32(endian);
                var functionHeaders = new FunctionHeader[functionCount];
                for (int i = 0; i < functionCount; i++)
                {
                    functionHeaders[i] = FunctionHeader.Read(input, endian);
                }

                input.Position = scriptHeader.EventTableOffset;
                var eventFunctionIndexCount = input.ReadValueU16(endian);
                var eventFunctionIndices = new short[eventFunctionIndexCount];
                for (int i = 0; i < eventFunctionIndexCount; i++)
                {
                    eventFunctionIndices[i] = input.ReadValueS16(endian);
                }

                var codeOffsetsUnique = jumpOffsets
                    .Concat(functionHeaders.Select(fh => fh.CodeOffset))
                    .Distinct()
                    .OrderBy(v => v)
                    .ToArray();

                Dictionary<uint, int> codeOffsetsToIndices = new();
                uint codeOffset = 0;
                var code = new Instruction[scriptHeader.CodeCount];
                input.Position = scriptHeader.CodeOffset;
                for (int i = 0; i < scriptHeader.CodeCount; i++)
                {
                    if (codeOffsetsUnique.Contains(codeOffset) == true)
                    {
                        codeOffsetsToIndices.Add(codeOffset, i);
                    }

                    var opcode = (Opcode)input.ReadValueU8();
                    codeOffset++;

                    Instruction instruction;
                    if (opcode.HasImmediate() == false)
                    {
                        instruction = new Instruction(opcode);
                    }
                    else
                    {
                        var immediate = input.ReadValueS16(endian);
                        instruction = new Instruction(opcode, immediate);
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

                for (int i = 0; i < eventFunctionIndexCount; i++)
                {
                    var functionIndex = eventFunctionIndices[i];
                    if (functionIndex == -1)
                    {
                        continue;
                    }

                    functions[functionIndex].Event = i;
                }

                var script = new Script()
                {
                    Name = scriptName,
                    TableIndex = scriptHeader.TableIndex,
                    Unknown06 = scriptHeader.Unknown06,
                    Unknown1C = scriptHeader.FrameOffset,
                    Unknown20 = scriptHeader.Unknown20,
                    Index = scriptHeader.Index,
                    Unknown24 = scriptHeader.Unknown24,
                    Unknown28 = scriptHeader.Unknown28,
                    Unknown2C = scriptHeader.Unknown2C,
                };
                script.Code.AddRange(code);
                script.Jumps.AddRange(jumps);
                script.Functions.AddRange(functions);
                scripts[scriptIndex] = script;
            }

            this.ScriptCounts.Clear();
            this.Scripts.Clear();
            this.IntTable.Clear();
            this.FloatTable.Clear();
            this.Variables.Clear();

            this.Endian = endian;
            this.AuthorName = authorName;
            this.SourceName = sourceName;
            this.Date = date;
            this.ScriptCounts.AddRange(scriptCounts);
            this.Scripts.AddRange(scripts);
            this.IntTable.AddRange(intTable);
            this.FloatTable.AddRange(floatTable);
            this.Variables.AddRange(variables);
            this.RequestTables.AddRange(requestTables);
        }
    }
}
