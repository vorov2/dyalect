using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dyalect.Linker
{
    public static class ObjectFileReader
    {
        public static Unit Read(string fileName)
        {
            var unit = new Unit();

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
                Read(reader, unit);

            return unit;
        }

        private static void Read(BinaryReader reader, Unit unit)
        {
            ReadHeader(reader, unit);

            ReadReferences(reader, unit);
            unit.UnitIds.AddRange(Enumerable.Repeat(-1, reader.ReadInt32()));
            ReadTypeDescriptors(reader, unit);
            ReadIndices(reader, unit);
            ReadOps(reader, unit);
            ReadSymbols(reader, unit);
            ReadGlobalScope(reader, unit);
            ReadMemoryLayouts(reader, unit);
            ReadExportList(reader, unit);
        }

        private static void ReadHeader(BinaryReader reader, Unit unit)
        {
            if (reader.BaseStream.Length < ObjectFile.BOM.Length)
                throw new DyException("Invalid object file.");

            for (var i = 0; i < ObjectFile.BOM.Length; i++)
                if (ObjectFile.BOM[i] != reader.ReadByte())
                    throw new DyException("Invalid object file.");

            if (reader.ReadInt32() != ObjectFile.Version)
                throw new DyException("Unsupported version of object file.");

            reader.ReadString();
            unit.Checksum = reader.ReadInt32();
        }

        private static void ReadOps(BinaryReader reader, Unit unit)
        {
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
                unit.Ops.Add(OpExtensions.DeserializeOp(reader));
        }

        private static void ReadIndices(BinaryReader reader, Unit unit)
        {
            unit.IndexedStrings.AddRange(ReadIndex<DyString>(reader));
            unit.IndexedIntegers.AddRange(ReadIndex<DyInteger>(reader));
            unit.IndexedFloats.AddRange(ReadIndex<DyFloat>(reader));
            unit.IndexedChars.AddRange(ReadIndex<DyChar>(reader));
        }

        private static IEnumerable<T> ReadIndex<T>(BinaryReader reader) where T : DyObject
        {
            var count = reader.ReadInt32();
            var typeId =
                typeof(T) == typeof(DyString) ? DyType.String
                : typeof(T) == typeof(DyInteger) ? DyType.Integer
                : typeof(T) == typeof(DyFloat) ? DyType.Float
                : DyType.Char;

            for (var i = 0; i < count; i++)
            {
                if (typeId == DyType.String)
                    yield return (T)(object)new DyString(reader.ReadString());
                else if (typeId == DyType.Integer)
                    yield return (T)(object)DyInteger.Get(reader.ReadInt64());
                else if (typeId == DyType.Float)
                    yield return (T)(object)new DyFloat(reader.ReadDouble());
                else if (typeId == DyType.Char)
                    yield return (T)(object)new DyChar(reader.ReadChar());
            }
        }

        private static void ReadMemoryLayouts(BinaryReader reader, Unit unit)
        {
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
                unit.Layouts.Add(new(
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32()));
        }

        private static void ReadGlobalScope(BinaryReader reader, Unit unit)
        {
            var count = reader.ReadInt32();
            unit.GlobalScope = new(ScopeKind.Lexical, default);

            for (var i = 0; i < count; i++)
            {
                unit.GlobalScope.Locals.Add(reader.ReadString(),
                    new(reader.ReadInt32(), reader.ReadInt32()));
            }
        }

        private static void ReadExportList(BinaryReader reader, Unit unit)
        {
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadString();
                unit.ExportList.Add(name,
                    new(reader.ReadInt32(), reader.ReadInt32()));
            }
        }

        private static void ReadReferences(BinaryReader reader, Unit unit)
        {
            var refs = reader.ReadInt32();
            for (var i = 0; i < refs; i++)
            {
                var checksum = reader.ReadInt32();
                string str;
                var r = new Reference(
                    reader.ReadString(),
                    (str = reader.ReadString()).Length == 0 ? null : str,
                    (str = reader.ReadString()).Length == 0 ? null : str,
                    new(reader.ReadInt32(), reader.ReadInt32()),
                    reader.ReadString()
                )
                {
                    Checksum = checksum
                };
                unit.References.Add(r);
            }
        }

        private static void ReadTypeDescriptors(BinaryReader reader, Unit unit)
        {
            var types = reader.ReadInt32();

            for (var i = 0; i < types; i++)
            {
                var td = new TypeDescriptor(
                    reader.ReadString(),
                    reader.ReadInt32(),
                    reader.ReadBoolean());
                unit.Types.Add(td);
                unit.TypeMap.Add(td.Name, td);
            }
        }

        private static void ReadSymbols(BinaryReader reader, Unit unit)
        {
            var di = new DebugInfo();
            unit.Symbols = di;
            di.File = reader.ReadString();

            var scopes = reader.ReadInt32();
            for (var i = 0; i < scopes; i++)
                di.Scopes.Add(new()
                {
                    Index = reader.ReadInt32(),
                    ParentIndex = reader.ReadInt32(),
                    StartOffset = reader.ReadInt32(),
                    EndOffset = reader.ReadInt32(),
                    StartLine = reader.ReadInt32(),
                    StartColumn = reader.ReadInt32(),
                    EndLine = reader.ReadInt32(),
                    EndColumn = reader.ReadInt32()
                });

            var lines = reader.ReadInt32();
            for (var i = 0; i < lines; i++)
            {
                di.Lines.Add(new()
                {
                    Offset = reader.ReadInt32(),
                    Line = reader.ReadInt32(),
                    Column = reader.ReadInt32()
                });
            }

            var vars = reader.ReadInt32();
            for (var i = 0; i < vars; i++)
            {
                di.Vars.Add(new()
                {
                    Name = reader.ReadString(),
                    Address = reader.ReadInt32(),
                    Offset = reader.ReadInt32(),
                    Scope = reader.ReadInt32(),
                    Flags = reader.ReadInt32(),
                    Data = reader.ReadInt32()
                });
            }

            var funs = reader.ReadInt32();
            for (var i = 0; i < funs; i++)
            {
                var f = new FunSym
                {
                    Name = reader.ReadString(),
                    Handle = reader.ReadInt32(),
                    StartOffset = reader.ReadInt32(),
                    EndOffset = reader.ReadInt32()
                };

                var pars = reader.ReadInt32();
                f.Parameters = new Par[pars];
                for (var j = 0; j < pars; j++)
                {
                    var name = reader.ReadString();
                    var va = reader.ReadBoolean();
                    var value = ObjectFile.DeserializeObject(reader);
                    var p = new Par(name, value, va);
                    f.Parameters[j] = p;
                }

                di.Functions.Add(f.Handle, f);
            }
        }
    }
}
