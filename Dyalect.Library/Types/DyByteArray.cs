using Dyalect.Compiler;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Types
{
    public sealed class DyByteArray : DyForeignObject<DyByteArrayTypeInfo>
    {
        private const int DEFAULT_SIZE = 32;
        private byte[] buffer;
        private int size;
        private int readPosition;

        public DyByteArray(RuntimeContext rtx, Unit unit, byte[] buffer) : base(rtx, unit)
        {
            if (buffer is not null)
            {
                this.buffer = buffer;
                size = buffer.Length;
            }
            else
                this.buffer = new byte[DEFAULT_SIZE];
        }

        public int Count => size;

        public override object ToObject() => GetBytes();

        public byte[] GetBytes() => Trim();

        public override DyObject Clone()
        {
            var clone = (DyByteArray)MemberwiseClone();

            if (buffer is not null)
                clone.buffer = GetBytes();

            return clone;
        }

        private byte[] Trim()
        {
            if (size != buffer.Length)
            {
                var dest = new byte[size];
                Array.Copy(buffer, 0, dest, 0, size);
                return dest;
            }

            return buffer;
        }

        private void EnsureSize(int append = 0)
        {
            if (size + append < buffer.Length)
                return;

            var newSize = System.Math.Max(append, buffer.Length == 0 ? 4 : buffer.Length * 2);
            var dest = new byte[newSize];
            Array.Copy(buffer, 0, dest, 0, size);
            buffer = dest;
        }

        public void Write(ExecutionContext ctx, DyObject obj)
        {
            switch (obj.TypeId)
            {
                case DyType.Integer:
                    Write(obj.GetInteger());
                    break;
                default:
                    ctx.InvalidType(obj);
                    break;
            }
        }

        private void Write(byte[] cz)
        {
            EnsureSize(cz.Length);
            Array.Copy(cz, 0, buffer, size, cz.Length);
            size += cz.Length;
        }

        private void Write(long value) => Write(BitConverter.GetBytes(value));

        public DyObject Read(ExecutionContext ctx, DyTypeInfo type) =>
            type.TypeCode switch
            {
                DyType.Integer => ReadInt64(),
                _ => ctx.InvalidType(type.TypeName)
            };

        private DyObject ReadInt64()
        {
            if (readPosition >= buffer.Length)
            {
                //return ctx.EndOfStream();
            }

            var cz = BitConverter.ToInt64(buffer, readPosition);
            readPosition += sizeof(long);
            return DyInteger.Get(cz);
        }
    }
}
