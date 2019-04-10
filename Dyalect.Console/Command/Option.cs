namespace Dyalect.Command
{
    public sealed class Option
    {
        public Option(string key) : this(key, true)
        {

        }

        public Option(string key, object value)
        {
            Key = key ?? "";
            Value = value ?? new object();
        }

        public string Key { get; }

        public object Value { get; }

        public override string ToString() => $"-{Key} {Value}";

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Key.GetHashCode();
                hash = hash * 23 + Value.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Option o && Key == o.Key && Value == o.Value;
        }
    }
}
