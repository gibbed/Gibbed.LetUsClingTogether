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

namespace Gibbed.Reborn.FileFormats
{
    public static class TextureFormatHelpers
    {
        public static int GetBitCount(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.PVRTII_2:
                {
                    return 2;
                }

                case TextureFormat.P4:
                case TextureFormat.BC1:
                case TextureFormat.BC4:
                case TextureFormat.PVRTII_4:
                {
                    return 4;
                }

                case TextureFormat.P8:
                case TextureFormat.BC2:
                case TextureFormat.BC3:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.L8:
                case TextureFormat.A8:
                case TextureFormat.L4A4:
                {
                    return 8;
                }

                case TextureFormat.R5G6B5:
                case TextureFormat.R5G5B5A1:
                case TextureFormat.R4G4B4A4:
                case TextureFormat.D15S1:
                case TextureFormat.L8A8:
                case TextureFormat.R16_UNorm:
                case TextureFormat.R16F:
                case TextureFormat.B5G6R5:
                case TextureFormat.B5G5R5A1:
                case TextureFormat.B4G4G4A4:
                {
                    return 16;
                }

                case TextureFormat.R8G8B8:
                case TextureFormat.B8G8R8:
                {
                    return 24;
                }

                case TextureFormat.R8G8B8A8:
                case TextureFormat.D24S8:
                case TextureFormat.YUV:
                case TextureFormat.U16U16:
                case TextureFormat.F16F16:
                case TextureFormat.F32:
                case TextureFormat.D32F:
                case TextureFormat.R10G10B10A2:
                case TextureFormat.R11G11B10:
                case TextureFormat.B8G8R8A8:
                case TextureFormat.A8R8G8B8:
                case TextureFormat.A8B8G8R8:
                {
                    return 32;
                }

                case TextureFormat.RGBA_U32U32:
                case TextureFormat.R16G16B16A16F:
                {
                    return 64;
                }

                case TextureFormat.R32G32B32A32F:
                {
                    return 128;
                }
            }

            throw new NotSupportedException();
        }

        public static int GetUnknown0C(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                {
                    return 0;
                }

                case TextureFormat.PVRTII_4:
                case TextureFormat.PVRTII_2:
                {
                    return 1;
                }

                case TextureFormat.BC1:
                case TextureFormat.BC2:
                case TextureFormat.BC3:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                {
                    return 4;
                }

                case TextureFormat.R8G8B8A8:
                case TextureFormat.R8G8B8:
                case TextureFormat.R5G6B5:
                case TextureFormat.R5G5B5A1:
                case TextureFormat.R4G4B4A4:
                case TextureFormat.P8:
                case TextureFormat.P4:
                case TextureFormat.D24S8:
                case TextureFormat.D15S1:
                case TextureFormat.L8:
                case TextureFormat.A8:
                case TextureFormat.L4A4:
                case TextureFormat.L8A8:
                case TextureFormat.YUV:
                case TextureFormat.U16U16:
                case TextureFormat.F16F16:
                case TextureFormat.F32:
                case TextureFormat.RGBA_U32U32:
                case TextureFormat.R16G16B16A16F:
                case TextureFormat.R32G32B32A32F:
                case TextureFormat.D32F:
                case TextureFormat.R10G10B10A2:
                case TextureFormat.R11G11B10:
                case TextureFormat.R16_UNorm:
                case TextureFormat.R16F:
                case TextureFormat.B8G8R8A8:
                case TextureFormat.A8R8G8B8:
                case TextureFormat.A8B8G8R8:
                case TextureFormat.B8G8R8:
                case TextureFormat.B5G6R5:
                case TextureFormat.B5G5R5A1:
                case TextureFormat.B4G4G4A4:
                {
                    return 16;
                }
            }

