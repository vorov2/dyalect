using System;

namespace Dyalect.Compiler
{
    public struct HashString : IEquatable<HashString>
    {
        private readonly string value;
        private int hashCode = 0;

        public HashString(string value) => this.value = value;

        public HashString() => this.value = string.Empty;

        public override bool Equals(object? obj) => obj is HashString str && Equals(str);

        public bool Equals(HashString other) => other.value.Equals(value);

        internal int LookupHash() => hashCode;

        public override int GetHashCode()
        {
            if (hashCode == 0)
                hashCode = value.GetHashCode();

            return hashCode;
        }

        public override string ToString() => value;

        public static explicit operator string(HashString str) => str.value;

        public static implicit operator HashString(string str) => new(str);
    }
}
