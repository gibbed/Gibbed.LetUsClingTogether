1. Number of usages of opcodes in all scripts in the US (`ULUS10565`) release.
2. Number of usages of opcodes in all scripts in the JP (`ULJM05753`) release. A * denotates it is the same as the US release.

| #    | Label                      | [FF12 Label](https://wiki.ff12.pl/index.php?title=VM_instructions_/_opcodes) | US<sup>1</sup> | JP<sup>2</sup> |
|:--:|:-------------------------- |:------------------ | ------:| ------:|
| 0  | `Invalid`                  | `NOP`              |        |        |
| 1  | `Label`                    | `LABEL`            |        |        |
| 2  | `Tag`                      | `TAG`              |        |        |
| 3  | `Halt`                     | `SYSHALT`          |        |        |
| 4  | `System`                   | `SYSTEM`           |        |        |
| 5  | `LogicalOr`                | `OPLOR`            | 29     | *      |
| 6  | `LogicalAnd`               | `OPLAND`           | 153    | *      |
| 7  | `Or`                       | `OPOR`             |        |        |
| 8  | `ExclusiveOr`              | `OPEOR`            |        |        |
| 9  | `And`                      | `OPAND`            |        |        |
| 10 | `Equal`                    | `OPEQ`             | 423    | *      |
| 11 | `NotEqual`                 | `OPNE`             |        |        |
| 12 | `GreaterThan`              | `OPGT`             |        |        |
| 13 | `LessThan`                 | `OPLS`             | 33     | *      |
| 14 | `GreaterThanOrEqual`       | `OPGTE`            | 3      | *      |
| 15 | `LessThanOrEqual`          | `OPLSE`            |        |        |
| 16 | `ShiftLeft`                | `OPSLL`            |        |        |
| 17 | `ShiftRight`               | `OPSRL`            |        |        |
| 18 | `Add`                      | `OPADD`            | 235    | *      |
| 19 | `Subtract`                 | `OPSUB`            | 16     | *      |
| 20 | `Multiply`                 | `OPMUL`            | 5      | *      |
| 21 | `Divide`                   | `OPDIV`            | 11     | *      |
| 22 | `Modulo`                   | `OPMOD`            |        |        |
| 23 | `Not`                      | `OPNOT`            | 39     | *      |
| 24 | `Negate`                   | `OPBNOT`           |        |        |
| 25 | `Undefined25`              | `OPUMINUS`         |        |        |
| 26 | `Undefined26`              | `OPFIXADRS`        |        |        |
| 27 | `PushA`                    | `PUSHA`            |        |        |
| 28 | `PopA`                     | `POPA`             |        |        |
| 29 | `PushX`                    | `PUSHX`            |        |        |
| 30 | `PushY`                    | `PUSHY`            |        |        |
| 31 | `PopX`                     | `POPX`             |        |        |
| 32 | `PopY`                     | `POPY`             | 13     | *      |
| 33 | `Unknown33`                | `REQ`              | 1249   | *      |
| 34 | `Unknown34`                | `FREQ`             |        |        |
| 35 | `Unknown35`                | `TREQ`             |        |        |
| 36 | `Unknown36`                | `REQSW`            |        |        |
| 37 | `Unknown37`                | `FREQSW`           |        |        |
| 38 | `Unknown38`                | `TREQSW`           |        |        |
| 39 | `Unknown39`                | `REQEW`            |        |        |
| 40 | `Unknown40`                | `FREQEW`           |        |        |
| 41 | `Unknown41`                | `TREQEW`           |        |        |
| 42 | `NoOperation42`            | `PREQ`             |        |        |
| 43 | `NoOperation43`            | `PREQSW`           |        |        |
| 44 | `NoOperation44`            | `PREQEW`           |        |        |
| 45 | `Return`                   | `RET`              | 17415  | *      |
| 46 | `ReturnN`                  | `RETN`             |        |        |
| 47 | `ReturnT`                  | `RETT`             |        |        |
| 48 | `ReturnTN`                 | `RETTN`            |        |        |
| 49 | `Undefined49`              | `DRET`             |        |        |
| 50 | `Unknown50`                | `REQWAIT`          | 684    | 683    |
| 51 | `NoOperation51`            | `PREQWAIT`         |        |        |
| 52 | `Unknown52`                | `REQCHG`           |        |        |
| 53 | `Unknown53`                | `REQCANCEL`        |        |        |
| 54 | `PopInt0`                  | `POPI0`            |        |        |
| 55 | `PopInt1`                  | `POPI1`            |        |        |
| 56 | `PopInt2`                  | `POPI2`            |        |        |
| 57 | `PopInt3`                  | `POPI3`            |        |        |
| 58 | `PopFloat0`                | `POPF0`            |        |        |
| 59 | `PopFloat1`                | `POPF1`            |        |        |
| 60 | `PopFloat2`                | `POPF2`            |        |        |
| 61 | `PopFloat3`                | `POPF3`            |        |        |
| 62 | `PushInt0`                 | `PUSHI0`           |        |        |
| 63 | `PushInt1`                 | `PUSHI1`           |        |        |
| 64 | `PushInt2`                 | `PUSHI2`           |        |        |
| 65 | `PushInt3`                 | `PUSHI3`           |        |        |
| 66 | `PushFloat0`               | `PUSHF0`           |        |        |
| 67 | `PushFloat1`               | `PUSHF1`           |        |        |
| 68 | `PushFloat2`               | `PUSHF2`           |        |        |
| 69 | `PushFloat3`               | `PUSHF3`           |        |        |
| 70 | `Undefined70`              | `DVAR`             |        |        |
| 71 | `Undefined71`              | `LONGCODESTART`    |        |        |
| 72 | `PushVariable`             | `PUSHV`            | 2230   | *      |
| 73 | `PopVariable`              | `POPV`             | 432    | *      |
| 74 | `PushVariableDebug`        | `PUSHDBG`          |        |        |
| 75 | `PushVariablePointer`      | `PUSHP`            | 1596   | *      |
| 76 | `PushTag`                  | `PUSHTAG`          |        |        |
| 77 | `PushAct`                  | `PUSHACT`          |        |        |
| 78 | `PushIntFromTable`         | `PUSHI`            |        |        |
| 79 | `PushIntImmediate`         | `PUSHII`           | 121615 | 121597 |
| 80 | `PushFloatFromTable`       | `PUSHF`            | 5362   | *      |
| 81 | `Jump`                     | `JMP`              | 563    | *      |
| 82 | `JumpIfEqual`              | `POPCMPYEQJMP`     | 60     | *      |
| 83 | `JumpIfGreaterThan`        | `POPCMPYLOWJMP`    |        |        |
| 84 | `JumpIfGreaterThanOrEqual` | `POPCMPYLOWEQJMP`  |        |        |
| 85 | `JumpIfLessThan`           | `POPCMPYHIGHJMP`   |        |        |
| 86 | `JumpIfLessThanOrEqual`    | `POPCMPYHIGHEQJMP` |        |        |
| 87 | `JumpIfBetween`            | `POP2CMPYBTWNJMP`  |        |        |
| 88 | `Call`                     | `CALL`             | 844    | *      |
| 89 | `CallAct`                  | `CALLACT`          |        |        |
| 90 | `PopXAndJump`              | `POPXJMP`          |        |        |
| 91 | `PopXAndJumpIfNotZero`     | `POPXCJMP`         | 18     | *      |
| 92 | `PopXAndJumpIfZero`        | `POPXNCJMP`        | 934    | *      |
| 93 | `CallAndPopA`              | `CALLPOPA`         | 93263  | 93249  |
| 94 | `CallActAndPopA`           | `CALLACTPOPA`      |        |        |
| 95 | `Unknown95`                | `REQALL`           | 1463   | *      |
| 96 | `JumpInternal`             | `JMPINTERNAL`      |        |        |
| 97 | `Unknown97`                | `REQWAITALL`       | 1570   | *      |
| 98 | `Unknown98`                | `INCINITTAG`       |        |        |
| 99 | `Unknown99`                | `REQIALL`          |        |        |
