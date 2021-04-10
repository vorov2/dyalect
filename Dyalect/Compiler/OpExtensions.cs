using System.IO;

namespace Dyalect.Compiler
{
    public static class OpExtensions
    {
        internal static int GetSize(this OpCode op) => OpSizeHelper.Op[(int)op];

        internal static int GetStack(this OpCode op) => OpStackHelper.Op[(int)op];

        internal static void Serialize(this Op op, BinaryWriter writer)
        {
            writer.Write((byte)op.Code);
            var size = GetSize(op.Code);

            if (size != 0)
                writer.Write(op.Data);
        }

        internal static Op DeserializeOp(this BinaryReader reader)
        {
            var opCode = (OpCode)reader.ReadByte();
            var size = GetSize(opCode);

            if (size == 0)
                return Op.Ops[opCode];
            else
                return new Op(opCode, reader.ReadInt32());
        }
    }
}
