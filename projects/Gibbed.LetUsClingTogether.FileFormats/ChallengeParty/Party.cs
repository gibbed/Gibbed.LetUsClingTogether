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

using System;
using System.IO;
using Gibbed.IO;

namespace Gibbed.LetUsClingTogether.FileFormats.ChallengeParty
{
    public struct Party
    {
        public const int Unknown0000Length = 25;
        public byte[] Unknown0000Bytes;
        public const int NameLength = 55;
        public byte[] NameBytes;
        public const int UnitCount = 12;
        public Unit[] Units;
        public const int ItemQuantityCount = 661 + 601;
        public byte[] ItemQuantities;
        public const int CursedWeaponCount = 17;
        public CursedWeapon[] CursedWeapons;
        public uint Unknown2CB8;
        public uint Unknown2CBC;

        private static readonly byte[] DummyItemQuantities;

        static Party()
        {
            DummyItemQuantities = new byte[ItemQuantityCount];
        }

        public static Party Read(Stream input, Endian endian)
        {
            Party instance;
            instance.Unknown0000Bytes = input.ReadBytes(Unknown0000Length);
            instance.NameBytes = input.ReadBytes(NameLength);
            instance.Units = new Unit[UnitCount];
            for (int i = 0; i < UnitCount; i++)
            {
                instance.Units[i] = Unit.Read(input, endian);
            }
            instance.ItemQuantities = input.ReadBytes(ItemQuantityCount);
            instance.CursedWeapons = new CursedWeapon[CursedWeaponCount];
            for (int i = 0; i < CursedWeaponCount; i++)
            {
                instance.CursedWeapons[i] = CursedWeapon.Read(input);
            }
            instance.Unknown2CB8 = input.ReadValueU32(endian);
            instance.Unknown2CBC = input.ReadValueU32(endian);
            return instance;
        }

        public static void Write(Stream output, Party instance, Endian endian)
        {
            output.WriteBytes(instance.Unknown0000Bytes);
            output.WriteBytes(instance.NameBytes);

            ArrayHelper.ForEach(instance.Units, e => Unit.Write(output, e, endian), UnitCount);

            int itemQuantityIndex = 0;
            if (instance.ItemQuantities != null)
            {
                if (instance.ItemQuantities.Length > ItemQuantityCount)
                {
                    throw new InvalidOperationException("too many item quantities to write");
                }
                var itemQuantityCount = Math.Min(instance.ItemQuantities.Length, ItemQuantityCount);
                output.Write(instance.ItemQuantities, 0, itemQuantityCount);
                itemQuantityIndex += itemQuantityCount;
            }
            if (itemQuantityIndex < ItemQuantityCount)
            {
                output.Write(DummyItemQuantities, 0, ItemQuantityCount - itemQuantityIndex);
            }

            ArrayHelper.ForEach(instance.CursedWeapons, e => CursedWeapon.Write(output, e), CursedWeaponCount);

            output.WriteValueU32(instance.Unknown2CB8, endian);
            output.WriteValueU32(instance.Unknown2CBC, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }
    }
}
