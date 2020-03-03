using Dyalect.Runtime.Types;
using System;
using System.IO;

namespace Dyalect.Linker
{
    internal static class ObjectFile
    {
        public static readonly byte[] BOM = { 67, 12, 43 };
        public const int Version = 3;

        public static DyObject DeserializeObject(BinaryReader reader)
        {
            switch (reader.ReadInt32())
            {
                case -1: return null;
                case DyType.Nil: return DyNil.Instance;
                case DyType.Integer: return DyInteger.Get(reader.ReadInt32());
                case DyType.Float: return new DyFloat(reader.ReadDouble());
                case DyType.String: return new DyString(reader.ReadString());
                case DyType.Char: return new DyChar(reader.ReadChar());
                case DyType.Bool: return (DyBool)reader.ReadBoolean();
                default: throw new NotSupportedException();
            }
        }
    }
}
