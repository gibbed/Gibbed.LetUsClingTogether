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
using Gibbed.Memory;
using IByteWriter = System.Buffers.IBufferWriter<byte>;

namespace Gibbed.TacticsOgre.SequenceFormats;

public sealed class Cell
{
    public short X { get; set; }
    public short Y { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public ushort U { get; set; }
    public ushort V { get; set; }

    internal static int GetSize(FileVersion version) => version == FileVersion.V1 ? 8 : 12;

    internal static Cell ReadV1(ReadOnlySpan<byte> span, ref int index, Endian endian)
    {
        Cell cell = new();
        cell.X = span.ReadValueS8(ref index);
        cell.Y = span.ReadValueS8(ref index);
        cell.Width = span.ReadValueU8(ref index);
        cell.Height = span.ReadValueU8(ref index);
        cell.U = span.ReadValueU16(ref index, endian);
        cell.V = span.ReadValueU16(ref index, endian);
        return cell;
    }

    internal static Cell ReadV3(ReadOnlySpan<byte> span, ref int index, Endian endian)
    {
        Cell cell = new();
        cell.X = span.ReadValueS16(ref index, endian);
        cell.Y = span.ReadValueS16(ref index, endian);
        cell.Width = span.ReadValueU16(ref index, endian);
        cell.Height = span.ReadValueU16(ref index, endian);
        cell.U = span.ReadValueU16(ref index, endian);
        cell.V = span.ReadValueU16(ref index, endian);
        return cell;
    }

    internal void WriteV1(IByteWriter writer, Endian endian)
    {
        if (this.X is < sbyte.MinValue or > sbyte.MaxValue ||
            this.Y is < sbyte.MinValue or > sbyte.MaxValue ||
            this.Width > byte.MaxValue || this.Height > byte.MaxValue)
        {
            throw new SequenceFormatException($"cell at ({this.X}, {this.Y}) sized {this.Width}x{this.Height} is too large for V1");
        }
        writer.WriteValueS8((sbyte)this.X);
        writer.WriteValueS8((sbyte)this.Y);
        writer.WriteValueU8((byte)this.Width);
        writer.WriteValueU8((byte)this.Height);
        writer.WriteValueU16(this.U, endian);
        writer.WriteValueU16(this.V, endian);
    }

    internal void WriteV3(IByteWriter writer, Endian endian)
    {
        writer.WriteValueS16(this.X, endian);
        writer.WriteValueS16(this.Y, endian);
        writer.WriteValueU16(this.Width, endian);
        writer.WriteValueU16(this.Height, endian);
        writer.WriteValueU16(this.U, endian);
        writer.WriteValueU16(this.V, endian);
    }

    public Cell Clone() => (Cell)this.MemberwiseClone();
}
