//using Dyalect.Compiler;
//using Dyalect.Runtime;
//using Dyalect.Runtime.Types;
//using System;
//using System.Text;

//namespace Dyalect.Library.Types
//{
//    public sealed class DyByteArray : DyForeignObject<DyByteArrayTypeInfo>
//    {
//        private const int DEFAULT_SIZE = 32;
//        private byte[] buffer;
//        private int size;
//        private int readPosition;

//        public DyByteArray(RuntimeContext rtx, Unit unit, byte[]? buffer) : base(rtx, unit)
//        {
//            if (buffer is not null)
//            {
//                this.buffer = buffer;
//                size = buffer.Length;
//            }
//            else
//                this.buffer = new byte[DEFAULT_SIZE];
//        }

//        public int Position => readPosition;
        
//        public int Count => size;

//        public override object ToObject() => GetBytes();

//        public byte[] GetBytes() => Trim();

//        public void Reset() => readPosition = 0;

//        public override DyObject Clone()
//        {
//            var clone = (DyByteArray)MemberwiseClone();

//            if (buffer is not null)
//                clone.buffer = GetBytes();

//            return clone;
//        }

//        private byte[] Trim()
//        {
//            if (size != buffer.Length)
//            {
//                var dest = new byte[size];
//                Array.Copy(buffer, 0, dest, 0, size);
//                return dest;
//            }

//            return buffer;
//        }

//        private void EnsureSize(int append = 0)
//        {
//            if (size + append < buffer.Length)
//                return;

//            var newSize = System.Math.Max(append, buffer.Length == 0 ? 4 : buffer.Length * 2);
//            var dest = new byte[newSize];
//            Array.Copy(buffer, 0, dest, 0, size);
//            buffer = dest;
//        }

//        public void Write(ExecutionContext ctx, DyObject obj)
//        {
//            Reset();

//            switch (obj.TypeId)
//            {
//                case DyType.Integer:
//                    Write(obj.GetInteger());
//                    break;
//                case DyType.Float:
//                    Write(obj.GetFloat());
//                    break;
//                case DyType.Bool:
//                    Write(obj.GetBool());
//                    break;
//                case DyType.Char:
//                    Write(obj.GetChar());
//                    break;
//                case DyType.String:
//                    Write(obj.GetString());
//                    break;
//                default:
//                    ctx.InvalidType(obj);
//                    break;
//            }
//        }

//        private void Write(byte[] cz)
//        {
//            EnsureSize(cz.Length);
//            Array.Copy(cz, 0, buffer, size, cz.Length);
//            size += cz.Length;
//        }

//        private void Write(long value) => Write(BitConverter.GetBytes(value));

//        private void Write(double value) => Write(BitConverter.GetBytes(value));

//        private void Write(bool value) => Write(BitConverter.GetBytes(value));

//        private void Write(char value) => Write(BitConverter.GetBytes(value));

//        private void Write(string value)
//        {
//            var cz = Encoding.UTF8.GetBytes(value);
//            Write(BitConverter.GetBytes(cz.Length));
//            Write(cz);
//        }

//        public DyObject Read(ExecutionContext ctx, DyTypeInfo type) =>
//            type.TypeCode switch
//            {
//                DyType.Integer => ReadInt64(ctx),
//                DyType.Float => ReadDouble(ctx),
//                DyType.Bool => ReadBool(ctx),
//                DyType.Char => ReadChar(ctx),
//                DyType.String => ReadString(ctx),
//                _ => ctx.InvalidType(type.TypeName)
//            };

//        private DyObject ReadInt64(ExecutionContext ctx)
//        {
//            if (readPosition + sizeof(long) > buffer.Length)
//                return ctx.IndexOutOfRange();

//            var cz = BitConverter.ToInt64(buffer, readPosition);
//            readPosition += sizeof(long);
//            return ctx.RuntimeContext.Integer.Get(cz);
//        }

//        private DyObject ReadDouble(ExecutionContext ctx)
//        {
//            if (readPosition + sizeof(double) > buffer.Length)
//                return ctx.IndexOutOfRange();

//            var cz = BitConverter.ToDouble(buffer, readPosition);
//            readPosition += sizeof(double);
//            return new DyFloat(cz);
//        }

//        private DyObject ReadBool(ExecutionContext ctx)
//        {
//            if (readPosition + 1 > buffer.Length)
//                return ctx.IndexOutOfRange();

//            var cz = buffer[readPosition];
//            readPosition++;
//            return cz is 1 ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
//        }

//        private DyObject ReadChar(ExecutionContext ctx)
//        {
//            if (readPosition + sizeof(char) > buffer.Length)
//                return ctx.IndexOutOfRange();

//            var cz = BitConverter.ToChar(buffer, readPosition);
//            readPosition += sizeof(char);
//            return new DyChar(cz);
//        }

//        private DyObject ReadString(ExecutionContext ctx)
//        {
//            if (readPosition + sizeof(int) > buffer.Length)
//                return ctx.IndexOutOfRange();

//            var len = BitConverter.ToInt32(buffer, readPosition);
//            readPosition += sizeof(int);

//            if (readPosition + len > buffer.Length)
//                return ctx.IndexOutOfRange();

//            var str = Encoding.UTF8.GetString(buffer, readPosition, len);
//            return new DyString(str);
//        }
//    }
//}
