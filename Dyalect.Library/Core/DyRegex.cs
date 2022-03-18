using Dyalect.Runtime.Types;
using System.Text.RegularExpressions;

namespace Dyalect.Library.Core
{
    public sealed class DyRegex : DyForeignObject
    {
        internal readonly Regex Regex;

        public DyRegex(DyForeignTypeInfo typeInfo, string regex) : base(typeInfo)
        {
            Regex = new Regex(regex, RegexOptions.Compiled);
        }

        public override object ToObject() => Regex;

        public override DyObject Clone() => this;
    }
}
