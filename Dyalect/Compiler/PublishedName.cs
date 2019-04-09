namespace Dyalect.Compiler
{
    public sealed class PublishedName
    {
        internal PublishedName(string name, ScopeVar data)
        {
            Name = name;
            Data = data;
        }

        public string Name { get; }

        public ScopeVar Data { get; }

        public override string ToString() => Name;
    }
}
