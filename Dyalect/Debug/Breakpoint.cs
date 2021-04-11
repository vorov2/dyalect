using System;

namespace Dyalect.Debug
{
    public sealed class Breakpoint : IEquatable<Breakpoint>
    {
        public Breakpoint(int line, int column) : this(line, column, false) { }

        public Breakpoint(int line, int column, bool temp)
        {
            Id = Guid.NewGuid();
            Line = line;
            Column = column;
            Temporary = temp;
        }

        public bool Temporary { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }

        internal Guid Id { get; private set; }

        internal int Offset { get; set; }

        public override int GetHashCode() => HashCode.Combine(Line, Column);

        public static bool Equals(Breakpoint fst, Breakpoint snd) =>
            ReferenceEquals(fst, snd) || (fst.Line == snd.Line && fst.Column == snd.Column);

        public bool Equals(Breakpoint other) => Equals(this, other);

        public override bool Equals(object obj) => obj is Breakpoint b && Equals(this, b);

        public override string ToString() => $"{(Temporary ? "#" : "")}{Line}:{Column}";
    }
}
