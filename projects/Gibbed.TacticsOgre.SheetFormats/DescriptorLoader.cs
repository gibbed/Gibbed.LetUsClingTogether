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
using System.Diagnostics;
using System.IO;
using Tommy;

namespace Gibbed.TacticsOgre.SheetFormats
{
    public class DescriptorLoader
    {
        private static string GetExecutablePath()
        {
            using var process = Process.GetCurrentProcess();
            var path = Path.GetFullPath(process.MainModule.FileName);
            return Path.GetFullPath(path);
        }

        public static DescriptorFactory Load(string subdirectory)
        {
            var factory = new DescriptorFactory();

            var executablePath = GetExecutablePath();
            var binPath = Path.GetDirectoryName(executablePath);
            var filetablesPath = string.IsNullOrEmpty(subdirectory) == true
                ? Path.Combine(binPath, "..", "configs", "sheets")
                : Path.Combine(binPath, "..", "configs", subdirectory, "sheets");

            if (Directory.Exists(filetablesPath) == false)
            {
                return factory;
            }

            foreach (var inputPath in Directory.GetFiles(filetablesPath, "*.sheet.toml"))
            {
                TomlTable table;
                var inputBytes = File.ReadAllBytes(inputPath);
                using (var input = new MemoryStream(inputBytes, false))
                using (var reader = new StreamReader(input, true))
                {
                    table = TOML.Parse(reader);
                }

                var descriptorName = table["name"]?.AsString?.Value ?? throw new FormatException();
                var rowsAsTableArray = table["rows_table_array"]?.AsBoolean?.Value ?? false;

                var descriptor = ParseDescriptor(table, table);
                var descriptorInfo = new DescriptorInfo(
                    descriptor.EntrySize,
                    descriptor.HasStrings,
                    rowsAsTableArray,
                    () => descriptor);
                factory.Add(descriptorName, descriptorInfo);
            }
            return factory;
        }

        private static IDescriptor ParseDescriptor(TomlTable table, TomlTable rootTable)
        {
            IDescriptor descriptor;

            var columnsArray = table["columns"]?.AsArray;
            if (columnsArray != null)
            {
                descriptor = ParseStructDescriptor(table, columnsArray, rootTable);
            }
            else if (TryParseEnum<PrimitiveType>(table["type"], out var type) == true)
            {
                var minimumWidth = table["width"]?.AsInteger?.Value ?? 0;

                if (type == PrimitiveType.String)
                {
                    descriptor = new StringDescriptor((int)minimumWidth);
                }
                else if (type == PrimitiveType.Boolean)
                {
                    descriptor = new BooleanDescriptor((int)minimumWidth);
                }
                else if (type.IsInteger() == true)
                {
                    var enumName = table["enum"]?.AsString;
                    TomlNode enumNode;
                    if (enumName != null)
                    {
                        enumNode = rootTable["types"][enumName.Value];
                        if (enumNode == null)
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    else
                    {
                        enumNode = table["enum"];
                    }
                    IntegerBase integerBase;
                    if (TryParseEnum(table["base"], out integerBase) == false)
                    {
                        integerBase = IntegerBase.Decimal;
                    }
                    descriptor = new IntegerDescriptor(type, integerBase, (int)minimumWidth, ParseEnum(enumNode));
                }
                else if (type.IsFloat() == true)
                {
                    descriptor = new FloatDescriptor(type, (int)minimumWidth);
                }
                else if (type.IsUndefined() == true)
                {
                    descriptor = new UndefinedDescriptor(type, (int)minimumWidth);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            var arrayCount = table["array_count"]?.AsInteger?.Value;
            if (arrayCount != null)
            {
                descriptor = ParseArrayDescriptor(descriptor, table, (int)arrayCount);
            }

            return descriptor;
        }

        private static Dictionary<long, string> ParseEnum(TomlNode node)
        {
            if (node == null || (node.HasValue == false && node.ChildrenCount == 0))
            {
                return null;
            }
            var table = node.AsTable;
            if (table != null)
            {
                return ParseEnumTable(table);
            }
            var array = node.AsArray;
            if (array != null)
            {
                return ParseEnumArray(array);
            }
            throw new NotSupportedException();
        }

        private static Dictionary<long, string> ParseEnumArray(TomlArray array)
        {
            Dictionary<long, string> members = new();
            long index = 0;
            foreach (TomlString str in array)
            {
                members.Add(index, str.Value);
                index++;
            }
            return members;
        }

        private static Dictionary<long, string> ParseEnumTable(TomlTable table)
        {
            Dictionary<long, string> members = new();
            foreach (var kv in table.RawTable)
            {
                members.Add(Convert.ToInt64(kv.Key), kv.Value.AsString.Value);
            }
            return members;
        }

        private static IDescriptor ParseArrayDescriptor(IDescriptor descriptor, TomlTable table, int count)
        {
            var minimumWidth = table["array_width"]?.AsInteger?.Value ?? 0;
            var isInline = table["array_inline"]?.AsBoolean?.Value ?? false;
            return new ArrayDescriptor(descriptor, count, (int)minimumWidth, isInline);
        }

        private static StructDescriptor ParseStructDescriptor(TomlTable table, TomlArray array, TomlTable rootTable)
        {
            var minimumWidth = table["width"]?.AsInteger?.Value ?? 0;
            var isInline = table["inline"]?.AsBoolean?.Value ?? false;
            var instance = new StructDescriptor((int)minimumWidth, isInline);
            foreach (TomlTable childTable in array)
            {
                var name = childTable["name"]?.AsString?.Value ?? throw new FormatException();
                instance.Add(name, ParseDescriptor(childTable, rootTable));
            }
            return instance;
        }

        private static bool TryParseEnum<T>(TomlNode node, out T value)
            where T : struct
        {
            var s = node?.AsString?.Value;
            if (string.IsNullOrEmpty(s) == true)
            {
                value = default;
                return false;
            }
            if (Enum.TryParse(s, true, out value) == false)
            {
                throw new InvalidOperationException($"unknown name '{s}' for enum {typeof(T).Name}");
            }
            return true;
        }
    }
}
