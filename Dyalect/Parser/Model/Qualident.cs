namespace Dyalect.Parser.Model
{
    public sealed class Qualident
    {
        internal Qualident(string local)
        {
            Local = local;
        }

        internal Qualident(string local, string parent) : this(local)
        {
            Parent = parent;
        }

        public string Parent { get; }

        public string Local { get; }

        public bool IsPossibleEquality(Qualident qua)
        {
            if (qua.Parent != null && Parent != null)
                return qua.Parent == Parent && qua.Local == Local;

            return Local == Local;
        }

        public override string ToString() => Parent == null ? Local : Parent + "." + Local;
    }
}
