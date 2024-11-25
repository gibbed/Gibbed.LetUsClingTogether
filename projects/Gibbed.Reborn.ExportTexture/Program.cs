/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
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
using Gibbed.IO;
using Gibbed.Reborn.FileFormats;
using Gibbed.TacticsOgre.FileFormats;
using NDesk.Options;

namespace Gibbed.Reborn.ExportTexture
{
    public class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            bool verbose = false;
            bool showHelp = false;

            OptionSet options = new()
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_tex+", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPaths = new List<string>();
            foreach (var inputPath in extras)
            {
                if (Directory.Exists(inputPath) == true)
                {
                    inputPaths.AddRange(Directory.EnumerateFiles(inputPath, "*.btx", SearchOption.AllDirectories));
                    inputPaths.AddRange(Directory.EnumerateFiles(inputPath, "*.tex", SearchOption.AllDirectories));
                    inputPaths.AddRange(Directory.EnumerateFiles(inputPath, "*.fnt", SearchOption.AllDirectories));
                }
                else
                {
                    inputPaths.Add(inputPath);
                }
            }

            foreach (var inputPath in inputPaths.OrderBy(v => v))
            {
                string outputPath = Path.ChangeExtension(inputPath, ".dds");
                if (verbose == true)
                {
                    Console.WriteLine(inputPath);
                }
                Export(inputPath, outputPath);
            }
        }

        private static void Export(string inputPath, string outputPath)
        {
            var inputBytes = File.ReadAllBytes(inputPath);

            if (BitConverter.ToUInt32(inputBytes, 0) == RLE.Signature)
            {
                inputBytes = RLE.Decompress(inputBytes, 0, inputBytes.Length);
            }

            TextureFile texture = new();
            using (MemoryStream input = new(inputBytes, false))
            {
                texture.Deserialize(input);
            }

            using (var output = File.Create(outputPath))
            {
                const Endian endian = Endian.Little;
                WriteDDSHeader(texture, output, endian);

                // TODO(gibbed): requires verification
                if (texture.Unknown11 == 3)
                {
                    output.WriteBytes(DeswizzleSwitch(texture));
                }
                else
                {
                    output.WriteBytes(texture.DataBytes);
                }
            }
        }

        private static byte[] DeswizzleSwitch(TextureFile texture)
        {
            var blockHeightMip0 = TegraSwizzle.BlockHeightMip0(texture.Height);
            var blockDim = texture.Format.IsCompressed() == false
                ? TegraSwizzle.BlockDim.Uncompressed
                : TegraSwizzle.BlockDim.Block4x4;
            var bytesPerBlock = texture.Format.GetBytesPerBlock();
            var swizzledMipSize = TegraSwizzle.SwizzledSurfaceSize(
                texture.Width, texture.Height, 1,
                blockDim,
                blockHeightMip0,
                (uint)bytesPerBlock,
                1, 1);
            if (texture.DataBytes.Length != swizzledMipSize)
            {
                throw new InvalidOperationException();
            }
            var deswizzledMipSize = TegraSwizzle.DeswizzledSurfaceSize(
                texture.Width, texture.Height, 1,
                blockDim,
                (uint)bytesPerBlock,
                1, 1);
            if (deswizzledMipSize != swizzledMipSize)
            {
                throw new FormatException();
            }
            var deswizzledBytes = new byte[(int)deswizzledMipSize];
            TegraSwizzle.DeswizzleSurface(
                texture.Width, texture.Height, 1,
                texture.DataBytes, 0, texture.DataBytes.Length,
                deswizzledBytes, 0, deswizzledBytes.Length,
                blockDim,
                blockHeightMip0,
                (uint)bytesPerBlock,
                1, 1);
            return deswizzledBytes;
        }

        public static void WriteDDSHeader(TextureFile texture, FileStream output, Endian endian)
        {
            output.WriteValueU32(0x20534444, endian); // 'DDS '
            output.WriteValueU32(124, endian); // size
            output.WriteValueU32(0x00001007, endian); // flags
            output.WriteValueS32(texture.Height, endian);
            output.WriteValueS32(texture.Width, endian);
            output.WriteValueU32(0, endian); // pitch
            output.WriteValueU32(1, endian); // depth
            output.WriteValueU32(1, endian); // mipmaps
            for (int i = 0; i < 11; i++)
            {
                output.WriteValueU32(0, endian); // reserved
            }
            WriteDDSPixelFormat(texture, output, endian);
            output.WriteValueU32(8 | 0x1000 | 0x400000, endian); // surface flags
            output.WriteValueU32(0, endian); // cubemap flags
            for (int i = 0; i < 3; i++)
            {
                output.WriteValueU32(0, endian); // reserved
            }

            if (IsDX10(texture.Format) == true)
            {
                output.WriteValueU32(TranslateDX10Format(texture.Format), endian);
                output.WriteValueU32(3, endian);
                output.WriteValueU32(0, endian);
                output.WriteValueU32(1, endian);
                output.WriteValueU32(0, endian);
            }
        }

        private static bool IsDX10(TextureFormat format) => format switch
        {
            TextureFormat.R8G8B8A8 => false,
            TextureFormat.BC7 => true,
            _ => throw new NotImplementedException(),
        };

        private static void WriteDDSPixelFormat(TextureFile texture, FileStream output, Endian endian)
        {
            uint flags, fourCC, rgbBitCount, redBitMask, greenBitMask, blueBitMask, alphaBitMask;
            switch (texture.Format)
            {
                case TextureFormat.R8G8B8A8:
                {
                    flags = 0x41;
                    fourCC = 0;
                    rgbBitCount = 32;
                    redBitMask = 0x000000FFu;
                    greenBitMask = 0x0000FF00u;
                    blueBitMask = 0x00FF0000u;
                    alphaBitMask = 0xFF000000u;
                    break;
                }

                case TextureFormat.BC7:
                {
                    flags = 4;
                    fourCC = 0x30315844; // 'DX10'
                    rgbBitCount = redBitMask = greenBitMask = blueBitMask = alphaBitMask = 0;
                    break;
                }

                default:
                {
                    throw new NotImplementedException();
                }
            }
            output.WriteValueU32(32, endian); // size
            output.WriteValueU32(flags, endian);
            output.WriteValueU32(fourCC, endian);
            output.WriteValueU32(rgbBitCount, endian);
            output.WriteValueU32(redBitMask, endian);
            output.WriteValueU32(greenBitMask, endian);
            output.WriteValueU32(blueBitMask, endian);
            output.WriteValueU32(alphaBitMask, endian);
        }

        private static uint TranslateDX10Format(TextureFormat format) => format switch
        {
            TextureFormat.BC7 => 0x62,
            _ => throw new NotImplementedException(),
        };
    }
}
