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

public readonly record struct Command
{
    public ushort Tick { get; init; }
    public byte RawOpcode { get; init; }
    public byte RawArg0 { get; init; }
    public byte RawArg1 { get; init; }
    public byte VFlipByte { get; init; }
    public sbyte Priority { get; init; }
    public short X { get; init; }
    public short Y { get; init; }
    public short Z { get; init; }

    public Command(
        ushort tick,
        byte rawOpcode,
        byte rawArg0, byte rawArg1,
        byte vflipByte,
        sbyte priority,
        short x, short y, short z)
    {
        this.Tick = tick;
        this.RawOpcode = rawOpcode;
        this.RawArg0 = rawArg0;
        this.RawArg1 = rawArg1;
        this.VFlipByte = vflipByte;
        this.Priority = priority;
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public Opcode Opcode => (Opcode)this.RawOpcode;

    public byte FrameIndex
    {
        get => this.RawArg0;
        init => this.RawArg0 = value;
    }

    public bool HFlip
    {
        get => this.RawArg1 != 0;
        init => this.RawArg1 = (byte)(value == true ? 1 : 0);
    }

    public bool VFlip
    {
        get => this.VFlipByte != 0;
        init => this.VFlipByte = (byte)(value == true ? 1 : 0);
    }

    public ushort ArgU16
    {
        get => (ushort)(this.RawArg0 | (this.RawArg1 << 8));
        init
        {
            this.RawArg0 = (byte)value;
            this.RawArg1 = (byte)(value >> 8);
        }
    }

    public sbyte ArgS8
    {
        get => unchecked((sbyte)this.RawArg0);
        init => this.RawArg0 = unchecked((byte)value);
    }

    internal static int GetSize(FileVersion version) => version == FileVersion.V1 ? 12 : 14;

    internal static Command Read(ReadOnlySpan<byte> span, ref int index, FileVersion version, Endian endian) => version switch
    {
        FileVersion.V1 => ReadV1(span, ref index, endian),
        FileVersion.V2 => throw new SequenceFormatException($"unsupported file version"),
        FileVersion.V3 => ReadV3(span, ref index, endian),
        _ => throw new ArgumentOutOfRangeException(nameof(version), version, "unknown file version"),
    };

    internal static Command ReadV1(ReadOnlySpan<byte> span, ref int index, Endian endian)
    {
        var start = index;
        var tick = span.ReadValueU16(ref index, endian);
        var rawOpcode = span.ReadValueU8(ref index);
        if (Enum.IsDefined(typeof(Opcode), rawOpcode) == false)
        {
            throw new SequenceFormatException($"command at 0x{start:X} has unknown opcode {rawOpcode}");
        }
        Helpers.ReadPadding(span, ref index, 1);
        var hasXY = ((Opcode)rawOpcode).HasXY();
        var rawArg0 = span.ReadValueU8(ref index);
        var rawArg1OrX = span.ReadValueU8(ref index);
        var y = span.ReadValueS8(ref index);
        var z = span.ReadValueS8(ref index);
        byte rawArg1;
        if (hasXY == true)
        {
            rawArg1 = span.ReadValueU8(ref index);
        }
        else
        {
            rawArg1 = rawArg1OrX;
            Helpers.ReadPadding(span, ref index, 1);
        }
        var vflipByte = span.ReadValueU8(ref index);
        var priority = span.ReadValueS8(ref index);
        Helpers.ReadPadding(span, ref index, 1);
        var x = hasXY == true
            ? unchecked((sbyte)rawArg1OrX)
            : (short)0;
        return new(tick, rawOpcode, rawArg0, rawArg1, vflipByte, priority, x, y, z);
    }

    internal static Command ReadV3(ReadOnlySpan<byte> span, ref int index, Endian endian)
    {
        var start = index;
        var tick = span.ReadValueU16(ref index, endian);
        var opcode = span.ReadValueU8(ref index);
        if (Enum.IsDefined(typeof(Opcode), opcode) == false)
        {
            throw new SequenceFormatException($"command at 0x{start:X} has unknown opcode {opcode}");
        }
        Helpers.ReadPadding(span, ref index, 1);
        var rawArg0 = span.ReadValueU8(ref index);
        var rawArg1 = span.ReadValueU8(ref index);
        var vflipByte = span.ReadValueU8(ref index);
        var priority = span.ReadValueS8(ref index);
        var x = span.ReadValueS16(ref index, endian);
        var y = span.ReadValueS16(ref index, endian);
        var z = span.ReadValueS16(ref index, endian);
        return new(tick, opcode, rawArg0, rawArg1, vflipByte, priority, x, y, z);
    }

    internal void WriteV1(IByteWriter writer, Endian endian)
    {
        if (this.X is < sbyte.MinValue or > sbyte.MaxValue ||
            this.Y is < sbyte.MinValue or > sbyte.MaxValue ||
            this.Z is < sbyte.MinValue or > sbyte.MaxValue)
        {
            throw new SequenceFormatException($"command ({this.X}, {this.Y}, {this.Z}) is too large for V1");
        }
        var hasXY = this.Opcode.HasXY();
        if (hasXY == false && this.X != 0)
        {
            throw new SequenceFormatException($"command X is {this.X} but V1 does not have X for opcode {this.Opcode}");
        }
        writer.WriteValueU16(this.Tick, endian);
        writer.WriteValueU8(this.RawOpcode);
        writer.WriteValueU8(0);
        writer.WriteValueU8(this.RawArg0);
        writer.WriteValueU8(hasXY == true ? unchecked((byte)(sbyte)this.X) : this.RawArg1);
        writer.WriteValueS8((sbyte)this.Y);
        writer.WriteValueS8((sbyte)this.Z);
        writer.WriteValueU8(hasXY == true ? this.RawArg1 : (byte)0);
        writer.WriteValueU8(this.VFlipByte);
        writer.WriteValueS8(this.Priority);
        writer.WriteValueU8(0);
    }

    internal void WriteV3(IByteWriter writer, Endian endian)
    {
        writer.WriteValueU16(this.Tick, endian);
        writer.WriteValueU8(this.RawOpcode);
        writer.WriteValueU8(0);
        writer.WriteValueU8(this.RawArg0);
        writer.WriteValueU8(this.RawArg1);
        writer.WriteValueU8(this.VFlipByte);
        writer.WriteValueS8(this.Priority);
        writer.WriteValueS16(this.X, endian);
        writer.WriteValueS16(this.Y, endian);
        writer.WriteValueS16(this.Z, endian);
    }
}
