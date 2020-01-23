/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
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
using System.IO;
using Gibbed.IO;
using System.Linq;
using System.Text;

namespace Gibbed.LetUsClingTogether.FileFormats
{
    public static class FileExtensions
    {
        public static string Detect(Stream input, uint size)
        {
            if (size == 0)
            {
                return ".null";
            }

            byte[] guess = new byte[Math.Min(16, size)];
            int read = input.Read(guess, 0, guess.Length);

            if (
                read >= 2 &&
                guess[0] == 'P' &&
                guess[1] == 'K')
            {
                return ".zip";
            }
            else if (
                read >= 4 &&
                guess[0] == 0x89 &&
                guess[1] == 'P' &&
                guess[2] == 'N' &&
                guess[3] == 'G')
            {
                return ".png";
            }
            else if (
                read >= 4 &&
                guess[0] == 'p' &&
                guess[1] == 'a' &&
                guess[2] == 'k' &&
                guess[3] == 'd')
            {
                return ".pack";
            }
            else if (
                read >= 4 &&
                guess[0] == 'P' &&
                guess[1] == 'S' &&
                guess[2] == 'M' &&
                guess[3] == 'F')
            {
                return ".pmf";
            }
            else if (
                read >= 3 &&
                guess[0] == 'O' &&
                guess[1] == 'M' &&
                guess[2] == 'G')
            {
                return ".gmo";
            }
            else if (
                read >= 3 &&
                guess[0] == 'S' &&
                guess[1] == 'A' &&
                guess[2] == 'S')
            {
                return ".spriteanim";
            }
            else if (
                read >= 4 &&
                guess[0] == 'T' &&
                guess[1] == 'C' &&
                guess[2] == '0' &&
                guess[3] == '1')
            {
                return ".tc01";
            }
            else if (
                read >= 3 &&
                guess[0] == 'E' &&
                guess[1] == 'F' &&
                guess[2] == 'X')
            {
                return ".efx";
            }
            else if (
                read >= 4 &&
                guess[0] == 'x' &&
                guess[1] == 'l' &&
                guess[2] == 'c' &&
                guess[3] == 'e')
            {
                return ".eclx";
            }
            else if (
                read >= 4 &&
                guess[0] == 'a' &&
                guess[1] == 's' &&
                guess[2] == 'h' &&
                guess[3] == 'g')
            {
                return ".ashg";
            }
            else if (
                read >= 8 &&
                guess[0] == 'S' &&
                guess[1] == 'E' &&
                guess[2] == 'D' &&
                guess[3] == 'B' &&
                guess[4] == 'S' &&
                guess[5] == 'S' &&
                guess[6] == 'C' &&
                guess[7] == 'F')
            {
                return ".scd";
            }
            else if (
                read >= 16 &&
                BitConverter.ToUInt32(guess, 0x00) == 0 &&
                BitConverter.ToUInt32(guess, 0x04) <= size &&
                BitConverter.ToUInt32(guess, 0x08) == 0x00100001 &&
                BitConverter.ToUInt32(guess, 0x0C) == 3)
            {
                return ".sprite";
            }

            //return null;
            return ".unknown";
        }
    }
}
