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

public sealed class SequenceFile
{
    public const int Directions = 4;

    public FileVersion Version { get; set; } = FileVersion.V3;
    public byte Scale { get; set; } = 1;
    public uint Unknown08 { get; set; }
    public List<Frame> Frames { get; } = [];
    public List<Slot> Slots { get; } = [];

    public int AnimationCount => (this.Slots.Count + Directions - 1) / Directions;

    public Slot GetSlot(int anim, int dir)
    {
        var i = anim * Directions + dir;
        return i < this.Slots.Count
            ? this.Slots[i]
            : Slot.Empty();
    }

    public void EnsureAnimation(int anim)
    {
        while (this.Slots.Count < (anim + 1) * Directions)
        {
            this.Slots.Add(Slot.Empty());
        }
    }

    public IEnumerable<(int anim, int dir)> UsersOf(Sequence sequence)
    {
        for (int i = 0; i < this.Slots.Count; i++)
        {
            if (ReferenceEquals(this.Slots[i].Sequence, sequence) == true)
            {
                yield return (i / Directions, i % Directions);
            }
        }
    }

    public IEnumerable<int> AliasesOfFrame(int index)
    {
        for (int i = 0; i < this.Frames.Count; i++)
        {
            if (i != index && ReferenceEquals(this.Frames[i], this.Frames[index]) == true)
            {
                yield return i;
            }
        }
    }

    public static SequenceFile Read(ReadOnlySpan<byte> span)
    {
        const Endian endian = Endian.Little;

        if (span.Length < 12)
        {
            throw new SequenceFormatException("file is too small for V1 header");
        }

        var index = 0;
        var magic = span.ReadValueU32(ref index, endian);
        var version = Helpers.GetVersion(magic)
            ?? throw new SequenceFormatException($"unknown file magic 0x{magic:X8}");

        SequenceFile instance = new();
        instance.Version = version;

        var hasScale = version is FileVersion.V1 or FileVersion.V2;

        int headerSize;
        if (hasScale == false)
        {
            instance.Scale = 1;
            headerSize = 12;
        }
        else
        {
            if (span.Length < 16)
            {
                throw new SequenceFormatException("File is too small to be SAS3 data.");
            }

            instance.Scale = span.ReadValueU8(ref index);
            Helpers.ReadPadding(span, ref index, 3);
            headerSize = 16;
        }

        var frameTableOffset = span.ReadValueU32(ref index, endian);
        if (frameTableOffset < headerSize || frameTableOffset + 4 > span.Length)
        {
            throw new SequenceFormatException($"frame table offset 0x{frameTableOffset:X} is outside of bounds");
        }

        uint slotCount;
        if (hasScale == true)
        {
            slotCount = span.ReadValueU32(ref index, endian);
        }
        else
        {
            instance.Unknown08 = span.ReadValueU32(ref index, endian);
            if ((frameTableOffset - headerSize) % 4 != 0)
            {
                throw new SequenceFormatException($"frame table offset 0x{frameTableOffset:X} does not end a complete sequence table");
            }
            slotCount = (frameTableOffset - (uint)headerSize) / 4;
        }

        index = (int)frameTableOffset;
        var frameCount = span.ReadValueS32(ref index, endian);
        if (frameCount < 0 || frameTableOffset + 4 + 4L * frameCount > span.Length)
        {
            throw new SequenceFormatException($"frame table claims {frameCount} frames, which is out of bounds");
        }

        var frameOffsets = new uint[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            frameOffsets[i] = span.ReadValueU32(ref index, endian);
        }

        var slotOffsetTableOffsetEnd = headerSize + 4L * slotCount;
        if (slotOffsetTableOffsetEnd > span.Length)
        {
            throw new SequenceFormatException($"header claims {slotCount} sequence table entries, which is out of bounds");
        }

        int slotFlagCount = (int)(slotOffsetTableOffsetEnd - headerSize) >> 2;
        var slotFlagses = new uint[slotFlagCount];

        index = (int)headerSize;
        for (int i = 0; i < slotFlagCount; i++)
        {
            slotFlagses[i] = span.ReadValueU32(ref index, endian);
        }

        Dictionary<uint, Sequence> sequences = [];
        for(int i = 0; i < slotFlagCount; i++)
        {
            var slotFlags = slotFlagses[i];
            
            var mirror = (slotFlags & 1) != 0;
            if (slotFlags < 2)
            {
                Slot emptySlot = new();
                emptySlot.Mirror = mirror;
                instance.Slots.Add(emptySlot);
                continue;
            }

            var slotOffset = slotFlags >> 1;
            if (slotOffset < slotOffsetTableOffsetEnd)
            {
                throw new SequenceFormatException($"sequence table entry {i} points into the table (0x{slotOffset:X})");
            }

            if (sequences.TryGetValue(slotOffset, out var sequence) == false)
            {
                sequences[slotOffset] = sequence = Sequence.Read(span, slotOffset, version, endian);
            }

            Slot slot = new();
            slot.Sequence = sequence;
            slot.Mirror = mirror;
            instance.Slots.Add(slot);
        }

        ReadExtraCommands(span, sequences, version, endian);

        Dictionary<uint, Frame> frames = new();
        foreach (var offset in frameOffsets)
        {
            if (frames.TryGetValue(offset, out var frame) == false)
            {
                frames[offset] = frame = Frame.Read(span, offset, version, endian);
            }
            instance.Frames.Add(frame);
        }

        return instance;
    }

