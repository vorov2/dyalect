﻿using Dyalect.Compiler;
using Dyalect.Debug;
using System.Collections.Generic;
using System.IO;

namespace Dyalect.Linker;

public static class ObjectFileWriter
{
    public static void Write(string fileName, Unit unit)
    {
        using var stream = File.OpenWrite(fileName);
        using var writer = new BinaryWriter(stream);
        Write(writer, unit);
    }

    private static void Write(BinaryWriter writer, Unit unit)
    {
        WriteHeader(writer, unit);
        WriteReferences(writer, unit.References);
        writer.Write(unit.UnitIds.Count);
        WriteIndices(writer, unit);
        WriteOps(writer, unit.Ops);
        WriteSymbols(writer, unit.Symbols ?? DebugInfo.Default);
        WriteGlobalScope(writer, unit.GlobalScope!);
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

    private static int CalculateChecksum(FastList<Op> ops)
    {
        var checksum = 0;

        unchecked
        {
            foreach (var op in ops)
                checksum += (byte) op.Code;

            checksum &= 0xFF;
        }

        return checksum;
    }

    private static void WriteOps(BinaryWriter writer, FastList<Op> ops)
    {
        writer.Write(ops.Count);

        for (var i = 0; i < ops.Count; i++)
            ops[i].Serialize(writer);
    }

    private static void WriteIndices(BinaryWriter writer, Unit unit)
    {
        writer.Write(unit.Strings.Count);

        foreach (var s in unit.Strings)
            writer.Write((string)s);

        writer.Write(unit.Objects.Count);

        foreach (var o in unit.Objects)
            o.Serialize(writer);
    }

    private static void WriteMemoryLayouts(BinaryWriter writer, FastList<MemoryLayout> layouts)
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

    private static void WriteExportList(BinaryWriter writer, Dictionary<HashString, ScopeVar> list)
    {
        writer.Write(list.Count);

        foreach (var kv in list)
        {
            writer.Write((string)kv.Key);
            writer.Write(kv.Value.Address);
            writer.Write(kv.Value.Data);
        }
    }

    private static void WriteReferences(BinaryWriter writer, FastList<Reference> refs)
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
            writer.Write(r.SourceFileName ?? "");
        }
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
            writer.Write(f.TypeName is not null);
            writer.Write(f.TypeName ?? "");
            writer.Write(f.Parameters is null ? 0 : f.Parameters.Length);
            if (f.Parameters is not null)
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
