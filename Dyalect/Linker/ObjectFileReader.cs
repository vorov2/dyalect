using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dyalect.Linker
{
    internal static class ObjectFileReader
    {
        private static DebugInfo ReadDebugInfo(BinaryReader reader)
        {
            var di = new DebugInfo();
            di.File = reader.ReadString();

            var scopes = reader.ReadInt32();
            for (var i = 0; i < scopes; i++)
            {
                var s = new ScopeSym();
                s.Index = reader.ReadInt32();
                s.ParentIndex = reader.ReadInt32();
                s.StartOffset = reader.ReadInt32();
                s.EndOffset = reader.ReadInt32();
                s.StartLine = reader.ReadInt32();
                s.StartColumn = reader.ReadInt32();
                s.EndLine = reader.ReadInt32();
                s.EndColumn = reader.ReadInt32();
            }

            var lines = reader.ReadInt32();
            for (var i = 0; i < lines; i++)
            {
                var l = new LineSym();
                l.Offset = reader.ReadInt32();
                l.Line = reader.ReadInt32();
                l.Column = reader.ReadInt32();
            }

            var vars = reader.ReadInt32();
            for (var i = 0; i < vars; i++)
            {
                var v = new VarSym();
                v.Name = reader.ReadString();
                v.Address = reader.ReadInt32();
                v.Offset = reader.ReadInt32();
                v.Scope = reader.ReadInt32();
                v.Flags = reader.ReadInt32();
                v.Data = reader.ReadInt32();
            }

            var funs = reader.ReadInt32();
            for (var i = 0; i < funs; i++)
            {
                var f = new FunSym();
                f.Name = reader.ReadString();
                f.Handle = reader.ReadInt32();
                f.StartOffset = reader.ReadInt32();
                f.EndOffset = reader.ReadInt32();

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
            }

            return di;
        }
    }
}
