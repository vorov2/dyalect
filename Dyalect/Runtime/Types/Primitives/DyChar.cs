﻿using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyChar : DyObject
    {
        public static readonly DyChar Empty = new('\0');
        public static readonly DyChar Max = new(char.MaxValue);
        public static readonly DyChar Min = new(char.MinValue);
        private readonly char value;

        public DyChar(char value) : base(DyType.Char) => this.value = value;

        public override object ToObject() => GetChar();

        protected internal override char GetChar() => value;

        protected internal override string GetString() => value.ToString();

        public override string ToString() => GetString();

        public override DyObject Clone() => this;

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeId);
            writer.Write(value);
        }

        public override int GetHashCode() => value.GetHashCode();
    }
}
