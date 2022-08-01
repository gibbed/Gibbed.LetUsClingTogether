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

namespace Gibbed.LetUsClingTogether.FileFormats.Text
{
    public class EnglishEncoding : BaseEncoding
    {
        public EnglishEncoding()
            : base(GetRanges())
        {
        }

        private static UnicodeRange[] GetRanges()
        {
            return new UnicodeRange[]
            {
                ('\u0020', '\u007E'),
                ('\u00C0', '\u00C2'),
                ('\u00C4', '\u00C4'),
                ('\u00C6', '\u00CF'),
                ('\u00D1', '\u00D6'),
                ('\u0152', '\u0152'),
                ('\u00D9', '\u00DC'),
                ('\u00DF', '\u00E2'),
                ('\u00E4', '\u00E4'),
                ('\u00E6', '\u00EF'),
                ('\u00F1', '\u00F6'),
                ('\u0153', '\u0153'),
                ('\u00F9', '\u00FC'),
                ('\u00DF', '\u00DF', true),
                ('\u00BF', '\u00BF'),
                ('\u00A1', '\u00A1'),
                ('\u201A', '\u201A'),
                ('\u201E', '\u201E'),
                ('\u2018', '\u2019'),
                ('\u201C', '\u201D'),
                ('\u2026', '\u2026'),
                ('\u2014', '\u2014'),
                ('\u00AB', '\u00AB'),
                ('\u00BB', '\u00BB'),
                ('\u2190', '\u2193'),
                ('\u00D7', '\u00D7'),
                ('\u00A3', '\u00A3'),
                ('\u20AC', '\u20AC'),
                ('\u00A7', '\u00A7'),
                ('\u00A2', '\u00A2'),
                ('\u00A8', '\u00A8'),
                ('\u00B4', '\u00B4'),
                ('\u2013', '\u2013'),
                ('\u2015', '\u2015'),
                ('\u00B9', '\u00B9'),
                ('\u00B2', '\u00B3'),
                ('\u2074', '\u2075'),
                ('\u00A9', '\u00A9'),
                ('\u00AE', '\u00AE'),
                ('\u2122', '\u2122'),
                ('\u00AA', '\u00AA'),
                ('\u00BA', '\u00BA'),
                ('\u00B0', '\u00B0'),
                ('\u2260', '\u2260'),
                ('\u2264', '\u2265'),
                ('\u25A0', '\u25A1'),
                ('\u25CB', '\u25CB'),
                ('\u25CF', '\u25CF'),
                ('\u2605', '\u2606'),
                ('\u2665', '\u2665'),
                ('\u266A', '\u266A'),
                ('\uFF5E', '\uFF5E'),
                ('\u00B1', '\u00B1'),
                ('\u00F7', '\u00F7'),
                ('\u2642', '\u2642'),
                ('\u2640', '\u2640'),
                ('\u03A3', '\u03A3'),
                ('\u2282', '\u2283'),
                ('\u2200', '\u2200'),
                ('\u0434', '\u0434'),
                ('\u03C9', '\u03C9'),
                ('\u25BC', '\u25BC'),
                ('\u25B3', '\u25B3'),
                ('\u0391', '\u039C'),
                ('\u03B9', '\u03B9'),
                ('\u0020', '\u0020', true),
                ('\u0020', '\u0020', true),
                ('\u0020', '\u0020', true),
                ('\u0020', '\u0020', true),
                ('\u2460', '\u246A'),
                ('\u246D', '\u2477'),
                ('\u03BA', '\u03C1'),
                ('\u03C3', '\u03C3'),
                ('\u3351', '\u3351'),
                ('\u2030', '\u2030'),
                ('\u2160', '\u2169'),
            };
        }
    }
}
