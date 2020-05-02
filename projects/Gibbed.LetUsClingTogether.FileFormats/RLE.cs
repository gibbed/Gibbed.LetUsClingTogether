using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.LetUsClingTogether.FileFormats
{
    public static class RLE
    {
        public static byte[] Decompress(byte[] inputBuffer, int inputIndex, int inputCount)
        {
            var uncompressedSize = BitConverter.ToInt32(inputBuffer, inputIndex + 8);
            var outputBuffer = new byte[uncompressedSize];
            Decompress(inputBuffer, inputIndex, inputCount, outputBuffer, 0);
            return outputBuffer;
        }

        public static void Decompress(byte[] inputBuffer, int inputIndex, int inputCount, byte[] outputBuffer, int outputIndex)
        {
            var compressedSize = BitConverter.ToInt32(inputBuffer, inputIndex + 4);
            var uncompressedSize = BitConverter.ToInt32(inputBuffer, inputIndex + 8);
            var version = inputBuffer[inputIndex + 12];

            if (version == 0)
            {
                Array.Copy(inputBuffer, inputIndex + 16, outputBuffer, outputIndex, uncompressedSize);
            }
            else if (version == 1)
            {
                throw new NotImplementedException();
            }
            else if (version == 2)
            {
                Decompress2(inputBuffer, inputIndex, inputCount, outputBuffer, outputIndex);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private static void Decompress2(byte[] inputBuffer, int inputIndex, int inputCount, byte[] outputBuffer, int outputIndex)
        {
            var uncompressedSize = BitConverter.ToInt32(inputBuffer, inputIndex + 8);
            var windowSize = inputBuffer[inputIndex + 13] & 0x1F;

            inputIndex += 16;
            var outputEnd = outputIndex + uncompressedSize;
            while (outputIndex < outputEnd)
            {
                var op = inputBuffer[inputIndex++];
                if ((op & 0x80) == 0)
                {
                    var length = op;
                    Array.Copy(inputBuffer, inputIndex, outputBuffer, outputIndex, length);
                    inputIndex += length;
                    outputIndex += length;
                }
                else
                {
                    var value = ((op & 0x7F) << 8) | inputBuffer[inputIndex++];
                    var offset = 1 + (value & ((1 << windowSize) - 1));
                    var length = 3 + (value >> windowSize);
                    //Array.Copy(outputBuffer, outputIndex - offset, outputBuffer, outputIndex, length);
                    for (int i = 0, o = outputIndex; i < length; i++, o++)
                    {
                        outputBuffer[o] = outputBuffer[o - offset];
                    }
                    outputIndex += length;
                }
            }

            if (outputIndex != outputEnd)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
