using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dyalect.Library.Core
{
    public sealed class DyRegexMatch : DyRegexCapture
    {
        private readonly Match match;

        public DyRegexMatch(Match match) : base(match) => this.match = match;

        protected internal override bool GetBool(ExecutionContext ctx) => match.Success;

        private DyTuple GetCaptures()
        {
            var xs = new List<DyRegexCapture>();

            for (var i = 0; i < match.Captures.Count; i++)
                xs.Add(new DyRegexCapture(match.Captures[i]));

            return new DyTuple(xs.ToArray());
        }

        protected internal override object? GetItem(string key) =>
            key switch
            {
                "name" => match.Name,
                "success" => match.Success,
                "captures" => GetCaptures(),
                _ => base.GetItem(key)
            };
    }
}
