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

namespace Gibbed.Reborn.FileFormats
{
    public enum TextureFormat : byte
    {
        R8G8B8A8 = 0,
        R8G8B8 = 1,
        R5G6B5 = 2,
        R5G5B5A1 = 3,
        R4G4B4A4 = 4,
        P8 = 5,
        P4 = 6,
        BC1 = 7,
        BC2 = 8,
        BC3 = 9,
        BC4 = 10,
        BC5 = 11,
        BC6H = 12,
        BC7 = 13,
        D24S8 = 14,
        D15S1 = 15,
        L8 = 16,
        A8 = 17,
        L4A4 = 18,
        L8A8 = 19,
        YUV = 20,
        PVRTII_4 = 21,
        PVRTII_2 = 22,
        U16U16 = 23,
        F16F16 = 24,
        F32 = 25,
        RGBA_U32U32 = 26,
        R16G16B16A16F = 27,
        R32G32B32A32F = 28,
        D32F = 29,
        R10G10B10A2 = 30,
        R11G11B10 = 31,
        R16_UNorm = 32,
        R16F = 33,
        B8G8R8A8 = 34,
        A8R8G8B8 = 35,
        A8B8G8R8 = 36,
        B8G8R8 = 37,
        B5G6R5 = 38,
        B5G5R5A1 = 39,
        B4G4G4A4 = 40,
    }
}
