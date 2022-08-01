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
using System.Text;
using Gibbed.IO;
using NDesk.Options;

namespace Gibbed.LetUsClingTogether.ExportSheet
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_scd [output_wav]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPath = Path.GetFullPath(extras[0]);
            var outputPath = extras.Count > 1
                ? Path.GetFullPath(extras[1])
                : Path.ChangeExtension(inputPath, null) + "_export";
            Export(inputPath, outputPath);
        }

        private static void Export(string inputPath, string outputBasePath)
        {
            using var input = File.OpenRead(inputPath);

            var magic1 = input.ReadString(4, false, Encoding.ASCII);
            if (magic1 != "SEDB")
            {
                throw new FormatException();
            }

            var magic2 = input.ReadString(4, false, Encoding.ASCII);
            if (magic2 != "SSCF")
            {
                throw new FormatException();
            }

            var version = input.ReadValueU32(Endian.Little);
            if (version != 3 && version.Swap() != 3)
            {
                throw new FormatException();
            }
            var endian = version == 3 ? Endian.Little : Endian.Big;

            var unknown0C = input.ReadValueU8();
            var unknown0D = input.ReadValueU8();
            var fileHeaderSize = input.ReadValueU16(endian);
            if ((unknown0C != 0 && unknown0C != 1) ||
                unknown0D != 4 ||
                fileHeaderSize != 0x30)
            {
                throw new FormatException();
            }

            var totalSize = input.ReadValueS64(endian);

            {
                var zero18 = input.ReadValueU32(endian);
                var zero1C = input.ReadValueU32(endian);
                var zero20 = input.ReadValueU32(endian);
                var zero24 = input.ReadValueU32(endian);
                var zero28 = input.ReadValueU32(endian);
                var zero2C = input.ReadValueU32(endian);
                if (zero18 != 0 || zero1C != 0 ||
                    zero20 != 0 || zero24 != 0 || zero28 != 0 || zero2C != 0)
                {
                    throw new FormatException();
                }
            }

            input.Position = fileHeaderSize;
            var unknown30Count = input.ReadValueU16(endian); // 00
            var unknown32Count = input.ReadValueU16(endian); // 02
            var soundCount = input.ReadValueU16(endian); // 04
            var unknown36Count = input.ReadValueU16(endian); // 06
            var tagTableTableOffset = input.ReadValueU32(endian); // 08
            var soundTableOffset = input.ReadValueU32(endian); // 0C
            var unknown40Offset = input.ReadValueU32(endian); // 10
            var zero44 = input.ReadValueU32(endian); // 14
            var unknown48Offset = input.ReadValueU32(endian); // 18
            var zero4C = input.ReadValueU32(endian); // 1C

            if (zero44 != 0 || zero4C != 0)
            {
                throw new FormatException();
            }

            var unknownOffsets0 = new uint[unknown30Count];
            for (int i = 0; i < unknown30Count; i++)
            {
                unknownOffsets0[i] = input.ReadValueU32(endian);
            }

            input.Position = tagTableTableOffset;
            var tagTableOffsets = new uint[unknown32Count];
            for (int i = 0; i < unknown32Count; i++)
            {
                tagTableOffsets[i] = input.ReadValueU32(endian);
            }

            input.Position = soundTableOffset;
            var soundOffsets = new uint[soundCount];
            for (int i = 0; i < soundCount; i++)
            {
                soundOffsets[i] = input.ReadValueU32(endian);
            }

            input.Position = unknown40Offset;
            var unknownOffsets3 = new uint[unknown30Count];
            for (int i = 0; i < unknown30Count; i++)
            {
                unknownOffsets3[i] = input.ReadValueU32(endian);
            }

            var tagLists = new List<(ushort id, object value)>[unknown32Count];
            for (int i = 0; i < unknown32Count; i++)
            {
                var tagOffsetStart = tagTableOffsets[i];
                var tagOffsetEnd = i + 1 < unknown32Count
                    ? tagTableOffsets[i + 1]
                    : unknownOffsets3[0];
                input.Position = tagOffsetStart;
                var tags = new List<(ushort id, object value)>();
                while (true)
                {
                    if (input.Position + 2 > tagOffsetEnd)
                    {
                        throw new InvalidOperationException();
                    }

                    var tagOffsetCurrent = input.Position;

                    var tagId = input.ReadValueU16(endian);
                    if (tagId == 0)
                    {
                        break;
                    }

                    object tagValue;
                    switch (tagId)
                    {
                        case 1:
                        case 2:
                        case 6:
                        case 33:
                        {
                            var tagValue0 = input.ReadValueF32(endian);
                            var tagValue1 = input.ReadValueU32(endian);
                            tagValue = (tagValue0, tagValue1);
                            break;
                        }

                        case 3:
                        {
                            // data offset?
                            tagValue = input.ReadValueU32(endian);
                            break;
                        }

                        case 4:
                        {
                            var tagValue0 = input.ReadValueU32(endian);
                            var tagValue1 = input.ReadValueF32(endian);
                            var tagValue2 = input.ReadValueU32(endian);
                            tagValue = (tagValue0, tagValue1, tagValue2);
                            break;
                        }

                        case 5:
                        {
                            tagValue = input.ReadValueU32(endian);
                            break;
                        }

                        case 7:
                        {
                            // something special about 7
                            tagValue = null;
                            break;
                        }

                        case 8:
                        case 9:
                        case 10:
                        {
                            var tagValue0 = input.ReadValueF32(endian);
                            var tagValue1 = input.ReadValueF32(endian);
                            tagValue = (tagValue0, tagValue1);
                            break;
                        }

                        case 12:
                        case 36:
                        {
                            tagValue = null;
                            // end?
                            break;
                        }

                        case 13:
                        {
                            var tagValue0 = input.ReadValueF32(endian);
                            var tagValue1 = input.ReadValueF32(endian);
                            tagValue = (tagValue0, tagValue1);
                            break;
                        }

                        case 14:
                        {
                            tagValue = null;
                            break;
                        }

                        case 15:
                        {
                            // condition?
                            var tagValue0 = input.ReadValueU16(endian);
                            var tagValue1 = input.ReadValueU16(endian);
                            tagValue = (tagValue0, tagValue1);
                            break;
                        }

                        case 16:
                        {
                            tagValue = null;
                            break;
                        }

                        case 17:
                        {
                            tagValue = input.ReadValueU32(endian);
                            break;
                        }

                        case 18:
                        {
                            var tagValue0 = input.ReadValueU32(endian);
                            var tagValue1 = input.ReadValueU32(endian);
                            tagValue = (tagValue0, tagValue1);
                            break;
                        }

                        case 19:
                        {
                            tagValue = input.ReadValueU32(endian);
                            break;
                        }

                        case 20:
                        {
                            var tagValue0 = input.ReadValueU32(endian);
                            var tagValue1 = input.ReadValueU32(endian);
                            tagValue = (tagValue0, tagValue1);
                            break;
                        }

                        case 21:
                        {
                            tagValue = input.ReadValueU32(endian);
                            break;
                        }

                        case 22:
                        {
                            var tagValue0 = input.ReadValueU32(endian);
                            var tagValue1 = input.ReadValueU32(endian);
                            tagValue = (tagValue0, tagValue1);
                            break;
                        }

                        case 23:
                        {
                            tagValue = input.ReadValueU32(endian);
                            break;
                        }

                        case 24:
                        case 25:
                        case 26:
                        case 27:
                        case 28:
                        {
                            var tagValue0 = input.ReadValueU32(endian);
                            var tagValue1 = input.ReadValueU32(endian);
                            tagValue = (tagValue0, tagValue1);
                            break;
                        }

                        case 29:
                        {
                            tagValue = input.ReadValueU32(endian);
                            break;
                        }

                        case 30:
                        case 31:
                        {
                            var tagValue0 = input.ReadValueU32(endian);
                            var tagValue1 = input.ReadValueU32(endian);
                            tagValue = (tagValue0, tagValue1);
                            break;
                        }

                        case 32:
                        {
                            tagValue = input.ReadValueU32(endian);
                            break;
                        }

                        case 34:
                        case 35:
                        case 37:
                        {
                            var tagValue0 = input.ReadValueU32(endian);
                            var tagValue1 = input.ReadValueU32(endian);
                            tagValue = (tagValue0, tagValue1);
                            break;
                        }

                        case 38:
                        {
                            // defaulted?
                            tagValue = null;
                            break;
                        }

                        case 11:
                        case 45:
                        case 46:
                        case 47:
                        {
                            tagValue = null;
                            break;
                        }

                        default:
                        {
                            throw new NotSupportedException();
                        }
                    }

                    tags.Add((tagId, tagValue));
                }

                if (input.Position > tagOffsetEnd)
                {
                    throw new FormatException();
                }

                tagLists[i] = tags;
            }

            for (int i = 0; i < soundCount; i++)
            {
                var soundOffset = soundOffsets[i];

                input.Position = soundOffset;
                var soundDataSize = input.ReadValueU32(endian);
                var soundChannelCount = input.ReadValueU32(endian);
                var soundFrequency = input.ReadValueU32(endian);
                var soundType = input.ReadValueS32(endian);
                var soundLoopStart = input.ReadValueU32(endian);
                var soundLoopEnd = input.ReadValueU32(endian);
                var soundDataOffset = input.ReadValueU32(endian);
                var soundUnknown = input.ReadValueU32(endian);

                if (soundType == -1)
                {
                    if (soundDataSize != 0)
                    {
                        throw new FormatException();
                    }

                    continue;
                }

                if (soundChannelCount != 1 && soundChannelCount != 2)
                {
                    throw new FormatException();
                }

                if (soundUnknown != 0)
                {
                    throw new FormatException();
                }

                if (soundDataOffset != 0)
                {
                    input.Position += soundDataOffset;
                }

                if (soundType == 3) // Sony VAG
                {
                    var outputPath = Path.Combine(outputBasePath, $"sound_{i}.vag");
                    var outputParentPath = Path.GetDirectoryName(outputPath);
                    if (string.IsNullOrEmpty(outputParentPath) == false)
                    {
                        Directory.CreateDirectory(outputParentPath);
                    }
                    using (var output = File.Create(outputPath))
                    {
                        // slap on a Sony VAG header
                        output.WriteValueU32(0x56414770, Endian.Big);
                        output.WriteValueU32(0x00000006, Endian.Big);
                        output.WriteValueU32(0x00000000, Endian.Big);
                        output.WriteValueU32(soundDataSize, Endian.Big);
                        output.WriteValueU32(soundFrequency, Endian.Big);
                        output.WriteValueU32(0x00000000, Endian.Big);
                        output.WriteValueU32(0x00000000, Endian.Big);
                        output.WriteValueU16(0x0000, Endian.Big);
                        output.WriteValueU8((byte)soundChannelCount);
                        output.WriteValueU8(0);
                        output.WriteValueU64(0, Endian.Big); // name
                        output.WriteValueU64(0, Endian.Big); // name
                        output.WriteFromStream(input, soundDataSize);
                    }
                }
                else if (soundType == 14) // .wav
                {
                    var outputPath = Path.Combine(outputBasePath, $"sound_{i}.wav");
                    var outputParentPath = Path.GetDirectoryName(outputPath);
                    if (string.IsNullOrEmpty(outputParentPath) == false)
                    {
                        Directory.CreateDirectory(outputParentPath);
                    }
                    using (var output = File.Create(outputPath))
                    {
                        output.WriteFromStream(input, soundDataSize);
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
