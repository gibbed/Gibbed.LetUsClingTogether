/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.TacticsOgre.ScriptFormats
{
    // Opcodes are the same as Final Fantasy XII's VM opcodes:
    // https://wiki.ff12.pl/index.php?title=VM_instructions_/_opcodes
    public enum Opcode : byte
    {
        Invalid = 0, // (invalid)
        Label = 1, // (invalid)
        Tag = 2, // (invalid)
        Halt = 3,
        System = 4,
        LogicalOr = 5,
        LogicalAnd = 6,
        Or = 7,
        ExclusiveOr = 8, // XOR
        And = 9,
        Equal = 10,
        NotEqual = 11,
        GreaterThan = 12,
        LessThan = 13,
        GreaterThanOrEqual = 14,
        LessThanOrEqual = 15,
        ShiftLeft = 16,
        ShiftRight = 17,
        Add = 18,
        Subtract = 19,
        Multiply = 20,
        Divide = 21,
        Modulo = 22,
        Not = 23,
        Negate = 24,

        Undefined25 = 25, // no handler: "OPUMINUS"
        Undefined26 = 26, // no handler: "OPFIXADRS"

        // Special registers
        PushA = 27,
        PopA = 28,
        PushX = 29,
        PushY = 30,
        PopX = 31,
        PopY = 32,

        // ????
        // These interact with a large amount of data, possibly variables/data in the script file?
        Request = 33, // REQ, can adjust code pointer
        Unknown34 = 34, // FREQ, can adjust code pointer
        Unknown35 = 35, // TREQ, can adjust code pointer
        Unknown36 = 36, // REQSW, can adjust code pointer
        Unknown37 = 37, // FREQSW, can adjust code pointer
        Unknown38 = 38, // TREQSW, can adjust code pointer
        Unknown39 = 39, // REQEW, can adjust code pointer
        Unknown40 = 40, // FREQEW, can adjust code pointer
        Unknown41 = 41, // TREQEW, can adjust code pointer
        // ????

        NoOperation42 = 42, // PREQ
        NoOperation43 = 43, // PREQSW
        NoOperation44 = 44, // PREQEW

        Return = 45, // can adjust code pointer
        ReturnN = 46, // RETN, can adjust code pointer
        ReturnTo = 47, // RETT, can adjust code pointer
        ReturnTN = 48, // RETTN

        Undefined49 = 49, // DRET, no handler

        // ????
        // These interact with a large amount of data, possibly variables/data in the script file?
        RequestWait = 50, // REQWAIT
        NoOperation51 = 51, // PREQWAIT
        RequestChange = 52, // REQCHG
        RequestCancel = 53, // REQCANCEL
        // ????

        // General registers
        PopInt0 = 54,
        PopInt1 = 55,
        PopInt2 = 56,
        PopInt3 = 57,
        PopFloat0 = 58,
        PopFloat1 = 59,
        PopFloat2 = 60,
        PopFloat3 = 61,
        PushInt0 = 62,
        PushInt1 = 63,
        PushInt2 = 64,
        PushInt3 = 65,
        PushFloat0 = 66,
        PushFloat1 = 67,
        PushFloat2 = 68,
        PushFloat3 = 69,

        Undefined70 = 70, // DVAR
        Undefined71 = 71, // LONGCODESTART

        PushVariable = 72,
        PopVariable = 73,
        PushVariableDebug = 74,
        PushVariablePointer = 75,

        PushTag = 76,
        PushAct = 77,
        PushIntFromTable = 78,
        PushIntImmediate = 79, // short constant in bytecode
        PushFloatFromTable = 80,

        Jump = 81, // can adjust code pointer, same handler as JumpInternal
        JumpIfEqual = 82, // can adjust code pointer
        JumpIfGreaterThan = 83, // can adjust code pointer
        JumpIfGreaterThanOrEqual = 84, // can adjust code pointer
        JumpIfLessThan = 85, // can adjust code pointer
        JumpIfLessThanOrEqual = 86, // can adjust code pointer
        JumpIfBetween = 87, // can adjust code pointer

        Call = 88, // can adjust code pointer
        CallAct = 89, // can adjust code pointer

        PopXAndJump = 90, // can adjust code pointer
        PopXAndJumpIfNotZero = 91, // can adjust code pointer
        PopXAndJumpIfZero = 92, // can adjust code pointer
        CallAndPopA = 93, // can adjust code pointer
        CallActAndPopA = 94, // can adjust code pointer

        RequestAll = 95, // REQALL, can adjust code pointer

        JumpInternal = 96, // same handler as Jump

        RequestWaitAll = 97, // REQWAITALL

        Unknown98 = 98, // INCINITTAG
        Unknown99 = 99, // REQIALL, can adjust code pointer
    }
}
