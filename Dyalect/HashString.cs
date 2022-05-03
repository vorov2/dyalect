namespace Dyalect;

public struct HashString : IEquatable<HashString>
{
    private readonly string value;
    private int hashCode;

    public HashString(string value) => (this.value, this.hashCode) = (value, 0);

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

    public static bool operator ==(HashString left, HashString right) => left.Equals(right);

    public static bool operator !=(HashString left, HashString right) => !left.Equals(right);
}