    private static void ReadExtraCommands(
        ReadOnlySpan<byte> span,
        Dictionary<uint, Sequence> sequences,
        FileVersion version,
        Endian endian)
    {
        var commandSize = Command.GetSize(version);
        var offsets = sequences.Keys.OrderBy(o => o).ToList();
        for (int i = 0; i < offsets.Count; i++)
        {
            var offset = offsets[i];
            var sequence = sequences[offset];
            var index = (int)offset + 4 + commandSize * sequence.Commands.Count;
            if (i + 1 < offsets.Count)
            {
                var gapSize = (int)offsets[i + 1] - index;
                if (gapSize < 0)
                {
                    throw new SequenceFormatException($"sequence at 0x{offset:X} overlaps the sequence at 0x{offsets[i + 1]:X}");
                }
                if (gapSize % commandSize != 0)
                {
                    throw new SequenceFormatException($"sequence at 0x{offset:X} is followed by {gapSize} bytes, which is not a complete number of commands");
                }
                while (index < (int)offsets[i + 1])
                {
                    sequence.ExtraCommands.Add(Command.Read(span, ref index, version, endian));
                }
            }
            else
            {
                while (index + commandSize <= span.Length && !Helpers.IsZero(span.Slice(index, commandSize)))
                {
                    sequence.ExtraCommands.Add(Command.Read(span, ref index, version, endian));
                }
            }
        }
    }

    public void Write(IByteWriter writer)
    {
        const Endian endian = Endian.Little;
        var version = this.Version;

        var hasScale = version is not (FileVersion.V1 or FileVersion.V2);

        var headerSize = hasScale ? 0x10 : 0x0C;

        var frameHeaderSize = version == FileVersion.V1 ? 4 : 6;
        var commandSize = Command.GetSize(version);
        var cellSize = Cell.GetSize(version);

        if (hasScale == false && this.Scale != 1)
        {
            throw new SequenceFormatException($"scale is {this.Scale}, but V1 does not support it");
        }

        var sequences = this.Slots
            .Where(s => s.Sequence != null)
            .Select(s => s.Sequence!)
            .Distinct<Sequence>(ReferenceEqualityComparer.Instance)
            .ToList();
        var frames = this.Frames
            .Distinct<Frame>(ReferenceEqualityComparer.Instance)
            .ToList();

        foreach (var sequence in sequences)
        {
            if (sequence.Commands.Count > Sequence.MaxCommands)
            {
                throw new SequenceFormatException($"sequence has {sequence.Commands.Count} commands; exceeds {Sequence.MaxCommands}");
            }

            foreach (var command in sequence.Commands.Concat(sequence.ExtraCommands))
            {
                if (Enum.IsDefined(typeof(Opcode), command.RawOpcode) == false)
                {
                    throw new SequenceFormatException($"command has unknown opcode {command.RawOpcode}");
                }
            }
        }

        foreach (var frame in frames)
        {
            if (frame.Cells.Count > Frame.MaxCells)
            {
                throw new SequenceFormatException($"frame has {frame.Cells.Count} cells; exceeds {Frame.MaxCells}");
            }
        }

        var position = headerSize + 4 * this.Slots.Count;
        var frameTableOffset = (uint)position;
        position += 4 + 4 * this.Frames.Count;

        Dictionary<Frame, uint> frameOffsets = new(ReferenceEqualityComparer.Instance);
        foreach (var frame in frames)
        {
            frameOffsets[frame] = (uint)position;
            position += frameHeaderSize + cellSize * frame.Cells.Count;
        }

        Dictionary<Sequence, uint> sequenceOffsets = new(ReferenceEqualityComparer.Instance);
        foreach (var sequence in sequences)
        {
            sequenceOffsets[sequence] = (uint)position;
            position += 4 + commandSize * (sequence.Commands.Count + sequence.ExtraCommands.Count);
        }

        var length = (position + 1).Align(16);

        writer.WriteValueU32(Helpers.GetSignature(version), endian);
        
        if (hasScale == true)
        {
            writer.WriteValueU8(this.Scale);
            writer.WriteBytes(new byte[3]);
        }

        writer.WriteValueU32(frameTableOffset, endian);
        
        writer.WriteValueU32(hasScale == true
                ? (uint)this.Slots.Count
                : this.Unknown08
            , endian);

        foreach (var slot in this.Slots)
        {
            var mirror = slot.Mirror ? 1u : 0u;
            writer.WriteValueU32(slot.Sequence == null
                    ? mirror
                    : (sequenceOffsets[slot.Sequence] << 1) | mirror
                , endian);
        }

        writer.WriteValueS32(this.Frames.Count, endian);
        foreach (var frame in this.Frames)
        {
            writer.WriteValueU32(frameOffsets[frame], endian);
        }

        foreach (var frame in frames)
        {
            frame.Write(writer, version, endian);
        }

        foreach (var sequence in sequences)
        {
            sequence.Write(writer, version, endian);
        }

        writer.WriteBytes(new byte[length - position]);
    }
}
