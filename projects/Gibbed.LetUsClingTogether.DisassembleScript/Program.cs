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
using Gibbed.LetUsClingTogether.FileFormats.Script;
using NDesk.Options;
using ScriptFile = Gibbed.LetUsClingTogether.FileFormats.ScriptFile;

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

            var eventNativeNames = new Dictionary<int, string>()
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

            var script = new ScriptFile();
            var scriptBytes = File.ReadAllBytes(inputPath);
            using (var input = new MemoryStream(scriptBytes, false))
            {
                script.Deserialize(input);
            }

            Console.WriteLine("Author: {0}", script.AuthorName);
            Console.WriteLine("Source Name: {0}", script.SourceName);
            Console.WriteLine("Source Version?: {0}", script.MaybeSourceVersion);

            if (script.EventCounts.Count > 0)
            {
                Console.Write("Event Counts: ");
                foreach (var eventCount in script.EventCounts)
                {
                    Console.Write(" {0}", eventCount);
                }
                Console.WriteLine();
            }

            foreach (var ev in script.Events)
            {
                Console.WriteLine();

                Console.WriteLine("event {0}", ev.Name);

                Console.WriteLine("  evtable index = {0}", ev.TableIndex);
                Console.WriteLine("  unknown06 = {0}", ev.Unknown06);
                Console.WriteLine("  unknown1C = {0}", ev.Unknown1C);
                Console.WriteLine("  unknown20 = {0}", ev.Unknown20);
                Console.WriteLine("  evindex = {0}", ev.Index);
                Console.WriteLine("  unknown24 = {0}", ev.Unknown24);
                Console.WriteLine("  unknown28 = {0}", ev.Unknown28);
                Console.WriteLine("  unknown2C = {0}", ev.Unknown2C);

                if (ev.Jumps.Count > 0)
                {
                    Console.Write("  jump table:");
                    foreach (var jump in ev.Jumps)
                    {
                        Console.Write(" {0}", jump);
                    }
                    Console.WriteLine();
                }

                foreach (var function in ev.Functions)
                {
                    Console.WriteLine("  function {0}", function.Name);

                    for (int bodyIndex = function.BodyStart; bodyIndex < function.BodyEnd; bodyIndex++)
                    {
                        var instruction = ev.Code[bodyIndex];
                        var opcode = instruction.Opcode;

                        Console.Write("    ");

                        Console.Write("@{0:D4} ", bodyIndex);

                        Console.Write("{0}", opcode.ToString().PadRight(28));

                        if (opcode == Opcode.CallNative ||
                            opcode == Opcode.UnknownCallNative ||
                            opcode == Opcode.CallNativeWithBar)
                        {
                            var argument = instruction.Argument;
                            Console.Write(" {0}", argument);
                            if (eventNativeNames.TryGetValue(argument, out var eventNativeName) == true)
                            {
                                Console.Write(" ({0})", eventNativeName);
                            }
                        }
                        else if (opcode.IsJump() == true)
                        {
                            var argument = instruction.Argument;
                            var index = ev.Jumps[argument];
                            Console.Write(" {0} => @{1:D4}", argument, index);
                        }
                        else if (opcode.HasArgument() == true)
                        {
                            Console.Write(" {0}", instruction.Argument);
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
