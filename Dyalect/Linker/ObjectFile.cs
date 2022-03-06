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
                (int)DyType.Nil => DyNil.Instance,
                (int)DyType.Integer => DyInteger.Get(reader.ReadInt32()),
                (int)DyType.Float => new DyFloat(reader.ReadDouble()),
                (int)DyType.String => new DyString(reader.ReadString()),
                (int)DyType.Char => new DyChar(reader.ReadChar()),
                (int)DyType.Bool => (DyBool)reader.ReadBoolean(),
                _ => throw new NotSupportedException(),
            };
    }
}
