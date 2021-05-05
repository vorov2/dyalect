using System;

namespace Dyalect.Debug
{
    public sealed class Breakpoint : IEquatable<Breakpoint>
    {
        public Breakpoint(int line, int column) : this(line, column, false) { }

        public Breakpoint(int line, int column, bool temp) =>
            (Id, Line, Column, Temporary) = (Guid.NewGuid(), line, column, temp);

        public bool Temporary { get; }

        public int Line { get; }

        public int Column { get; }

        internal Guid Id { get; }

        internal int Offset { get; set; }

        public override int GetHashCode() => HashCode.Combine(Line, Column);

        public static bool Equals(Breakpoint? fst, Breakpoint? snd) =>
            fst is null && snd is null || ReferenceEquals(fst, snd) || (fst?.Line == snd?.Line && fst?.Column == snd?.Column);

        public bool Equals(Breakpoint? other) => Equals(this, other);

        public override bool Equals(object? obj) => obj is Breakpoint b && Equals(this, b);

        public override string ToString() => $"{(Temporary ? "#" : "")}{Line}:{Column}";
    }
}
