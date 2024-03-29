﻿/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Gibbed.IO;
using Gibbed.TacticsOgre.FileFormats;
using Gibbed.TacticsOgre.FileFormats.Images;
using NDesk.Options;
using Image = Gibbed.TacticsOgre.FileFormats.Images.Image;

namespace Gibbed.TacticsOgre.ExportImage
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        private static bool IsImagePath(string path)
        {
            return
                path.EndsWith(".img", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".spr", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".ashg", StringComparison.OrdinalIgnoreCase);
        }

        public static void Main(string[] args)
        {
            bool verbose = false;
            bool showHelp = false;
            string palettePath = null;
            int paletteIndex = 0;


            var options = new OptionSet()
            {
                { "p|palette=", "set palette path", v => palettePath = v },
                { "i|palette-index=", "set palette index", v => paletteIndex = int.Parse(v) },
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_img+", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            List<string> inputPaths = new();
            foreach (var inputPath in extras)
            {
                if (Directory.Exists(inputPath) == true)
                {
                    inputPaths.AddRange(Directory
                        .EnumerateFiles(inputPath, "*", SearchOption.AllDirectories)
                        .Where(fp => IsImagePath(fp) == true));
                }
                else
                {
                    inputPaths.Add(inputPath);
                }
            }

            Palette? palette;
            if (string.IsNullOrEmpty(palettePath) == true)
            {
                palette = null;
            }
            else
            {
                using (var input = File.OpenRead(palettePath))
                {
                    ImageFile paletteFile = new();
                    paletteFile.Deserialize(input, Endian.Little);
                    if (paletteIndex < 0 || paletteIndex >= paletteFile.Image.Palettes.Length)
                    {
                        Console.WriteLine("Invalid palette index.");
                        return;
                    }
                    palette = paletteFile.Image.Palettes[paletteIndex];
                }
            }

            foreach (var inputPath in inputPaths.OrderBy(v => v))
            {
                string outputPath = Path.ChangeExtension(inputPath, null);

                if (verbose == true)
                {
                    Console.WriteLine(inputPath);
                }

                Export(inputPath, palette, outputPath);
            }
        }

        private static void Export(string inputPath, Palette? palette, string outputPath)
        {
            var inputBytes = File.ReadAllBytes(inputPath);

            if (BitConverter.ToUInt32(inputBytes, 0) == RLE.Signature)
            {
                inputBytes = RLE.Decompress(inputBytes, 0, inputBytes.Length);
            }

            Image image;
            using (var input = new MemoryStream(inputBytes, false))
            {
                ImageFile imageFile = new();
                imageFile.Deserialize(input, Endian.Little);
                image = imageFile.Image;
            }

            if (image.Texture != null)
            {
                var texture = image.Texture.Value;
                if (palette != null)
                {
                    var bitmapPath = Path.ChangeExtension(outputPath, ".png");
                    var data = ExportPalettized(texture);
                    var bitmap = MakeBitmapPalettized(
                        texture.TotalWidth, texture.TotalHeight,
                        data,
                        palette.Value);
                    using (bitmap)
                    {
                        bitmap.Save(bitmapPath, ImageFormat.Png);
                    }
                }
                else if (image.Palettes.Length == 1)
                {
                    var bitmapPath = Path.ChangeExtension(outputPath, ".png");
                    var data = ExportPalettized(texture);
                    var bitmap = MakeBitmapPalettized(
                        texture.TotalWidth, texture.TotalHeight,
                        data,
                        image.Palettes[0]);
                    using (bitmap)
                    {
                        bitmap.Save(bitmapPath, ImageFormat.Png);
                    }
                }
                else
                {
                    int i = 0;
                    foreach (var palette2 in image.Palettes)
                    {
                        var bitmapPath = Path.ChangeExtension($"{outputPath}_{i}", ".png");
                        var data = ExportPalettized(texture);
                        var bitmap = MakeBitmapPalettized(
                            texture.TotalWidth, texture.TotalHeight,
                            data,
                            palette2);
                        using (bitmap)
                        {
                            bitmap.Save(bitmapPath, ImageFormat.Png);
                        }
                        i++;
                    }
                }
            }
        }

        private static byte[] ExportPalettized(Texture texture)
        {
            if (texture.BitsPerPixel != 4 && texture.BitsPerPixel != 8)
            {
                throw new NotSupportedException();
            }
            byte[] data = texture.IsReborn == false
                ? Deblock(texture)
                : texture.Data;
            if (texture.BitsPerPixel == 8)
            {
                return data;
            }
            byte[] newData = new byte[texture.TotalWidth * texture.TotalHeight];
            for (int i = 0, o = 0; i < data.Length; i++, o += 2)
            {
                var b = data[i];
                newData[o + 0] = (byte)((b & 0x0F) >> 0);
                newData[o + 1] = (byte)((b & 0xF0) >> 4);
            }
            return newData;
        }

        private static byte[] Deblock(Texture texture)
        {
            int outputPitch = (int)(texture.TotalWidth * (texture.BitsPerPixel / 8.0f));
            var data = new byte[texture.TotalHeight * outputPitch];
            int blockPitch = (int)(texture.BlockWidth * (texture.BitsPerPixel / 8.0f));
            int blockLength = texture.BlockHeight * blockPitch;
            int x = 0, bx = 0, y = 0;
            for (int i = 0; i < texture.Data.Length; i += blockLength)
            {
                int outputOffset = y * outputPitch;
                outputOffset += bx;
                for (int j = 0, inputOffset = i; inputOffset < texture.Data.Length && j < texture.BlockHeight; j++)
                {
                    Array.Copy(texture.Data, inputOffset, data, outputOffset, blockPitch);
                    inputOffset += blockPitch;
                    outputOffset += outputPitch;
                }
                x += texture.BlockWidth;
                bx += blockPitch;
                if (x >= texture.TotalWidth)
                {
                    x = 0;
                    bx = 0;
                    y += texture.BlockHeight;
                }
            }
            return data;
        }

        private static Bitmap MakeBitmapPalettized(int width, int height, byte[] buffer, Palette palette)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var bitmapPalette = bitmap.Palette;
            for (int i = 0; i < palette.Colors.Length; i++)
            {
                var abgr = palette.Colors[i];
                uint b = (abgr & 0x00FF0000) >> 16;
                uint r = (abgr & 0x000000FF) << 16;
                var argb = (int)((abgr & 0xFF00FF00) | b | r);
                bitmapPalette.Entries[i] = Color.FromArgb(argb);
            }
            bitmap.Palette = bitmapPalette;
            var area = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(area, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(buffer, 0, data.Scan0, width * height);
            bitmap.UnlockBits(data);
            return bitmap;
        }
    }
}
