namespace Dyalect.Parser.Model
{
    public sealed class Qualident
    {
        internal Qualident(string local) => Local = local;

        internal Qualident(string local, string parent) : this(local) => Parent = parent;

        public string Parent { get; }

        public string Local { get; }

        public bool IsPossibleEquality(Qualident qua)
        {
            if (qua.Parent is not null && Parent is not null)
                return qua.Parent == Parent && qua.Local == Local;

            return Local == Local;
        }

        public override string ToString() => Parent is null ? Local : Parent + "." + Local;
    }
}
