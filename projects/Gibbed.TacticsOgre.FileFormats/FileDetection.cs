/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.TacticsOgre.FileFormats
{
    public static class FileDetection
    {
        public const int BestGuessLength = 16;

        public static string Guess(Stream input, int length, long fileSize)
        {
            var guessSize = Math.Min(length, BestGuessLength);
            var guessBytes = input.ReadBytes(guessSize);
            return Guess(guessBytes, 0, guessSize, fileSize);
        }

        public static string Guess(byte[] buffer, int index, int count, long fileSize)
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
                return ".pakd"; // probably "packed archive"
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'A' &&
                buffer[index + 1] == 'R' &&
                buffer[index + 2] == 'C' &&
                buffer[index + 3] == 0)
            {
                return ".pac"; // Reborn UI archive
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
               buffer[index + 0] == 'b' &&
               buffer[index + 1] == 't' &&
               buffer[index + 2] == 'x')
            {
                return ".tex"; // Reborn texture
            }
            else if (
                count >= 3 &&
                buffer[index + 0] == 'S' &&
                buffer[index + 1] == 'A' &&
                buffer[index + 2] == 'S')
            {
                return ".sas";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 's' &&
                buffer[index + 1] == 'a' &&
                buffer[index + 2] == 'b' &&
                buffer[index + 3] == 'f')
            {
                return ".sab";
            }
            else if (
              count >= 4 &&
              buffer[index + 0] == 't' &&
              buffer[index + 1] == 'x' &&
              buffer[index + 2] == 't' &&
              buffer[index + 3] == 'e')
            {
                return ".etxt"; // Reborn event text
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
                buffer[index + 0] == 'E' &&
                buffer[index + 1] == 'M' &&
                buffer[index + 2] == 'E' &&
                buffer[index + 3] == 'S')
            {
                return ".emes";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'I' &&
                buffer[index + 1] == 'D' &&
                buffer[index + 2] == 'X' &&
                buffer[index + 3] == '0')
            {
                return ".idx";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'P' &&
                buffer[index + 1] == 'G' &&
                buffer[index + 2] == 'R' &&
                buffer[index + 3] == 'S')
            {
                return ".pgrs";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'T' &&
                buffer[index + 1] == 'A' &&
                buffer[index + 2] == 'S' &&
                buffer[index + 3] == 'K')
            {
                return ".task";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'I' &&
                buffer[index + 1] == 'N' &&
                buffer[index + 2] == 'V' &&
                buffer[index + 3] == 'K')
            {
                return ".invk";
            }
            else if (
                count >= 4 &&
                buffer[index + 0] == 'x' &&
                buffer[index + 1] == 'l' &&
                buffer[index + 2] == 'c')
            {
                return ".xlc";
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
                BitConverter.ToUInt32(buffer, index + 0x4) + index <= fileSize &&
                BitConverter.ToUInt32(buffer, index + 0x8) == 0x00100001 &&
                BitConverter.ToUInt32(buffer, index + 0xC) == 3)
            {
                return ".img";
            }
            else if (
                count >= 4 &&
                fileSize >= 100 &&
                BitConverter.ToUInt32(buffer, index + 0x0) == 0x8000000C)
            {
                return ".script";
            }

            return ".unknown";
        }
    }
}
