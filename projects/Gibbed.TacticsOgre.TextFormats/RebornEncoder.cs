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
using System.IO;
using System.Text;

namespace Gibbed.TacticsOgre.TextFormats;

public class RebornEncoder : BaseEncoder
{
    internal override Dialect Dialect => Dialect.Reborn;

    protected override void WriteText(string text, Stream output)
    {
        foreach (var rune in text.EnumerateRunes())
        {
            if (rune.Value >= 0xF8FC && rune.Value <= 0xF8FF)
            {
                throw new FormatException($"U+{rune.Value:X4} is reserved for control codes");
            }
        }
        var bytes = Encoding.UTF8.GetBytes(text);
        output.Write(bytes, 0, bytes.Length);
    }

    protected override void WriteMacroPrefix(Stream output)
    {
        output.Write([0xEF, 0xA3, 0xBF], 0, 3);
    }

    protected override void WriteEscape(byte escape, Stream output)
    {
        // FC-FF become U+F8FC-U+F8FF
        output.Write([0xEF, 0xA3, (byte)(escape - 0x40)], 0, 3);
    }

    protected override void WriteGaiji(uint codepoint, Stream output)
    {
        var bytes = Encoding.UTF8.GetBytes(char.ConvertFromUtf32((int)codepoint));
        output.Write(bytes, 0, bytes.Length);
    }
}