            throw new NotSupportedException();
        }

        public static int GetBlockSize(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                {
                    return 0;
                }

                case TextureFormat.R8G8B8A8:
                case TextureFormat.R8G8B8:
                case TextureFormat.R5G6B5:
                case TextureFormat.R5G5B5A1:
                case TextureFormat.R4G4B4A4:
                case TextureFormat.P8:
                case TextureFormat.P4:
                case TextureFormat.D24S8:
                case TextureFormat.D15S1:
                case TextureFormat.L8:
                case TextureFormat.A8:
                case TextureFormat.L4A4:
                case TextureFormat.L8A8:
                case TextureFormat.YUV:
                case TextureFormat.U16U16:
                case TextureFormat.F16F16:
                case TextureFormat.F32:
                case TextureFormat.RGBA_U32U32:
                case TextureFormat.R16G16B16A16F:
                case TextureFormat.R32G32B32A32F:
                case TextureFormat.D32F:
                case TextureFormat.R10G10B10A2:
                case TextureFormat.R11G11B10:
                case TextureFormat.R16_UNorm:
                case TextureFormat.R16F:
                case TextureFormat.B8G8R8A8:
                case TextureFormat.A8R8G8B8:
                case TextureFormat.A8B8G8R8:
                case TextureFormat.B8G8R8:
                case TextureFormat.B5G6R5:
                case TextureFormat.B5G5R5A1:
                case TextureFormat.B4G4G4A4:
                {
                    return 1;
                }

                case TextureFormat.PVRTII_2:
                {
                    return 2;
                }

                case TextureFormat.BC1:
                case TextureFormat.BC2:
                case TextureFormat.BC3:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.PVRTII_4:
                {
                    return 4;
                }
            }

            throw new NotSupportedException();
        }

        public static int GetAlphaBitCount(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.R8G8B8:
                case TextureFormat.R5G6B5:
                case TextureFormat.P8:
                case TextureFormat.P4:
                case TextureFormat.BC1:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.D24S8:
                case TextureFormat.D15S1:
                case TextureFormat.L8:
                case TextureFormat.PVRTII_4:
                case TextureFormat.PVRTII_2:
                case TextureFormat.U16U16:
                case TextureFormat.F16F16:
                case TextureFormat.F32:
                case TextureFormat.D32F:
                case TextureFormat.R10G10B10A2:
                case TextureFormat.R11G11B10:
                case TextureFormat.R16_UNorm:
                case TextureFormat.R16F:
                case TextureFormat.B8G8R8:
                case TextureFormat.B5G6R5:
                {
                    return 0;
                }

                case TextureFormat.R5G5B5A1:
                case TextureFormat.B5G5R5A1:
                {
                    return 1;
                }

                case TextureFormat.R4G4B4A4:
                case TextureFormat.BC2:
                case TextureFormat.L4A4:
                case TextureFormat.B4G4G4A4:
                {
                    return 4;
                }

                case TextureFormat.R8G8B8A8:
                case TextureFormat.BC3:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.A8:
                case TextureFormat.L8A8:
                case TextureFormat.YUV:
                case TextureFormat.RGBA_U32U32:
                case TextureFormat.R16G16B16A16F:
                case TextureFormat.R32G32B32A32F:
                case TextureFormat.B8G8R8A8:
                case TextureFormat.A8R8G8B8:
                case TextureFormat.A8B8G8R8:
                {
                    return 8;
                }
            }

            throw new NotSupportedException();
        }

        public static bool IsPalettized(this TextureFormat format) => format switch
        {
            TextureFormat.P8 => true,
            TextureFormat.P4 => true,
            _ => false,
        };

        public static bool IsCompressed(this TextureFormat format) => format switch
        {
            TextureFormat.BC1 => true,
            TextureFormat.BC2 => true,
            TextureFormat.BC3 => true,
            TextureFormat.BC4 => true,
            TextureFormat.BC5 => true,
            TextureFormat.BC6H => true,
            TextureFormat.BC7 => true,
            _ => false,
        };

        public static (int stride, int rows) CalculateBufferStrideAndRows(
            this TextureFormat format,
            int width, int height,
            int level)
        {
            width = Math.Max(1, width >> level);
            height = Math.Max(1, height >> level);

            int stride, rows;

            if (format.IsCompressed() == false)
            {
                stride = width * (format.GetBitCount() >> 3);
                rows = height;
            }
            else
            {
                int blockSize = format.GetBlockSize();
                var bitsPerPixel = format.GetBitCount();
                var bytesPerBlock = blockSize * blockSize * bitsPerPixel / 8;
                stride = RoundUp(width, blockSize) / blockSize * bytesPerBlock;
                rows = RoundUp(height, blockSize) / blockSize;
            }

            return (stride, rows);
        }

        public static int GetBytesPerBlock(this TextureFormat format)
        {
            if (format.IsCompressed() == false)
            {
                return format.GetBitCount() >> 3;
            }
            else
            {
                int blockSize = format.GetBlockSize();
                var bitsPerPixel = format.GetBitCount();
                return blockSize * blockSize * bitsPerPixel / 8;
            }
        }

        public static int CalculateBufferSize(this TextureFormat format, int width, int height, int level)
        {
            var (stride, rows) = format.CalculateBufferStrideAndRows(width, height, level);
            return stride * rows;
        }

        private static int RoundUp(int value, int multiple)
        {
            return value != 0 ? (((value + multiple - 1) / multiple) * multiple) : multiple;
        }
    }
}
