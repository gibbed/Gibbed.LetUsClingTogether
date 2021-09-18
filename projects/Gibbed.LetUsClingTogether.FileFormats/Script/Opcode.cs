/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.LetUsClingTogether.FileFormats.Script
{
    public enum Opcode : byte
    {
        Invalid = 0,
        // 1 (invalid)
        // 2 (invalid)
        Unknown3 = 3,
        Pop = 4,
        Either = 5,
        Both = 6,
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

        Undefined25 = 25, // no handler
        Undefined26 = 26, // no handler

        // Some sort of state-specific temporary variables
        PushFoo = 27,
        PopFoo = 28,
        PushBar = 29,
        PushBaz = 30,
        PopBar = 31,
        PopBaz = 32,

        // ????
        // These interact with a large amount of data, possibly variables/data in the script file?
        Unknown33 = 33, // can adjust code pointer
        Unknown34 = 34, // can adjust code pointer
        Unknown35 = 35, // can adjust code pointer
        Unknown36 = 36, // can adjust code pointer
        Unknown37 = 37, // can adjust code pointer
        Unknown38 = 38, // can adjust code pointer
        Unknown39 = 39, // can adjust code pointer
        Unknown40 = 40, // can adjust code pointer
        Unknown41 = 41, // can adjust code pointer
        // ????

        NoOperation42 = 42,
        NoOperation43 = 43,
        NoOperation44 = 44,

        Return = 45, // can adjust code pointer
        UnknownReturn46 = 46, // can adjust code pointer
        UnknownJump47 = 47, // can adjust code pointer
        UnknownJump48 = 48,

        Undefined49 = 49, // no handler

        // ????
        // These interact with a large amount of data, possibly variables/data in the script file?
        Unknown50 = 50,
        NoOperation51 = 51,
        Unknown52 = 52,
        Unknown53 = 53,
        // ????

        // ????
        // Some sort of state-specific temporary variables
        PopIntParam38 = 54,
        PopIntParam3C = 55,
        PopIntParam40 = 56,
        PopIntParam44 = 57,
        PopFloatParam48 = 58,
        PopFloatParam4C = 59,
        PopFloatParam50 = 60,
        PopFloatParam54 = 61,
        PushIntParam38 = 62,
        PushIntParam3C = 63,
        PushIntParam40 = 64,
        PushIntParam44 = 65,
        PushFloatParam48 = 66,
        PushFloatParam4C = 67,
        PushFloatParam50 = 68,
        PushFloatParam54 = 69,
        // ????

        Undefined70 = 70,
        Undefined71 = 71,

        MaybeGetVariable = 72,
        MaybeSetVariable = 73,
        Unknown74 = 74,
        Unknown75 = 75,

        PushUnknown76 = 76,
        PushUnknown77 = 77,
        PushUnknown78 = 78,
        PushInt = 79, // short constant in bytecode
        Unknown80 = 80,

        Jump = 81, //  // can adjust code pointer, same handler as Jump_
        JumpIfEqual = 82, // can adjust code pointer
        JumpIfGreaterThan = 83, // can adjust code pointer
        JumpIfGreaterThanOrEqual = 84, // can adjust code pointer
        JumpIfLessThan = 85, // can adjust code pointer
        JumpIfLessThanOrEqual = 86, // can adjust code pointer
        JumpIfBetween = 87, // can adjust code pointer

        CallNative = 88, // can adjust code pointer
        UnknownCallNative = 89, // can adjust code pointer

        PopBarAndJump = 90, // can adjust code pointer
        PopBarAndJumpIfNotZero = 91, // can adjust code pointer
        PopBarAndJumpIfZero = 92, // can adjust code pointer
        CallNativeWithBar = 93, // can adjust code pointer
        UnknownBar94 = 94, // can adjust code pointer

        Unknown95 = 95, // can adjust code pointer

        Jump_ = 96, // same handler as Jump

        Unknown97 = 97,

        Unknown98 = 98,
        Unknown99 = 99, // can adjust code pointer
    }
}
