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
using System.IO;
using Gibbed.IO;

namespace Gibbed.LetUsClingTogether.FileFormats
{
    public static class FileDetection
    {
        public const int BestGuessLength = 16;

        public static string Guess(Stream input, int length)
        {
            var guessSize = Math.Min(length, BestGuessLength);
            var guessBytes = input.ReadBytes(guessSize);
            return Guess(guessBytes, 0, guessSize);
        }

        public static string Guess(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0 || index > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0 || index + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count == 0)
            {
                return ".null";
            }

            if (count >= 2 &&
                buffer[index + 0] == 'P' &&
                buffer[index + 1] == 'K')
            {
                return ".zip";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 0x89 &&
                buffer[index + 1] == 'P' &&
                buffer[index + 2] == 'N' &&
                buffer[index + 3] == 'G')
            {
                return ".png";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'p' &&
                buffer[index + 1] == 'a' &&
                buffer[index + 2] == 'k' &&
                buffer[index + 3] == 'd')
            {
                return ".pack";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'P' &&
                buffer[index + 1] == 'S' &&
                buffer[index + 2] == 'M' &&
                buffer[index + 3] == 'F')
            {
                return ".pmf";
            }
            else if (
                count >= 3 &&
                buffer[index + 0] == 'O' &&
                buffer[index + 1] == 'M' &&
                buffer[index + 2] == 'G')
            {
                return ".gmo";
            }
            else if (
                count >= 3 &&
                buffer[index + 0] == 'S' &&
                buffer[index + 1] == 'A' &&
                buffer[index + 2] == 'S')
            {
                return ".spriteanim";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'T' &&
                buffer[index + 1] == 'C' &&
                buffer[index + 2] == '0' &&
                buffer[index + 3] == '1')
            {
                return ".tc01";
            }
            else if (
                count >= 3 &&
                buffer[index + 0] == 'E' &&
                buffer[index + 1] == 'F' &&
                buffer[index + 2] == 'X')
            {
                return ".efx";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'x' &&
                buffer[index + 1] == 'l' &&
                buffer[index + 2] == 'c' &&
                buffer[index + 3] == 'e')
            {
                return ".eclx";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'a' &&
                buffer[index + 1] == 's' &&
                buffer[index + 2] == 'h' &&
                buffer[index + 3] == 'g')
            {
                return ".ashg";
            }
            else if (
                count >= 8 &&
                buffer[index + 0] == 'S' &&
                buffer[index + 1] == 'E' &&
                buffer[index + 2] == 'D' &&
                buffer[index + 3] == 'B' &&
                buffer[index + 4] == 'S' &&
                buffer[index + 5] == 'S' &&
                buffer[index + 6] == 'C' &&
                buffer[index + 7] == 'F')
            {
                return ".scd";
            }
            else if (
                count >= 16 &&
                BitConverter.ToUInt32(buffer, index + 0x0) == 0 &&
                BitConverter.ToUInt32(buffer, index + 0x4) <= count &&
                BitConverter.ToUInt32(buffer, index + 0x8) == 0x00100001 &&
                BitConverter.ToUInt32(buffer, index + 0xC) == 3)
            {
                return ".sprite";
            }

            return ".unknown";
        }
    }
}
