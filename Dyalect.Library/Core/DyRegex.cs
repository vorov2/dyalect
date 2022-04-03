using Dyalect.Runtime.Types;
using System.Text.RegularExpressions;

namespace Dyalect.Library.Core
{
    public sealed class DyRegex : DyForeignObject
    {
        internal readonly Regex Regex;

        internal bool RemoveEmptyEntries { get; }

        public DyRegex(DyForeignTypeInfo typeInfo, string regex, bool ignoreCase, bool singleline, bool multiline, bool removeEmptyEntries) : base(typeInfo)
        {
            var opt = RegexOptions.Compiled;

            if (ignoreCase)
                opt |= RegexOptions.IgnoreCase;
            if (singleline)
                opt |= RegexOptions.Singleline;
            if (multiline)
                opt |= RegexOptions.Multiline;

            RemoveEmptyEntries = removeEmptyEntries;
            Regex = new Regex(regex, opt);
        }

        public override object ToObject() => Regex;

        public override DyObject Clone() => this;

        public override bool Equals(DyObject? other) => other is DyRegex r && r.Regex == Regex;

        public override int GetHashCode() => Regex.GetHashCode();

        public override string ToString() => Regex.ToString();
    }
}
