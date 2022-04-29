using Dyalect.Runtime.Types;
using System;
using System.IO;

namespace Dyalect.Linker
{
    internal static class ObjectFile
    {
        public static readonly byte[] BOM = { 67, 12, 43 };
        public const int Version = 11;

        public static DyObject? DeserializeObject(BinaryReader reader) =>
            reader.ReadInt32() switch
            {
                -1 => null,
                Dy.Nil => DyNil.Instance,
                Dy.Integer => DyInteger.Get(reader.ReadInt32()),
                Dy.Float => new DyFloat(reader.ReadDouble()),
                Dy.String => new DyString(reader.ReadString()),
                Dy.Char => new DyChar(reader.ReadChar()),
                Dy.Bool => reader.ReadBoolean() ? True : False,
                _ => throw new NotSupportedException(),
            };
    }
}
