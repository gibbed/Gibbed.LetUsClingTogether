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

namespace Gibbed.TacticsOgre.SequenceFormats;

public static class Opcodes
{
    public enum ArgKind
    {
        None,
        U8,
        U16,
        S8,
    }

    public sealed record Info(Opcode Opcode, string Name, ArgKind Arg, OpcodeFields Fields);

    private const OpcodeFields None = OpcodeFields.None;
    private const OpcodeFields Flips = OpcodeFields.Flips;
    private const OpcodeFields Priority = OpcodeFields.Priority;
    private const OpcodeFields XY = OpcodeFields.XY;
    private const OpcodeFields Z = OpcodeFields.Z;

    public static IReadOnlyList<Info> All { get; } =
    [
        new(Opcode.ShowFrame, "Show frame", ArgKind.U8, Flips | Priority | XY),
        new(Opcode.Move, "Move", ArgKind.None, XY),
        new(Opcode.ShowFrame3D, "Show frame 3D", ArgKind.U8, Flips | Priority | XY | Z),
        new(Opcode.Move3D, "Move 3D", ArgKind.None, XY | Z),
        new(Opcode.Nop4, "No-op 4", ArgKind.None, None),
        new(Opcode.Nop5, "No-op 5", ArgKind.None, None),
        new(Opcode.Nop6, "No-op 6", ArgKind.None, None),
        new(Opcode.Hide, "Hide", ArgKind.None, None),
        new(Opcode.PlaySound, "Play sound", ArgKind.U16, None),
        new(Opcode.Event, "Event", ArgKind.U16, None),
        new(Opcode.HitCue, "Hit cue", ArgKind.U8, None),
        new(Opcode.Goto, "Goto", ArgKind.S8, None),
        new(Opcode.AttachPoint, "Attach point", ArgKind.None, XY),
        new(Opcode.Nop13, "No-op 13", ArgKind.None, None),
        new(Opcode.ReactionCue, "Reaction cue", ArgKind.U8, None),
    ];

    public static Info Get(Opcode opcode) => All[(int)opcode];

    public static int GetArg(Command c) => Get(c.Opcode).Arg switch
    {
        ArgKind.U8 => c.RawArg0,
        ArgKind.U16 => c.ArgU16,
        ArgKind.S8 => c.ArgS8,
        _ => c.RawArg0,
    };

    public static Command WithArg(Command c, int value) => Get(c.Opcode).Arg switch
    {
        ArgKind.U16 => c with
        {
            ArgU16 = (ushort)Math.Clamp(value, 0, ushort.MaxValue),
        },
        ArgKind.S8 => c with
        {
            ArgS8 = (sbyte)Math.Clamp(value, sbyte.MinValue, sbyte.MaxValue),
        },
        _ => c with
        {
            RawArg0 = (byte)Math.Clamp(value, 0, byte.MaxValue),
        },
    };

    public static string Describe(Command c)
    {
        string flips = (c.HFlip ? ", h-flip" : "") + (c.VFlip ? ", v-flip" : "") + (c.Priority != 0 ? $", prio {c.Priority:+0;-0}" : "");
        return c.Opcode switch
        {
            Opcode.ShowFrame => $"Show frame {c.FrameIndex} at ({c.X}, {c.Y}){flips}",
            Opcode.Move => $"Move to ({c.X}, {c.Y})",
            Opcode.ShowFrame3D => $"Show frame {c.FrameIndex} at 3D ({c.X}, {c.Y}, {c.Z}){flips}",
            Opcode.Move3D => $"Move to 3D ({c.X}, {c.Y}, {c.Z})",
            Opcode.Hide => "Hide",
            Opcode.PlaySound => $"Play sound effect {c.ArgU16}",
            Opcode.Event => DescribeEvent(c.ArgU16),
            Opcode.HitCue => $"Hit cue {c.RawArg0}",
            Opcode.Goto => $"Goto command {c.ArgS8}",
            Opcode.AttachPoint => $"Attach point ({c.X}, {c.Y})",
            Opcode.ReactionCue => $"Reaction cue {c.RawArg0}",
            _ => "No-op",
        };
    }

    public static string DescribeEvent(int arg) => arg switch
    {
        0 => "Event 0 (start action effect)",
        1 => "Event 1 (release held projectile)",
        2 => "Event 2 (fire projectile)",
        3 => "Event 3 (jump takeoff)",
        4 => "Event 4 (hit trigger)",
        5 => "Event 5 (wait point)",
        6 => "Event 6 (overhead effect)",
        7 => "Event 7 (?)",
        10 => "Event 10 (suppress water effect)",
        11 => "Event 11 (allow water effect)",
        >= 100 => $"Signal {arg - 100}",
        _ => $"Event {arg} (unused)",
    };
}
