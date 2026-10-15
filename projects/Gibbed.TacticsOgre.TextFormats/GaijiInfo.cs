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

using System.Collections.Generic;

namespace Gibbed.TacticsOgre.TextFormats;

internal static class GaijiInfo
{
    private static readonly Dictionary<uint, string> _PSPLabels = new()
    {
        { 0x000391u, "enemy_1" },
        { 0x000392u, "enemy_2" },
        { 0x000393u, "enemy_3" },
        { 0x000394u, "enemy_4" },
        { 0x000395u, "leader_1" },
        { 0x000396u, "leader_2" },
        { 0x000397u, "leader_3" },
        { 0x000398u, "leader_4" },
        { 0x000399u, "guest_1" },
        { 0x00039Au, "guest_2" },
        { 0x00039Bu, "guest_3" },
        { 0x00039Cu, "guest_4" },
        { 0x002469u, "ai_1" },
        { 0x00246Au, "ai_2" },
    };

    private static readonly Dictionary<uint, string> _PSPEnglishLabels = new(_PSPLabels)
    {
        { 0x002030u, "er" },
    };

    private static readonly Dictionary<uint, string> _RebornLabels = new()
    {
        { 0x100000u, "heart" },
        { 0x10000Bu, "ai" },
        { 0x10000Cu, "skillrankup" },
        { 0x10000Du, "enemy" },
        { 0x10000Eu, "leader" },
        { 0x10000Fu, "guest" },
    };

    private static Dictionary<uint, string> GetLabels(Dialect dialect) => dialect switch
    {
        Dialect.PSPEnglish => _PSPEnglishLabels,
        Dialect.Reborn => _RebornLabels,
        _ => _PSPLabels,
    };

    public static bool IsGaiji(Dialect dialect, uint codepoint)
    {
        if (dialect == Dialect.Reborn)
        {
            // private use areas, except the four escape characters U+F8FC-U+F8FF
            return
                (codepoint >= 0x00E000u && codepoint <= 0x00F8FBu) ||
                (codepoint >= 0x0F0000u && codepoint <= 0x0FFFFDu) ||
                (codepoint >= 0x100000u && codepoint <= 0x10FFFDu);
        }
        return
            GetLabels(dialect).ContainsKey(codepoint) == true ||
            (codepoint >= 0x246Du && codepoint <= 0x2477u);
    }

    public static string GetLabel(Dialect dialect, uint codepoint)
    {
        return GetLabels(dialect).TryGetValue(codepoint, out var label) == true
            ? label
            : $"0x{codepoint:X}";
    }

    public static bool TryGetCodepoint(Dialect dialect, string label, out uint codepoint)
    {
        foreach (var kv in GetLabels(dialect))
        {
            if (kv.Value == label)
            {
                codepoint = kv.Key;
                return true;
            }
        }
        codepoint = default;
        return false;
    }
}
