/* Copyright (c) 2026 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.TacticsOgre.TextFormats;

internal static class MacroCodeHelpers
{
    private static readonly byte[] _PSPJapaneseSizes =
    {
        1, 2, 5, 6, 4, 3, 1, 2, 2, 2, 2, 3, 2, 3, 3, 3, // 80-8F
        3, 1, 1, 1, 1, 4, 1, 1, 3, 3, 2, 1, 3, 0, 0, 0, // 90-9F
        1, 1, 1, 1, 1, 1, 1, 1, 1, // A0-A8
    };

    private static readonly byte[] _PSPEnglishSizes =
    {
        1, 3, 5, 6, 4, 3, 1, 2, 2, 2, 2, 3, 2, 3, 3, 3, // 80-8F
        3, 1, 1, 1, 1, 4, 1, 1, 3, 3, 2, 1, 3, 2, 1, 1, // 90-9F
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // A0-A9
    };

    private static readonly byte[] _RebornSizes =
    {
        1, 3, 5, 6, 5, 3, 1, 2, 2, 2, 2, 3, 2, 3, 3, 3, // 80-8F
        3, 1, 1, 1, 1, 4, 1, 1, 3, 3, 2, 1, 3, 2, 1, 1, // 90-9F
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 3, 1, 3, 3, // A0-AF
        1, 1, 4, 4, 3, 3, // B0-B5
    };

    public static int GetSize(this MacroCode opcode, Dialect dialect)
    {
        var sizes = dialect switch
        {
            Dialect.PSPJapanese => _PSPJapaneseSizes,
            Dialect.PSPEnglish => _PSPEnglishSizes,
            Dialect.Reborn => _RebornSizes,
            _ => throw new ArgumentOutOfRangeException(nameof(dialect)),
        };
        var index = (int)opcode - 0x80;
        return index >= 0 && index < sizes.Length ? sizes[index] : 0;
    }

    public static readonly string[] WindowTitles =
    [
        "caution",
        "confirm",
        "notice",
        "tutorial",
        "warning",
    ];

    public static class Conditions
    {
        public const int Undead = 1;
    }

    public static int DecodeU16(byte a, byte b) => ((a - 1) * 255) + (b - 1);

    public static (byte a, byte b) EncodeU16(int value)
    {
        if (value < 0 || value > (254 * 255) + 254)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "16-bit text arguments go from 0 to 65024");
        }
        return ((byte)((value / 255) + 1), (byte)((value % 255) + 1));
    }
}
