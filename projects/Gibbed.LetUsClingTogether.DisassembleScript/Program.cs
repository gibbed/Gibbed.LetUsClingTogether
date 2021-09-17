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
using System.Text;
using Gibbed.IO;
using Gibbed.LetUsClingTogether.FileFormats.Script;
using NDesk.Options;

namespace Gibbed.LetUsClingTogether.DisassembleScript
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            bool verbose = false;
            bool showHelp = false;

            var options = new OptionSet()
            {
                { "v|verbose", "be verbose", v => verbose = v != null },
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extras;
            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count < 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_script", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPath = extras[0];

            // temporary, until I do real file output
            Console.OutputEncoding = Encoding.UTF8;

            var sjis = Encoding.GetEncoding(932);

            var eventNativeMethodNames = new Dictionary<int, string>()
            {
                { 0, "Delay" },
                { 27, "Talk_LeftLower" },
                { 28, "Talk_LeftUpper" },
                { 29, "Talk_RightLower" },
                { 30, "Talk_RightUpper" },
                { 31, "Talk_RightCenter" },
                { 32, "Talk_LeftCenter" },
                { 34, "Message_FullScreen" },
                { 65, "Unit_SetSprite" },
            };

            var scriptBytes = File.ReadAllBytes(inputPath);
            using (var input = new MemoryStream(scriptBytes, false))
            {
                const uint signature = 0x8000000C;

                var magic = input.ReadValueU32(Endian.Little);
                if (magic != signature && magic.Swap() != signature)
                {
                    throw new FormatException();
                }
                var endian = magic == signature ? Endian.Little : Endian.Big;

                var totalSize = input.ReadValueU32(endian);
                var authorNameOffset = input.ReadValueU32(endian);
                var sourceNameOffset = input.ReadValueU32(endian);
                var versionOffset = input.ReadValueU32(endian);
                var unknown14Offset = input.ReadValueU32(endian);
                var functionTableOffset = input.ReadValueU32(endian);
                var unknown1C = input.ReadValueU32(endian);
                var unknown20 = input.ReadValueU32(endian);
                var unknown24 = input.ReadValueU32(endian);
                var unknown28Offset = input.ReadValueU32(endian);
                var unknown2COffset = input.ReadValueU32(endian);
                var unknown30Offset = input.ReadValueU32(endian);
                var unknown34 = input.ReadValueU32(endian);
                var unknown38 = input.ReadValueU32(endian);
                var unknown3C = input.ReadValueU32(endian);
                var unknown40 = input.ReadValueU32(endian);
                var unknown44 = input.ReadValueU32(endian);
                var unknown48Offset = input.ReadValueU32(endian);
                var stringTableOffset = input.ReadValueU32(endian);
                var unknown50 = input.ReadValueS16(endian);
                var unknown52 = input.ReadValueS16(endian);
                var unknown54Offset = input.ReadValueU32(endian);
                var unknown58 = input.ReadValueU32(endian);
                var unknown5COffset = input.ReadValueU32(endian);
                var unknown60 = input.ReadValueU32(endian);

                input.Position = authorNameOffset;
                var authorName = input.ReadStringZ(sjis);

                input.Position = sourceNameOffset;
                var sourceName = input.ReadStringZ(sjis);

                input.Position = versionOffset;
                var version = input.ReadStringZ(sjis);

                input.Position = functionTableOffset;
                var functionCount = input.ReadValueS32(endian);

                Console.WriteLine("Author: {0}", authorName);
                Console.WriteLine("Source: {0}", sourceName);
                Console.WriteLine("Version?: {0}", version);

                var rawFunctionInfos = new RawFunctionInfo[functionCount];
                for (int i = 0; i < functionCount; i++)
                {
                    RawFunctionInfo rawFunctionInfo;
                    rawFunctionInfo.NameOffset = input.ReadValueU32(endian);
                    rawFunctionInfo.Unknown04 = input.ReadValueU32(endian);
                    rawFunctionInfo.CodeOffset = input.ReadValueU32(endian);
                    rawFunctionInfo.CodeCount = input.ReadValueS32(endian);
                    rawFunctionInfo.Unknown10 = input.ReadValueU32(endian);
                    rawFunctionInfo.Unknown14 = input.ReadValueU32(endian);
                    rawFunctionInfo.Unknown18 = input.ReadValueU32(endian);
                    rawFunctionInfo.Unknown1C = input.ReadValueU32(endian);
                    rawFunctionInfo.Unknown20 = input.ReadValueU32(endian);
                    rawFunctionInfo.Unknown24 = input.ReadValueU32(endian);
                    rawFunctionInfo.Unknown28 = input.ReadValueU32(endian);
                    rawFunctionInfo.Unknown2C = input.ReadValueU32(endian);
                    rawFunctionInfos[i] = rawFunctionInfo;
                }

                foreach (var rawFunctionInfo in rawFunctionInfos)
                {
                    Console.WriteLine();

                    input.Position = stringTableOffset + rawFunctionInfo.NameOffset;
                    var name = input.ReadStringZ(sjis);

                    Console.WriteLine("function {0}", name);

                    input.Position = rawFunctionInfo.CodeOffset;
                    for (int i = 0; i < rawFunctionInfo.CodeCount; i++)
                    {
                        var instructionPosition = input.Position;

                        var opcode = (Opcode)input.ReadValueU8();
                        var extraBytes = input.ReadBytes(opcode.GetSize() - 1);

                        Console.Write("  ");

                        Console.Write("@{0:X} : ", instructionPosition);
                        Console.Write("@{0:X} : ", instructionPosition - rawFunctionInfo.CodeOffset);

                        Console.Write("{0}", opcode.ToString().PadRight(24));

                        if (opcode == Opcode.CallNative)
                        {
                            if (extraBytes.Length != 2)
                            {
                                throw new FormatException();
                            }

                            var arg = BitConverter.ToInt16(extraBytes, 0);
                            Console.Write(" {0}", arg);

                            if (eventNativeMethodNames.TryGetValue(arg, out var nativeMethodName) == true)
                            {
                                Console.Write(" ({0})", nativeMethodName);
                            }
                        }
                        else if (opcode >= Opcode.Undefined71)
                        {
                            if (extraBytes.Length != 2)
                            {
                                throw new FormatException();
                            }
                            var arg = BitConverter.ToInt16(extraBytes, 0);
                            Console.Write(" {0}", arg);
                        }
                        else if (extraBytes.Length > 0)
                        {
                            Console.Write(" {0}", BitConverter.ToString(extraBytes));
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
    }

    internal struct RawFunctionInfo
    {
        public uint NameOffset;
        public uint Unknown04;
        public uint CodeOffset;
        public int CodeCount;
        public uint Unknown10;
        public uint Unknown14;
        public uint Unknown18;
        public uint Unknown1C;
        public uint Unknown20;
        public uint Unknown24;
        public uint Unknown28;
        public uint Unknown2C;
    }
}
