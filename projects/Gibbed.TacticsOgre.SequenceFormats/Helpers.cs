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
using Gibbed.Memory;

namespace Gibbed.TacticsOgre.SequenceFormats;

internal static class Helpers
{
    public static void ReadPadding(ReadOnlySpan<byte> span, ref int index, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var b = span.ReadValueU8(ref index);
            if (b != 0)
            {
                throw new SequenceFormatException($"Padding byte at 0x{index - 1:X} is {b}; it must be zero.");
            }
        }
    }

    public static bool IsZero(ReadOnlySpan<byte> span)
    {
        foreach (var b in span)
        {
            if (b != 0)
            {
                return false; 
            }
        }
        return true;
    }

    public static FileVersion? GetVersion(uint magic) => magic switch
    {
        Signatures.V1 => FileVersion.V1,
        Signatures.V2 => FileVersion.V2,
        Signatures.V3 => FileVersion.V3,
        _ => null,
    };

    public static uint GetSignature(FileVersion version) => version switch
    {
        FileVersion.V1 => Signatures.V1,
        FileVersion.V2 => Signatures.V2,
        FileVersion.V3 => Signatures.V3,
        _ => throw new ArgumentOutOfRangeException(nameof(version), version, "unknown file version"),
    };
}
