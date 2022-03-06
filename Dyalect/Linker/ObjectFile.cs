using Dyalect.Runtime.Types;
using System;
using System.IO;

namespace Dyalect.Linker
{
    internal static class ObjectFile
    {
        public static readonly byte[] BOM = { 67, 12, 43 };
        public const int Version = 9;

        public static DyObject? DeserializeObject(BinaryReader reader) =>
            reader.ReadInt32() switch
            {
                -1 => null,
                (int)DyTypeCode.Nil => ctx.RuntimeContext.Nil.Instance,
                (int)DyTypeCode.Integer => ctx.RuntimeContext.Integer.Get(reader.ReadInt32()),
                (int)DyTypeCode.Float => new DyFloat(reader.ReadDouble()),
                (int)DyTypeCode.String => new DyString(reader.ReadString()),
                (int)DyTypeCode.Char => new DyChar(reader.ReadChar()),
                (int)DyTypeCode.Bool => (DyBool)reader.ReadBoolean(),
                _ => throw new NotSupportedException(),
            };
    }
}
