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
using System.Collections.Generic;
using System.Linq;
using Gibbed.Memory;
using IByteWriter = System.Buffers.IBufferWriter<byte>;

namespace Gibbed.TacticsOgre.SequenceFormats;

public sealed class Sequence
{
    public ushort Length { get; set; }
    public List<Command> Commands { get; } = [];
    public List<Command> ExtraCommands { get; } = [];

    public const int MaxCommands = 127;

    internal static Sequence Read(ReadOnlySpan<byte> span, uint offset, FileVersion version, Endian endian) => version switch
    {
        FileVersion.V1 => ReadV1(span, (int)offset, endian),
        FileVersion.V2 => throw new NotImplementedException("unsupported file version"),
        FileVersion.V3 => ReadV3(span, (int)offset, endian),
        _ => throw new ArgumentOutOfRangeException(nameof(version), version, "unknown file version"),
    };

    internal static Sequence ReadV1(ReadOnlySpan<byte> span, int index, Endian endian)
    {
        var start = index;
        var commandCount = span.ReadValueS8(ref index);
        if (commandCount < 0)
        {
            throw new SequenceFormatException($"sequence at 0x{start:X} has a negative command count ({commandCount})");
        }
        Sequence instance = new();
        Helpers.ReadPadding(span, ref index, 1);
        instance.Length = span.ReadValueU16(ref index, endian);
        for (int i = 0; i < commandCount; i++)
        {
            instance.Commands.Add(Command.ReadV1(span, ref index, endian));
        }
        return instance;
    }

    internal static Sequence ReadV3(ReadOnlySpan<byte> span, int index, Endian endian)
    {
        var start = index;
        var commandCount = span.ReadValueS8(ref index);
        if (commandCount < 0)
        {
            throw new SequenceFormatException($"sequence at 0x{start:X} has a negative command count ({commandCount})");
        }
        Sequence instance = new();
        Helpers.ReadPadding(span, ref index, 1);
        instance.Length = span.ReadValueU16(ref index, endian);
        for (int i = 0; i < commandCount; i++)
        {
            instance.Commands.Add(Command.ReadV3(span, ref index, endian));
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
        writer.WriteValueS8((sbyte)this.Commands.Count);
        writer.WriteValueU8(0);
        writer.WriteValueU16(this.Length, endian);
        foreach (var command in this.Commands.Concat(this.ExtraCommands))
        {
            command.WriteV1(writer, endian);
        }
    }

    internal void WriteV3(IByteWriter writer, Endian endian)
    {
        writer.WriteValueS8((sbyte)this.Commands.Count);
        writer.WriteValueU8(0);
        writer.WriteValueU16(this.Length, endian);
        foreach (var command in this.Commands.Concat(this.ExtraCommands))
        {
            command.WriteV3(writer, endian);
        }
    }

    public Sequence Clone()
    {
        Sequence clone = new()
        {
            Length = this.Length,
        };
        clone.Commands.AddRange(this.Commands);
        clone.ExtraCommands.AddRange(this.ExtraCommands);
        return clone;
    }
}
