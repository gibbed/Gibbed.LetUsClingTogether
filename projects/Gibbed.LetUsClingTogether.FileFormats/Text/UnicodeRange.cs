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

namespace Gibbed.LetUsClingTogether.FileFormats.Text
{
    internal struct UnicodeRange : IEquatable<UnicodeRange>
    {
        public readonly char Start;
        public readonly char End;
        public readonly bool Fallback;

        public UnicodeRange(char start, char end)
            : this(start, end, false)
        {
        }

        public UnicodeRange(char start, char end, bool fallback)
        {
            if (start != '\0' && end != '\0')
            {
                if (start < '\u0020')
                {
                    throw new ArgumentOutOfRangeException(nameof(start));
                }

                if (end < start)
                {
                    throw new ArgumentOutOfRangeException(nameof(end));
                }
            }

            this.Start = start;
            this.End = end;
            this.Fallback = fallback;
        }

        #region Deconstruct & tuple operators
        public void Deconstruct(out char start, out char end)
        {
            start = this.Start;
            end = this.End;
        }

        public void Deconstruct(out char start, out char end, out bool fallback)
        {
            start = this.Start;
            end = this.End;
            fallback = this.Fallback;
        }

        public static implicit operator (char start, char end)(UnicodeRange value)
        {
            return (value.Start, value.End);
        }

        public static implicit operator UnicodeRange((char start, char end) value)
        {
            return new UnicodeRange(value.start, value.end);
        }

        public static implicit operator (char start, char end, bool fallback)(UnicodeRange value)
        {
            return (value.Start, value.End, value.Fallback);
        }

        public static implicit operator UnicodeRange((char start, char end, bool fallback) value)
        {
            return new UnicodeRange(value.start, value.end, value.fallback);
        }
        #endregion
        #region Equals, IEquatable & equality operators
        public override bool Equals(object obj)
        {
            return obj is UnicodeRange range && this.Equals(range) == true;
        }

        public bool Equals(UnicodeRange other)
        {
            return this.Start == other.Start &&
                   this.End == other.End &&
                   this.Fallback == other.Fallback;
        }

        public override int GetHashCode()
        {
            int hashCode = -46522716;
            hashCode = hashCode * -1521134295 + this.Start.GetHashCode();
            hashCode = hashCode * -1521134295 + this.End.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Fallback.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(UnicodeRange left, UnicodeRange right)
        {
            return left.Equals(right) == true;
        }

        public static bool operator !=(UnicodeRange left, UnicodeRange right)
        {
            return left.Equals(right) == false;
        }
        #endregion
    }
}
