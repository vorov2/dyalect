using System;

namespace Dyalect.Debug
{
    public sealed class Breakpoint : IEquatable<Breakpoint>
    {
        public Breakpoint(int line, int column) : this(line, column, false)
        {

        }

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

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + Line.GetHashCode();
                hash = hash * 31 + Column.GetHashCode();
                return hash;
            }
        }

        public static bool Equals(Breakpoint fst, Breakpoint snd)
        {
            return object.ReferenceEquals(fst, snd)
                || (fst.Line == snd.Line && fst.Column == snd.Column);
        }

        public bool Equals(Breakpoint other)
        {
            return Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            var bp = obj as Breakpoint;
            return Equals(this, bp);
        }

        public override string ToString()
        {
            return string.Format("{0}{1}:{2}", Temporary ? "#" : "", Line, Column);
        }
    }
}
