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
using System.Collections.Generic;
using System.Text;

namespace Gibbed.LetUsClingTogether.FileFormats.Text
{
    public abstract partial class BaseEncoding : Encoding
    {
        private readonly Dictionary<char, ushort> _UnicodeToEncodedCodepoint;
        private readonly Dictionary<ushort, char> _EncodedCodepointToUnicode;

        internal BaseEncoding(UnicodeRange[] ranges)
        {
            if (ranges == null)
            {
                throw new ArgumentNullException(nameof(ranges));
            }

            // count total codepoints
            int count = 0;
            foreach (var range in ranges)
            {
                count += (range.End + 1) - range.Start;
            }

            // build mapping tables
            this._UnicodeToEncodedCodepoint = new(count);
            this._EncodedCodepointToUnicode = new(count);
            ushort codepoint = 0;
            for (int i = 0; i < ranges.Length; i++)
            {
                var range = ranges[i];
                for (char character = range.Start; character <= range.End; character++, codepoint++)
                {
                    var encodedCodepoint = Encode(codepoint);
                    if (range.Fallback == false)
                    {
                        this._UnicodeToEncodedCodepoint.Add(character, encodedCodepoint);
                    }
                    this._EncodedCodepointToUnicode.Add(encodedCodepoint, character);
                }
            }
        }

        private static ushort Encode(ushort codepoint)
        {
            ushort encodedCodepoint;
            if (codepoint <= 0xDF)
            {
                return (ushort)((codepoint + 0x20) & 0xFF);
            }
            codepoint -= 0xE0;
            encodedCodepoint = (ushort)(((((codepoint / 0xFF) & 0xFF) + 1) & 0xFF) << 8);
            encodedCodepoint |= (ushort)(((codepoint % 0xFF) + 1) & 0xFF);
            return encodedCodepoint;
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (chars.Length - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(chars));
            }

            if (chars.Length == 0)
            {
                return 0;
            }

            int length = index + count;
            int result = 0;
            for (int i = index; i < length; i++)
            {
                var ch = chars[i];

                if (this._UnicodeToEncodedCodepoint.TryGetValue(ch, out var encodedCodepoint) == false)
                {
                    throw new EncoderFallbackException();
                }

                if (encodedCodepoint < 0x100)
                {
                    result++;
                }
                else
                {
                    result += 2;
                }
            }
            return result;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (charIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(charIndex));
            }

            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(charCount));
            }

            if (chars.Length - charIndex < charCount)
            {
                throw new ArgumentOutOfRangeException(nameof(chars));
            }

            if (byteIndex < 0 || byteIndex > bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(byteIndex));
            }

            int charLength = charIndex + charCount;
            int o = byteIndex;
            for (int i = charIndex; i < charLength; i++)
            {
                char ch = chars[i];

                if (this._UnicodeToEncodedCodepoint.TryGetValue(ch, out var encodedCodepoint) == false)
                {
                    throw new EncoderFallbackException();
                }

                if (encodedCodepoint < 0x100)
                {
                    bytes[o++] = (byte)encodedCodepoint;
                }
                else
                {
                    bytes[o++] = (byte)(encodedCodepoint >> 8);
                    bytes[o++] = (byte)encodedCodepoint;
                }
            }
            return o - byteIndex;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (bytes.Length - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes));
            }

            var length = index + count;
            int charCount = 0;
            for (int i = index; i < length;)
            {
                var b = bytes[i++];
                if ((b & 0xE0) == 0)
                {
                    i++;
                }
                charCount++;
            }
            return charCount;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            if (byteIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteIndex));
            }

            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteCount));
            }

            if (bytes.Length - byteIndex < byteCount)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes));
            }

            if (charIndex < 0 || charIndex > chars.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(charIndex));
            }

            var byteLength = byteIndex + byteCount;
            int o = charIndex;
            for (int i = byteIndex; i < byteLength;)
            {
                ushort encodedCodepoint = bytes[i++];
                if ((encodedCodepoint & 0xE0) == 0)
                {
                    encodedCodepoint <<= 8;
                    encodedCodepoint |= bytes[i++];
                }

                if (this._EncodedCodepointToUnicode.TryGetValue(encodedCodepoint, out var ch) == false)
                {
                    throw new EncoderFallbackException();
                }

                chars[o++] = ch;
            }
            return o - charIndex;
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(charCount));
            }

            return charCount * 2;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteCount));
            }

            return byteCount;
        }
    }
}
