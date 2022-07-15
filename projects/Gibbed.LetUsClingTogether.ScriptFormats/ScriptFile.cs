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
        private readonly List<Unknown28Header> _Unknown28s;

        public ScriptFile()
        {
            this._ScriptCounts = new List<ushort>();
            this._Scripts = new List<Script>();
            this._Unknown28s = new List<Unknown28Header>();
        }

        public Endian Endian { get; set; }
        public string AuthorName { get; set; }
        public string SourceName { get; set; }
        public string MaybeSourceVersion { get; set; }
        public List<ushort> ScriptCounts { get { return this._ScriptCounts; } }
        public List<Script> Scripts { get { return this._Scripts; } }
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
            var scriptCountCount = input.ReadValueU16(endian);
            var scriptCounts = new ushort[scriptCountCount];
            for (int i = 0; i < scriptCountCount; i++)
            {
                scriptCounts[i] = input.ReadValueU16(endian);
            }

            input.Position = header.EventTableOffset;
            var scriptCount = input.ReadValueS32(endian);
            var scriptHeaders = new ScriptHeader[scriptCount];
            for (int i = 0; i < scriptCount; i++)
            {
                scriptHeaders[i] = ScriptHeader.Read(input, endian);
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

                input.Position = scriptHeader.Unknown18Offset;
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

                var script = new Script()
                {
                    Name = scriptName,
                    TableIndex = scriptHeader.TableIndex,
                    Unknown06 = scriptHeader.Unknown06,
                    Unknown1C = scriptHeader.Unknown1C,
                    Unknown20 = scriptHeader.Unknown20,
                    Index = scriptHeader.Index,
                    Unknown24 = scriptHeader.Unknown24,
                    Unknown28 = scriptHeader.Unknown28,
                    Unknown2C = scriptHeader.Unknown2C,
                };
                script.Code.AddRange(code);
                script.Jumps.AddRange(jumps);
                script.Functions.AddRange(functions);
                script.Unknown18s.AddRange(unknown18s);
                scripts[scriptIndex] = script;
            }

            this.ScriptCounts.Clear();
            this.Scripts.Clear();
            this.Unknown28s.Clear();

            this.Endian = endian;
            this.AuthorName = authorName;
            this.SourceName = sourceName;
            this.MaybeSourceVersion = maybeSourceVersion;
            this.ScriptCounts.AddRange(scriptCounts);
            this.Scripts.AddRange(scripts);
            this.Unknown28s.AddRange(unknown28s);
        }
    }
}
