using System;

namespace Dyalect.Compiler
{
    internal struct HashString : IEquatable<HashString>
    {
        private readonly string value;
        private int hashCode = 0;

        public HashString(string value) => this.value = value;

        public override bool Equals(object? obj) => obj is HashString str && Equals(str);

        public bool Equals(HashString other) => other.value.Equals(value);

        public override int GetHashCode()
        {
            if (hashCode == 0)
                hashCode = GetHashCode();

            return hashCode;
        }

        public override string ToString() => value;

        public static implicit operator string(HashString str) => str.value;

        public static implicit operator HashString(string str) => new(str);
    }
}
