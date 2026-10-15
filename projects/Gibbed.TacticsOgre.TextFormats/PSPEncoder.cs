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

public class PSPEncoder : BaseEncoder
{
    private readonly Encodings.BaseEncoding _Encoding;
    private readonly Dialect _Dialect;

    public PSPEncoder(Encodings.BaseEncoding encoding)
        : this(encoding, DialectHelpers.FromEncodingPSP(encoding))
    {
    }

    internal PSPEncoder(Encodings.BaseEncoding encoding, Dialect dialect)
    {
        this._Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        if (dialect == Dialect.Reborn)
        {
            throw new ArgumentOutOfRangeException(nameof(dialect));
        }
        this._Dialect = dialect;
    }

    internal override Dialect Dialect => this._Dialect;

    protected override void WriteText(string text, Stream output)
    {
        foreach (var c in text)
        {
            byte[] bytes;
            try
            {
                bytes = this._Encoding.GetBytes(new[] { c });
            }
            catch (EncoderFallbackException)
            {
                throw new FormatException($"'{c}' (U+{(int)c:X4}) is not in the PSP font");
            }
            output.Write(bytes, 0, bytes.Length);
        }
    }

    protected override void WriteMacroPrefix(Stream output)
    {
        output.WriteByte(0xFF);
    }

    protected override void WriteEscape(byte escape, Stream output)
    {
        output.WriteByte(escape);
    }

    protected override void WriteGaiji(uint codepoint, Stream output)
    {
        if (codepoint > 0xFFFF)
        {
            throw new FormatException($"gaiji 0x{codepoint:X} is out of range");
        }
        this.WriteText(((char)codepoint).ToString(), output);
    }

    public static PSPEncoder ForEN()
    {
        return new(new Encodings.EnglishEncoding());
    }

    public static PSPEncoder ForJP()
    {
        return new(new Encodings.JapaneseEncoding());
    }
}
