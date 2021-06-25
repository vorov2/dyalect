using Dyalect.Runtime.Types;
using System;
using System.IO;

namespace Dyalect.Linker
{
    internal static class ObjectFile
    {
        public static readonly byte[] BOM = { 67, 12, 43 };
        public const int Version = 8;

        public static DyObject? DeserializeObject(BinaryReader reader) =>
            reader.ReadInt32() switch
            {
                -1 => null,
                DyType.Nil => DyNil.Instance,
                DyType.Integer => DyInteger.Get(reader.ReadInt32()),
                DyType.Float => new DyFloat(reader.ReadDouble()),
                DyType.String => new DyString(reader.ReadString()),
                DyType.Char => new DyChar(reader.ReadChar()),
                DyType.Bool => (DyBool)reader.ReadBoolean(),
                _ => throw new NotSupportedException(),
            };
    }
}
