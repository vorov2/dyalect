using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Runtime.Types;
using System;
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
            using var stream = File.OpenRead(fileName);
            using var reader = new BinaryReader(stream);
            Read(reader, unit);
            return unit;
        }

        private static void Read(BinaryReader reader, Unit unit)
        {
            ReadHeader(reader, unit);
            ReadReferences(reader, unit);
            unit.UnitIds.AddRange(Enumerable.Repeat(-1, reader.ReadInt32()));
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
                unit.Ops.Add(reader.DeserializeOp());
        }

        private static void ReadIndices(BinaryReader reader, Unit unit)
        {
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
                unit.Strings.Add(reader.ReadString());

            count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var typeId = reader.ReadInt32();

                if (typeId == DyType.Integer)
                    unit.Objects.Add(new DyInteger(reader.ReadInt64()));
                else if (typeId == DyType.Float)
                    unit.Objects.Add(new DyFloat(reader.ReadDouble()));
                else if (typeId == DyType.String)
                    unit.Objects.Add(new DyString(reader.ReadString()));
                else if (typeId == DyType.Char)
                    unit.Objects.Add(new DyChar(reader.ReadChar()));
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
                    Guid.NewGuid(),
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
                di.Vars.Add(new
                (
                    name: reader.ReadString(),
                    address: reader.ReadInt32(),
                    offset: reader.ReadInt32(),
                    scope: reader.ReadInt32(),
                    flags: reader.ReadInt32(),
                    data: reader.ReadInt32()
                ));
            }

            var funs = reader.ReadInt32();
            for (var i = 0; i < funs; i++)
            {
                var f = new FunSym(reader.ReadString())
                {
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
