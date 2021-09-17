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
        Unknown0 = 0,

        Undefined1 = 1, // no handler
        Undefined2 = 2, // no handler

        Return = 3,
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
        Unknown33 = 33,
        Unknown34 = 34,
        Unknown35 = 35,
        Unknown36 = 36,
        Unknown37 = 37,
        Unknown38 = 38,
        Unknown39 = 39,
        Unknown40 = 40,
        Unknown41 = 41,
        // ????

        NoOperation42 = 42,
        NoOperation43 = 43,
        NoOperation44 = 44,

        UnknownJump45 = 45,
        UnknownJump46 = 46,
        UnknownJump47 = 47,
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
        Unknown73 = 73,
        Unknown74 = 74,
        MaybeSetVariable = 75,

        PushUnknown76 = 76,
        PushUnknown77 = 77,
        PushUnknown78 = 78,
        PushInt = 79, // short constant in bytecode
        Unknown80 = 80,

        Unknown81 = 81, // same handler as Unknown96

        UnknownBazEquals = 82,
        UnknownBazGreaterThan = 83,
        UnknownBazGreaterThanOrEqual = 84,
        UnknownBazLessThan = 85,
        UnknownBazLessThanOrEqual = 86,
        UnknownBazLessThanOrEqual_Two = 87,

        UnknownBar88 = 88,
        UnknownBar89 = 89,
        UnknownBar90 = 90,
        UnknownBar91 = 91,
        UnknownBar92 = 92,
        CallNative = 93,
        UnknownBar94 = 94,

        Unknown95 = 95,

        Unknown96 = 96, // same handler as Unknown81

        Unknown97 = 97,

        Unknown98 = 98,
        Unknown99 = 99,
    }
}
