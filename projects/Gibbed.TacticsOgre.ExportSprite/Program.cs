/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
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
using System.Runtime.InteropServices;
using Gibbed.IO;
using Gibbed.TacticsOgre.FileFormats;
using NDesk.Options;
using Palette = Gibbed.TacticsOgre.FileFormats.Sprite.Palette;
using Texture = Gibbed.TacticsOgre.FileFormats.Sprite.Texture;

namespace Gibbed.TacticsOgre.ExportSprite
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
                { "v|verbose", "be verbose (list files)", v => verbose = v != null },
                { "h|help", "show this message and exit", v => showHelp = v != null },
            };

            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extra.Count < 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_sprite+", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPaths = new List<string>();
            foreach (var inputPath in extra)
            {
                if (Directory.Exists(inputPath) == true)
                {
                    inputPaths.AddRange(Directory.GetFiles(inputPath, "*.sprite", SearchOption.AllDirectories));
                }
                else
                {
                    inputPaths.Add(inputPath);
                }
            }

            foreach (var inputPath in inputPaths)
            {
                string outputPath = Path.ChangeExtension(inputPath, null);

                if (verbose == true)
                {
                    Console.WriteLine(inputPath);
                }

                Export(inputPath, outputPath);
            }
        }

        private static void Export(string inputPath, string outputPath)
        {
            var file = new SpriteFile();
            using (var input = File.OpenRead(inputPath))
            {
                file.Deserialize(input, Endian.Little);
            }

            var sprite = file.Sprite;
            if (sprite.Texture != null)
            {
                var texture = sprite.Texture.Value;
                if (sprite.Palettes.Length == 1)
                {
                    var bitmapPath = Path.ChangeExtension(outputPath, ".png");
                    var data = ExportPalettized(texture, sprite.Palettes[0]);
                    var bitmap = MakeBitmapPalettized(
                        texture.TotalWidth, texture.TotalHeight,
                        data,
                        sprite.Palettes[0]);
                    using (bitmap)
                    {
                        bitmap.Save(bitmapPath, ImageFormat.Png);
                    }
                }
                else
                {
                    int i = 0;
                    foreach (var palette in sprite.Palettes)
                    {
                        var bitmapPath = Path.ChangeExtension($"{outputPath}_{i}", ".png");
                        var data = ExportPalettized(texture, palette);
                        var bitmap = MakeBitmapPalettized(
                            texture.TotalWidth, texture.TotalHeight,
                            data,
                            palette);
                        using (bitmap)
                        {
                            bitmap.Save(bitmapPath, ImageFormat.Png);
                        }
                        i++;
                    }
                }
            }
        }

        private static byte[] ExportARGB(Texture texture, Palette palette)
        {
            if (texture.BitsPerPixel != 4 && texture.BitsPerPixel != 8)
            {
                throw new NotSupportedException();
            }

            var data = new byte[texture.TotalWidth * texture.TotalHeight * 4];

            int x = 0;
            int y = 0;

            int blockLength = (int)(texture.BlockWidth * texture.BlockHeight * (texture.BitsPerPixel / 8.0f));
            for (int i = 0; i < texture.Data.Length; i += blockLength)
            {
                var block = new byte[blockLength];
                Array.Copy(texture.Data, i, block, 0, Math.Min(blockLength, texture.Data.Length - i));

                int rx = 0;
                int ry = 0;
                for (int j = 0; j < block.Length; j++)
                {
                    if (texture.BitsPerPixel == 8)
                    {
                        if (ry >= texture.BlockHeight)
                        {
                            throw new InvalidOperationException();
                        }

                        var abgr = palette.Colors[block[j]];
                        int o = (((y + ry) * texture.TotalWidth) + (x + rx)) * 4;

                        data[o + 0] = (byte)((abgr & 0x00FF0000) >> 16);
                        data[o + 1] = (byte)((abgr & 0x0000FF00) >> 8);
                        data[o + 2] = (byte)((abgr & 0x000000FF) >> 0);
                        data[o + 3] = (byte)((abgr & 0xFF000000) >> 24);

                        rx++;
                        if (rx >= texture.BlockWidth)
                        {
                            rx = 0;
                            ry++;
                        }
                    }
                    else if (texture.BitsPerPixel == 4)
                    {
                        // a
                        {
                            if (ry >= texture.BlockHeight)
                            {
                                throw new InvalidOperationException();
                            }

                            var abgr = palette.Colors[(block[j] & 0x0F) >> 0];
                            int o = (((y + ry) * texture.TotalWidth) + (x + rx)) * 4;

                            data[o + 0] = (byte)((abgr & 0x00FF0000) >> 16);
                            data[o + 1] = (byte)((abgr & 0x0000FF00) >> 8);
                            data[o + 2] = (byte)((abgr & 0x000000FF) >> 0);
                            data[o + 3] = (byte)((abgr & 0xFF000000) >> 24);

                            rx++;
                            if (rx >= texture.BlockWidth)
                            {
                                rx = 0;
                                ry++;
                            }
                        }

                        // b
                        {
                            if (ry >= texture.BlockHeight)
                            {
                                throw new InvalidOperationException();
                            }

                            var abgr = palette.Colors[(block[j] & 0xF0) >> 4];
                            int o = (((y + ry) * texture.TotalWidth) + (x + rx)) * 4;

                            data[o + 0] = (byte)((abgr & 0x00FF0000) >> 16);
                            data[o + 1] = (byte)((abgr & 0x0000FF00) >> 8);
                            data[o + 2] = (byte)((abgr & 0x000000FF) >> 0);
                            data[o + 3] = (byte)((abgr & 0xFF000000) >> 24);

                            rx++;
                            if (rx >= texture.BlockWidth)
                            {
                                rx = 0;
                                ry++;
                            }
                        }
                    }
                }

                x += texture.BlockWidth;
                if (x >= texture.TotalWidth)
                {
                    x = 0;
                    y += texture.BlockHeight;
                }
            }

            return data;
        }

        private static byte[] ExportPalettized(Texture texture, Palette palette)
        {
            if (texture.BitsPerPixel != 4 && texture.BitsPerPixel != 8)
            {
                throw new NotSupportedException();
            }

            var data = new byte[texture.TotalWidth * texture.TotalHeight * 4];

            int x = 0;
            int y = 0;

            int blockLength = (int)(texture.BlockWidth * texture.BlockHeight * (texture.BitsPerPixel / 8.0f));
            for (int i = 0; i < texture.Data.Length; i += blockLength)
            {
                var block = new byte[blockLength];
                Array.Copy(texture.Data, i, block, 0, Math.Min(blockLength, texture.Data.Length - i));

                int rx = 0;
                int ry = 0;
                for (int j = 0; j < block.Length; j++)
                {
                    if (texture.BitsPerPixel == 8)
                    {
                        if (ry >= texture.BlockHeight)
                        {
                            throw new InvalidOperationException();
                        }

                        int o = ((y + ry) * texture.TotalWidth) + (x + rx);
                        data[o] = block[j];

                        rx++;
                        if (rx >= texture.BlockWidth)
                        {
                            rx = 0;
                            ry++;
                        }
                    }
                    else if (texture.BitsPerPixel == 4)
                    {
                        // a
                        {
                            if (ry >= texture.BlockHeight)
                            {
                                throw new InvalidOperationException();
                            }

                            int o = ((y + ry) * texture.TotalWidth) + (x + rx);

                            data[o] = (byte)((block[j] & 0x0F) >> 0);

                            rx++;
                            if (rx >= texture.BlockWidth)
                            {
                                rx = 0;
                                ry++;
                            }
                        }

                        // b
                        {
                            if (ry >= texture.BlockHeight)
                            {
                                throw new InvalidOperationException();
                            }

                            int o = ((y + ry) * texture.TotalWidth) + (x + rx);

                            data[o] = (byte)((block[j] & 0xF0) >> 4);

                            rx++;
                            if (rx >= texture.BlockWidth)
                            {
                                rx = 0;
                                ry++;
                            }
                        }
                    }
                }

                x += texture.BlockWidth;
                if (x >= texture.TotalWidth)
                {
                    x = 0;
                    y += texture.BlockHeight;
                }
            }

            return data;
        }

        private static Bitmap MakeBitmapARGB(int width, int height, byte[] buffer, bool keepAlpha)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            for (int i = 0; i < width * height * 4; i += 4)
            {
                // flip red and blue
                byte r = buffer[i + 0];
                buffer[i + 0] = buffer[i + 2];
                buffer[i + 2] = r;
            }
            var area = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(area, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(buffer, 0, data.Scan0, width * height * 4);
            bitmap.UnlockBits(data);
            return bitmap;
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
