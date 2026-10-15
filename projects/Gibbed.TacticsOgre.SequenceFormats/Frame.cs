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
using System.Collections.Generic;
using Gibbed.Memory;
using IByteWriter = System.Buffers.IBufferWriter<byte>;

namespace Gibbed.TacticsOgre.SequenceFormats;

public sealed class Frame
{
    public byte SpritesheetIndex { get; set; }
    public short CenterX { get; set; }
    public short CenterY { get; set; }
    public List<Cell> Cells { get; } = new();

    public const int MaxCells = 255;

    internal static Frame Read(ReadOnlySpan<byte> span, uint offset, FileVersion version, Endian endian) => version switch
    {
        FileVersion.V1 => ReadV1(span, (int)offset, endian),
        FileVersion.V2 => throw new NotImplementedException("unsupported file version"),
        FileVersion.V3 => ReadV3(span, (int)offset, endian),
        _ => throw new ArgumentOutOfRangeException(nameof(version), version, "unknown file version"),
    };

    internal static Frame ReadV1(ReadOnlySpan<byte> span, int index, Endian endian)
    {
        Frame instance = new();
        instance.SpritesheetIndex = span.ReadValueU8(ref index);
        instance.CenterX = span.ReadValueS8(ref index);
        instance.CenterY = span.ReadValueS8(ref index);
        var cellCount = span.ReadValueU8(ref index);
        for (int i = 0; i < cellCount; i++)
        {
            instance.Cells.Add(Cell.ReadV1(span, ref index, endian));
        }
        return instance;
    }

    internal static Frame ReadV3(ReadOnlySpan<byte> span, int index, Endian endian)
    {
        Frame instance = new();
        instance.SpritesheetIndex = span.ReadValueU8(ref index);
        var cellCount = span.ReadValueU8(ref index);
        instance.CenterX = span.ReadValueS16(ref index, endian);
        instance.CenterY = span.ReadValueS16(ref index, endian);
        for (int i = 0; i < cellCount; i++)
        {
            instance.Cells.Add(Cell.ReadV3(span, ref index, endian));
        }
        return instance;
    }

    internal void Write(IByteWriter writer, FileVersion version, Endian endian)
    {
        switch (version)
        {
            case FileVersion.V1:
            {
                this.WriteV1(writer, endian);
                break;
            }
            case FileVersion.V2:
            {
                throw new NotImplementedException("unsupported file version");
            }
            case FileVersion.V3:
            {
                this.WriteV3(writer, endian);
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(version), version, "unknown file version");
            }
        }
    }

    internal void WriteV1(IByteWriter writer, Endian endian)
    {
        if (this.CenterX is < sbyte.MinValue or > sbyte.MaxValue ||
            this.CenterY is < sbyte.MinValue or > sbyte.MaxValue)
        {
            throw new SequenceFormatException($"Frame center ({this.CenterX}, {this.CenterY}) doesn't fit SAS version 1, which stores it as two s8s.");
        }
        writer.WriteValueU8(this.SpritesheetIndex);
        writer.WriteValueS8((sbyte)this.CenterX);
        writer.WriteValueS8((sbyte)this.CenterY);
        writer.WriteValueU8((byte)this.Cells.Count);
        foreach (var cell in this.Cells)
        {
            cell.WriteV1(writer, endian);
        }
    }

    internal void WriteV3(IByteWriter writer, Endian endian)
    {
        writer.WriteValueU8(this.SpritesheetIndex);
        writer.WriteValueU8((byte)this.Cells.Count);
        writer.WriteValueS16(this.CenterX, endian);
        writer.WriteValueS16(this.CenterY, endian);
        foreach (var cell in this.Cells)
        {
            cell.WriteV3(writer, endian);
        }
    }

    public Frame Clone()
    {
        Frame clone = new()
        {
            SpritesheetIndex = this.SpritesheetIndex,
            CenterX = this.CenterX,
            CenterY = this.CenterY,
        };
        foreach (var p in this.Cells)
        {
            clone.Cells.Add(p.Clone());
        }
        return clone;
    }
}
