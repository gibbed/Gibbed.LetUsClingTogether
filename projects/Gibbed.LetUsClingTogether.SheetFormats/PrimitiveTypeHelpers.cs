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

namespace Gibbed.LetUsClingTogether.SheetFormats
{
    internal static class PrimitiveTypeHelpers
    {
        public static bool IsInteger(this PrimitiveType type) => type switch
        {
            PrimitiveType.Int8 => true,
            PrimitiveType.UInt8 => true,
            PrimitiveType.Int16 => true,
            PrimitiveType.UInt16 => true,
            PrimitiveType.Int32 => true,
            PrimitiveType.UInt32 => true,
            PrimitiveType.Int64 => true,
            PrimitiveType.UInt64 => true,
            _ => false,
        };

        public static bool IsFloat(this PrimitiveType type) => type switch
        {
            PrimitiveType.Float32 => true,
            _ => false,
        };

        public static bool IsUndefined(this PrimitiveType type) => type switch
        {
            PrimitiveType.Undefined8 => true,
            PrimitiveType.Undefined16 => true,
            PrimitiveType.Undefined32 => true,
            PrimitiveType.Undefined64 => true,
            _ => false,
        };
    }
}
