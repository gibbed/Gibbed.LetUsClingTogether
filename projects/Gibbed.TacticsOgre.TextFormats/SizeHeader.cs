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

internal struct SizeHeader
{
    public int Width;
    public int CharacterCount;
    public int LineCount;
    public bool HasControlCodes;
    public bool Flag7;
    public int A;
    public bool HasRecords;

    public static SizeHeader Unpack(ReadOnlySpan<byte> h)
    {
        SizeHeader header;
        header.HasRecords = (h[0] & 1) != 0;
        header.Width = (127 * (h[0] >> 1)) + (h[1] >> 1) - 128;
        header.CharacterCount = (63 * (h[2] & 0x3F)) + (h[3] & 0x3F) - 64;
        header.HasControlCodes = (h[2] & 0x40) != 0;
        header.Flag7 = (h[2] & 0x80) != 0;
        header.A = ((h[3] >> 6) << 3) | (h[4] & 7);
        header.LineCount = h[4] >> 3;
        return header;
    }

    public readonly void Pack(Span<byte> h)
    {
        var (widthHigh, widthLow) = Split(this.Width + 128, 127, 127, "width");
        var (countHigh, countLow) = Split(this.CharacterCount + 64, 63, 63, "character count");
        if (this.A < 0 || this.A > 31)
        {
            throw new FormatException($"size header value a goes from 0 to 31, got {this.A}");
        }
        if (this.LineCount < 1 || this.LineCount > 31)
        {
            throw new FormatException($"size header line count goes from 1 to 31, got {this.LineCount}");
        }
        h[0] = (byte)((widthHigh << 1) | (this.HasRecords == true ? 1 : 0));
        h[1] = (byte)(widthLow << 1);
        h[2] = (byte)(countHigh | (this.HasControlCodes == true ? 0x40 : 0) | (this.Flag7 == true ? 0x80 : 0));
        h[3] = (byte)(countLow | ((this.A >> 3) << 6));
        h[4] = (byte)((this.LineCount << 3) | (this.A & 7));
    }

    private static (int high, int low) Split(int value, int radix, int maxHigh, string what)
    {
        var high = value / radix;
        var low = value % radix;
        if (low == 0)
        {
            // keep the low byte non-zero
            high--;
            low = radix;
        }
        if (high < 1 || high > maxHigh)
        {
            throw new FormatException($"size header {what} is out of range");
        }
        return (high, low);
    }
}
