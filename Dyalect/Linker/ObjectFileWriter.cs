using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System.IO;

namespace Dyalect.Linker
{
    internal static class ObjectFileWriter
    {
        private static void WriteHeader(BinaryWriter writer)
        {
            for (var i = 0; i < ObjectFile.BOM.Length; i++)
                writer.Write(ObjectFile.BOM[i]);

            writer.Write(ObjectFile.Version);
            writer.Write(Meta.Version);
        }

        private static void WriteOps(BinaryWriter writer, List<Op> ops)
        {
            writer.Write(ops.Count);

            for (var i = 0; i < ops.Count; i++)
                ops[i].Serialize(writer);
        }

        private static void WriteIndexed(BinaryWriter writer, List<DyObject> table)
        {
            writer.Write(table.Count);

            for (var i = 0; i < table.Count; i++)
            {
                var o = table[i];

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
    }
}
