using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dyalect.Linker
{
    public static class ObjectFileWriter
    {
        public static void Write(string fileName, Unit unit)
        {
            using (var stream = File.OpenWrite(fileName))
            using (var writer = new BinaryWriter(stream))
                Write(writer, unit);
        }

        private static void Write(BinaryWriter writer, Unit unit)
        {
            WriteHeader(writer, unit);

            WriteReferences(writer, unit.References);
            writer.Write(unit.UnitIds.Count);
            WriteTypeDescriptors(writer, unit.Types);
            WriteMembers(writer, unit);
            WriteIndices(writer, unit);
            WriteOps(writer, unit.Ops);
            WriteSymbols(writer, unit.Symbols);
            WriteGlobalScope(writer, unit.GlobalScope);
            WriteMemoryLayouts(writer, unit.Layouts);
            WriteExportList(writer, unit.ExportList);
        }

        private static void WriteHeader(BinaryWriter writer, Unit unit)
        {
            for (var i = 0; i < ObjectFile.BOM.Length; i++)
                writer.Write(ObjectFile.BOM[i]);

            writer.Write(ObjectFile.Version);
            writer.Write(Meta.Version);
            writer.Write(CalculateChecksum(unit.Ops));
        }

        private static int CalculateChecksum(List<Op> ops)
        {
            var checksum = 0;

            foreach (var op in ops)
                checksum += (byte)op.Code;

            checksum &= 0xFF;
            return checksum;
        }

        private static void WriteOps(BinaryWriter writer, List<Op> ops)
        {
            writer.Write(ops.Count);

            for (var i = 0; i < ops.Count; i++)
                ops[i].Serialize(writer);
        }

        private static void WriteIndices(BinaryWriter writer, Unit unit)
        {
            WriteIndex(writer, unit.IndexedStrings);
            WriteIndex(writer, unit.IndexedIntegers);
            WriteIndex(writer, unit.IndexedFloats);
            WriteIndex(writer, unit.IndexedChars);
        }

        private static void WriteIndex(BinaryWriter writer, IEnumerable<DyObject> table)
        {
            var len = table.Count();
            writer.Write(table.Count());

            foreach (var o in table)
            {
                if (o.TypeId == DyType.String)
                    writer.Write(o.GetString());
                else if (o.TypeId == DyType.Integer)
                    writer.Write(o.GetInteger());
                else if (o.TypeId == DyType.Float)
                    writer.Write(o.GetFloat());
                else if (o.TypeId == DyType.Char)
                    writer.Write(o.GetChar());
            }
        }

        private static void WriteMemoryLayouts(BinaryWriter writer, List<MemoryLayout> layouts)
        {
            writer.Write(layouts.Count);

            foreach (var m in layouts)
            {
                writer.Write(m.Size);
                writer.Write(m.StackSize);
                writer.Write(m.Address);
            }
        }

        private static void WriteGlobalScope(BinaryWriter writer, Scope scope)
        {
            writer.Write(scope.Locals.Count);

            foreach (var l in scope.Locals)
            {
                writer.Write(l.Key);
                writer.Write(l.Value.Address);
                writer.Write(l.Value.Data);
            }
        }

        private static void WriteExportList(BinaryWriter writer, Dictionary<string, ScopeVar> list)
        {
            writer.Write(list.Count);

            foreach (var kv in list)
            {
                writer.Write(kv.Key);
                writer.Write(kv.Value.Address);
                writer.Write(kv.Value.Data);
            }
        }

        private static void WriteReferences(BinaryWriter writer, List<Reference> refs)
        {
            writer.Write(refs.Count);

            foreach (var r in refs)
            {
                writer.Write(r.Checksum);
                writer.Write(r.ModuleName);
                writer.Write(r.LocalPath ?? "");
                writer.Write(r.DllName ?? "");
                writer.Write(r.SourceLocation.Line);
                writer.Write(r.SourceLocation.Column);
                writer.Write(r.SourceFileName);
            }
        }

        private static void WriteTypeDescriptors(BinaryWriter writer, List<TypeDescriptor> types)
        {
            writer.Write(types.Count);

            foreach (var t in types)
            {
                writer.Write(t.Name);
                writer.Write(t.Id);
                writer.Write(t.AutoGenConstructors);
            }
        }

        private static void WriteMembers(BinaryWriter writer, Unit unit)
        {
            writer.Write(unit.MemberIds.Count);

            foreach (var m in unit.MemberNames)
                writer.Write(m);
        }

        private static void WriteSymbols(BinaryWriter writer, DebugInfo di)
        {
            writer.Write(di.File ?? "");

            writer.Write(di.Scopes.Count);
            foreach (var s in di.Scopes)
            {
                writer.Write(s.Index);
                writer.Write(s.ParentIndex);
                writer.Write(s.StartOffset);
                writer.Write(s.EndOffset);
                writer.Write(s.StartLine);
                writer.Write(s.StartColumn);
                writer.Write(s.EndLine);
                writer.Write(s.EndColumn);
            }

            writer.Write(di.Lines.Count);
            foreach (var l in di.Lines)
            {
                writer.Write(l.Offset);
                writer.Write(l.Line);
                writer.Write(l.Column);
            }

            writer.Write(di.Vars.Count);
            foreach (var v in di.Vars)
            {
                writer.Write(v.Name);
                writer.Write(v.Address);
                writer.Write(v.Offset);
                writer.Write(v.Scope);
                writer.Write(v.Flags);
                writer.Write(v.Data);
            }

            writer.Write(di.Functions.Count);
            foreach (var f in di.Functions.Values)
            {
                writer.Write(f.Name);
                writer.Write(f.Handle);
                writer.Write(f.StartOffset);
                writer.Write(f.EndOffset);
                writer.Write(f.Parameters.Length);
                foreach (var p in f.Parameters)
                {
                    writer.Write(p.Name);
                    writer.Write(p.IsVarArg);
                    if (p.Value == null)
                        writer.Write(-1);
                    else
                        p.Value.Serialize(writer);
                }
            }
        }
    }
}
