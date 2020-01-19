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
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Gibbed.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Gibbed.TacticsOgre.FileFormats;
using NDesk.Options;

namespace Gibbed.TacticsOgre.ConvertSprite
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        private static byte[] UnpackSpriteARGB(SpriteFile sprite, List<Color> palette)
        {
            if (sprite.BitsPerPixel != 4 &&
                sprite.BitsPerPixel != 8)
            {
                throw new NotSupportedException();
            }

            var data = new byte[sprite.TotalWidth * sprite.TotalHeight * 4];

            int x = 0;
            int y = 0;

            int blockLength = (int)(sprite.BlockWidth * sprite.BlockHeight * (sprite.BitsPerPixel / 8.0f));
            for (int i = 0; i < sprite.Data.Length; i += blockLength)
            {
                var block = new byte[blockLength];
                Array.Copy(sprite.Data, i, block, 0, Math.Min(blockLength, sprite.Data.Length - i));

                int rx = 0;
                int ry = 0;
                for (int j = 0; j < block.Length; j++)
                {
                    if (sprite.BitsPerPixel == 8)
                    {
                        if (ry >= sprite.BlockHeight)
                        {
                            throw new InvalidOperationException();
                        }

                        var color = palette[block[j]];
                        int o = (((y + ry) * sprite.TotalWidth) + (x + rx)) * 4;
                        data[o + 0] = color.B;
                        data[o + 1] = color.G;
                        data[o + 2] = color.R;
                        data[o + 3] = color.A;

                        rx++;
                        if (rx >= sprite.BlockWidth)
                        {
                            rx = 0;
                            ry++;
                        }
                    }
                    else if (sprite.BitsPerPixel == 4)
                    {
                        // a
                        {
                            if (ry >= sprite.BlockHeight)
                            {
                                throw new InvalidOperationException();
                            }

                            var color = palette[(block[j] & 0x0F) >> 0];
                            int o = (((y + ry) * sprite.TotalWidth) + (x + rx)) * 4;

                            data[o + 0] = color.B;
                            data[o + 1] = color.G;
                            data[o + 2] = color.R;
                            data[o + 3] = color.A;

                            rx++;
                            if (rx >= sprite.BlockWidth)
                            {
                                rx = 0;
                                ry++;
                            }
                        }

                        // b
                        {
                            if (ry >= sprite.BlockHeight)
                            {
                                throw new InvalidOperationException();
                            }

                            var color = palette[(block[j] & 0xF0) >> 4];
                            int o = (((y + ry) * sprite.TotalWidth) + (x + rx)) * 4;
                            data[o + 0] = color.B;
                            data[o + 1] = color.G;
                            data[o + 2] = color.R;
                            data[o + 3] = color.A;

                            rx++;
                            if (rx >= sprite.BlockWidth)
                            {
                                rx = 0;
                                ry++;
                            }
                        }
                    }
                }

                x += sprite.BlockWidth;
                if (x >= sprite.TotalWidth)
                {
                    x = 0;
                    y += sprite.BlockHeight;
                }
            }

            return data;
        }

        private static byte[] UnpackSpritePalette(SpriteFile sprite, List<Color> palette)
        {
            if (sprite.BitsPerPixel != 4 &&
                sprite.BitsPerPixel != 8)
            {
                throw new NotSupportedException();
            }

            var data = new byte[sprite.TotalWidth * sprite.TotalHeight * 4];

            int x = 0;
            int y = 0;

            int blockLength = (int)(sprite.BlockWidth * sprite.BlockHeight * (sprite.BitsPerPixel / 8.0f));
            for (int i = 0; i < sprite.Data.Length; i += blockLength)
            {
                var block = new byte[blockLength];
                Array.Copy(sprite.Data, i, block, 0, Math.Min(blockLength, sprite.Data.Length - i));

                int rx = 0;
                int ry = 0;
                for (int j = 0; j < block.Length; j++)
                {
                    if (sprite.BitsPerPixel == 8)
                    {
                        if (ry >= sprite.BlockHeight)
                        {
                            throw new InvalidOperationException();
                        }

                        int o = ((y + ry) * sprite.TotalWidth) + (x + rx);
                        data[o] = block[j];

                        rx++;
                        if (rx >= sprite.BlockWidth)
                        {
                            rx = 0;
                            ry++;
                        }
                    }
                    else if (sprite.BitsPerPixel == 4)
                    {
                        // a
                        {
                            if (ry >= sprite.BlockHeight)
                            {
                                throw new InvalidOperationException();
                            }

                            int o = ((y + ry) * sprite.TotalWidth) + (x + rx);

                            data[o] = (byte)((block[j] & 0x0F) >> 0);

                            rx++;
                            if (rx >= sprite.BlockWidth)
                            {
                                rx = 0;
                                ry++;
                            }
                        }

                        // b
                        {
                            if (ry >= sprite.BlockHeight)
                            {
                                throw new InvalidOperationException();
                            }

                            int o = ((y + ry) * sprite.TotalWidth) + (x + rx);

                            data[o] = (byte)((block[j] & 0xF0) >> 4);

                            rx++;
                            if (rx >= sprite.BlockWidth)
                            {
                                rx = 0;
                                ry++;
                            }
                        }
                    }
                }

                x += sprite.BlockWidth;
                if (x >= sprite.TotalWidth)
                {
                    x = 0;
                    y += sprite.BlockHeight;
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

        private static Bitmap MakeBitmapPalette(int width, int height, byte[] buffer, List<Color> colors)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var palette = bitmap.Palette;
            for (int i = 0; i < colors.Count; i++)
            {
                palette.Entries[i] = colors[i];
            }
            bitmap.Palette = palette;
            var area = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(area, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(buffer, 0, data.Scan0, width * height);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static void Main(string[] args)
        {
            bool verbose = false;
            bool showHelp = false;

            OptionSet options = new OptionSet()
            {
                {
                    "v|verbose",
                    "be verbose (list files)",
                    v => verbose = v != null
                },

                {
                    "h|help",
                    "show this message and exit", 
                    v => showHelp = v != null
                },
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
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_gfx+", GetExecutableName());
                Console.WriteLine("Convert specified sprite.");
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

                Console.WriteLine(inputPath);

                var sprite = new SpriteFile();
                using (var input = File.OpenRead(inputPath))
                {
                    sprite.Deserialize(input);
                }

                if (sprite.Data != null)
                {
                    if (sprite.Palettes.Count == 1)
                    {
                        var data = UnpackSpritePalette(sprite, sprite.Palettes[0]);
                        var bitmap = MakeBitmapPalette(
                            sprite.TotalWidth, sprite.TotalHeight,
                            data,
                            sprite.Palettes[0]);
                        var bitmapPath = Path.ChangeExtension(
                            outputPath,
                            ".png");
                        bitmap.Save(bitmapPath, ImageFormat.Png);
                    }
                    else
                    {
                        int i = 0;
                        foreach (var palette in sprite.Palettes)
                        {
                            var data = UnpackSpritePalette(sprite, palette);
                            var bitmap = MakeBitmapPalette(
                                sprite.TotalWidth, sprite.TotalHeight,
                                data,
                                palette);
                            var bitmapPath = Path.ChangeExtension(
                                string.Format("{0}_{1}", outputPath, i),
                                ".png");
                            bitmap.Save(bitmapPath, ImageFormat.Png);
                            i++;
                        }
                    }
                }
            }
        }
    }
}
