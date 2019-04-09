namespace Dyalect.Parser.Model
{
    public sealed class Qualident
    {
        internal Qualident()
        {

        }

        public string Parent { get; private set; }

        public string Local { get; private set; }

        internal void AddName(string name)
        {
            if (Parent == null && Local == null)
                Local = name;
            else if (Local != null)
            {
                Parent = Local;
                Local = name;
            }
        }

        public override string ToString() => Parent == null ? Local : Parent + "." + Local;
    }
}
