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
using Gibbed.LetUsClingTogether.ScriptFormats;
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

            var opcodePadding = 1 + Enum.GetNames(typeof(Opcode)).Max(v => v.Length);

            var targetNames = new Dictionary<int, string>()
            {
                { 0, "Wait" },
                { 27, "Talk_LeftLower" },
                { 28, "Talk_LeftUpper" },
                { 29, "Talk_RightLower" },
                { 30, "Talk_RightUpper" },
                { 31, "Talk_RightCenter" },
                { 32, "Talk_LeftCenter" },
                { 34, "Message_FullScreen" },
                { 65, "Unit_SetSprite" },
            };

            var scriptFile = new ScriptFile();
            var scriptBytes = File.ReadAllBytes(inputPath);
            using (var input = new MemoryStream(scriptBytes, false))
            {
                scriptFile.Deserialize(input);
            }

            Console.WriteLine($"Author: {scriptFile.AuthorName}");
            Console.WriteLine($"Source Name: {scriptFile.SourceName}");
            Console.WriteLine($"Date: {scriptFile.Date}");

            if (scriptFile.ScriptCounts.Count > 0)
            {
                Console.Write("Script Counts: ");
                foreach (var scriptCount in scriptFile.ScriptCounts)
                {
                    Console.Write($" {scriptCount}");
                }
                Console.WriteLine();
            }

            foreach (var script in scriptFile.Scripts)
            {
                Console.WriteLine();

                Console.WriteLine($"script {script.Name}");

                Console.WriteLine($"  table index = {script.TableIndex}");
                Console.WriteLine($"  unknown06 = {script.Unknown06}");
                Console.WriteLine($"  unknown1C = {script.Unknown1C}");
                Console.WriteLine($"  unknown20 = {script.Unknown20}");
                Console.WriteLine($"  index = {script.Index}");
                Console.WriteLine($"  unknown24 = {script.Unknown24}");
                Console.WriteLine($"  unknown28 = {script.Unknown28}");
                Console.WriteLine($"  unknown2C = {script.Unknown2C}");

                if (script.Unknown18s.Count > 0)
                {
                    Console.Write("  unknown18s =");
                    foreach (var value in script.Unknown18s)
                    {
                        Console.Write($" {value}");
                    }
                    Console.WriteLine();
                }

                if (script.Jumps.Count > 0)
                {
                    Console.Write("  jump table:");
                    foreach (var jump in script.Jumps)
                    {
                        Console.Write($" {jump}");
                    }
                    Console.WriteLine();
                }

                foreach (var function in script.Functions)
                {
                    Console.WriteLine($"  function {function.Name} ({function.BodyStart})");

                    for (int bodyIndex = function.BodyStart; bodyIndex < function.BodyEnd; bodyIndex++)
                    {
                        var instruction = script.Code[bodyIndex];
                        var opcode = instruction.Opcode;

                        Console.Write("    ");
                        Console.Write($"@{bodyIndex:D4} ");
                        Console.Write(opcode.ToString().PadRight(opcodePadding));

                        if (opcode == Opcode.Call ||
                            opcode == Opcode.CallAct ||
                            opcode == Opcode.CallAndPopA ||
                            opcode == Opcode.CallActAndPopA)
                        {
                            var immediate = instruction.Immediate;
                            Console.Write($" {immediate}");
                            if (targetNames.TryGetValue(immediate, out var targetName) == true)
                            {
                                Console.Write($" ({targetName})");
                            }
                        }
                        else if (opcode == Opcode.PushIntFromTable)
                        {
                            var immediate = instruction.Immediate;
                            var value = scriptFile.IntTable[immediate];
                            Console.Write($" {value} (#{immediate})");
                        }
                        else if (opcode == Opcode.PushFloatFromTable)
                        {
                            var immediate = instruction.Immediate;
                            var value = scriptFile.FloatTable[immediate];
                            Console.Write($" {value} (#{immediate})");
                        }
                        else if (opcode.IsJump() == true)
                        {
                            var immediate = instruction.Immediate;
                            var index = script.Jumps[immediate];
                            Console.Write($" {immediate} => @{index:D4}");
                        }
                        else if (opcode.HasImmediate() == true)
                        {
                            Console.Write($" {instruction.Immediate}");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
