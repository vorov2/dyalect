using System;

namespace Dyalect.Parser.Model
{
    public sealed class Qualident
    {
        internal Qualident(string local) => Local = local;

        internal Qualident(string local, string parent) : this(local) => Parent = parent;

        public string? Parent { get; }

        public string Local { get; }

        public bool IsPossibleEquality(Qualident qua)
        {
            if (qua.Parent is not null && Parent is not null)
                return qua.Parent == Parent && qua.Local == Local;

            return Local == qua.Local;
        }

        public override string ToString() => Parent is null ? Local : Parent + "." + Local;

        public override int GetHashCode() => HashCode.Combine(Parent, Local);

        public override bool Equals(object? obj) => obj is Qualident q
            && Parent == q.Parent && Local == q.Local;
    }
}
