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

using System;
using System.Collections.Generic;

namespace Gibbed.LetUsClingTogether.UnpackFILETABLE
{
    internal static class ScenarioHelpers
    {
        public static void GetScenarioTypeAndId(
            List<long> sceneIds,
            List<long> eventEntryIds,
            long id,
            out byte scenarioType,
            out long scenarioId)
        {
            if (sceneIds.Contains(id) == true)
            {
                // script
                scenarioType = 0;
                scenarioId = id;
            }
            else if (sceneIds.Contains(id - 0x08000) == true)
            {
                // resources
                scenarioType = 1;
                scenarioId = id - 0x08000;
            }
            else if (sceneIds.Contains(id - 0x10000) == true)
            {
                // messages
                scenarioType = 2;
                scenarioId = id - 0x10000;
            }
            else if (eventEntryIds.Contains(id - 0x0A000) == true)
            {
                // actors
                scenarioType = 6;
                scenarioId = id - 0x0A000;
            }
            else
            {
                GetScenarioId(id, out scenarioType, out scenarioId);
            }

            if (scenarioId < 0 || scenarioId > ushort.MaxValue)
            {
                throw new InvalidOperationException();
            }
        }

        private static void GetScenarioId(long id, out byte type, out long scenarioId)
        {
            switch (id)
            {
                case >= 0x08000 and <= 0x09FFF:
                {
                    // should have been caught already
                    throw new NotSupportedException();
                    /*
                    scenarioId = id - 0x08000;
                    type = 1;
                    return;
                    */
                }
                case >= 0x0A000 and <= 0x0AFFF:
                {
                    // should have been caught already
                    throw new NotSupportedException();
                    /*
                    type = 6;
                    scenarioId = id - 0x0A000;
                    return;
                    */
                }
                case >= 0x0D000 and <= 0x0DFFF:
                {
                    // entry unit list
                    type = 7;
                    scenarioId = id - 0x0D000;
                    return;
                }
                case >= 0x10000 and <= 0x10FFF:
                {
                    // messages
                    // should have been caught already
                    throw new NotSupportedException();
                    /*
                    type = 2;
                    scenarioId = id - 0x10000;
                    return;
                    */
                }
                case >= 0x11000 and <= 0x11FFF:
                {
                    // portraits
                    type = 3;
                    scenarioId = id - 0x11000;
                    return;
                }
                case >= 0x12000 and <= 0x12FFF:
                {
                    // animations
                    scenarioId = id - 0x12000;
                    type = 4;
                    return;
                }
                case >= 0x13000 and <= 0x13FFF:
                {
                    // sounds
                    scenarioId = id - 0x13000;
                    type = 8;
                    return;
                }
                case >= 0x50000 and <= 0x50FFF:
                {
                    // units
                    scenarioId = id - 0x50000;
                    type = 5;
                    return;
                }
            }
            throw new NotSupportedException();
        }
    }
}
