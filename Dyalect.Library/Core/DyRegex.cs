using Dyalect.Runtime.Types;
using System.Text.RegularExpressions;

namespace Dyalect.Library.Core
{
    public sealed class DyRegex : DyForeignObject
    {
        internal readonly Regex Regex;

        public DyRegex(DyForeignTypeInfo typeInfo, string regex, bool ignoreCase, bool singleline, bool multiline) : base(typeInfo)
        {
            var opt = RegexOptions.Compiled;

            if (ignoreCase)
                opt |= RegexOptions.IgnoreCase;
            if (singleline)
                opt |= RegexOptions.Singleline;
            if (multiline)
                opt |= RegexOptions.Multiline;

            Regex = new Regex(regex, opt);
        }

        public override object ToObject() => Regex;

        public override DyObject Clone() => this;
    }
}
