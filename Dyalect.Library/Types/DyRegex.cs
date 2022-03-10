using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Text.RegularExpressions;

namespace Dyalect.Library.Types
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

    public class DyRegexCapture : DyObject
    {
        private readonly Capture capture;

        public DyRegexCapture(Capture capture) : base(DyType.Object) => this.capture = capture;

        public override SupportedOperations Supports() => SupportedOperations.Get;

        public override int GetHashCode() => capture.GetHashCode();

        public override object ToObject() => capture;

        public override string ToString() => capture.Value;

        protected internal override object? GetItem(string key) =>
            key switch
            {
                "index" => capture.Index,
                "length" => capture.Length,
                "value" => capture.Value,
                _ => null
            };
    }

    public sealed class DyRegexMatch : DyRegexCapture
    {
        private readonly Match match;

        public DyRegexMatch(Match match) : base(match) => this.match = match;

        protected internal override bool GetBool() => match.Success;

        private DyTuple GetCaptures()
        {
            var xs = new FastList<DyRegexCapture>();

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

    public sealed class DyRegexTypeInfo : DyForeignTypeInfo
    {
        public override string TypeName => "Regex";

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            return new DyString(((DyRegex)arg).Regex.ToString());
        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            return left is DyRegex a && right is DyRegex b && a.Regex.ToString() == b.Regex.ToString()
                ? DyBool.True : DyBool.False;
        }

        private DyObject StaticReplace(ExecutionContext ctx, DyObject pattern, DyObject input, DyObject replacement)
        {
            if (input.TypeId != DyType.String)
                return ctx.InvalidType(input);

            if (replacement.TypeId != DyType.String)
                return ctx.InvalidType(replacement);

            if (pattern.TypeId != DyType.String)
                return ctx.InvalidType(pattern);

            var str = Regex.Replace(input.GetString(), pattern.GetString(), replacement.GetString());
            return new DyString(str);
        }

        private DyObject Replace(ExecutionContext ctx, DyObject self, DyObject input, DyObject replacement)
        {
            if (input.TypeId != DyType.String)
                return ctx.InvalidType(input);

            if (replacement.TypeId != DyType.String)
                return ctx.InvalidType(replacement);

            var rx = ((DyRegex)self).Regex;
            var str = rx.Replace(input.GetString(), replacement.GetString());
            return new DyString(str);
        }

        private DyObject Match(ExecutionContext ctx, DyObject self, DyObject input, DyObject start, DyObject len)
        {
            if (input.TypeId != DyType.String)
                return ctx.InvalidType(input);

            var str = input.GetString();
            var istart = (int)(long)start.ToObject();
            var ilen = len.TypeId == DyType.Nil ? str.Length : (int)(long)len.ToObject();
            var rx = ((DyRegex)self).Regex;

            if (istart + ilen > str.Length)
                return ctx.IndexOutOfRange();

            var m = rx.Match(str, istart, ilen);
            return new DyRegexMatch(m);
        }

        private DyObject Matches(ExecutionContext ctx, DyObject self, DyObject input, DyObject start)
        {
            if (input.TypeId != DyType.String)
                return ctx.InvalidType(input);

            var str = (string)input.ToObject();
            var istart = (int)(long)start.ToObject();
            var rx = ((DyRegex)self).Regex;

            if (istart > str.Length)
                return ctx.IndexOutOfRange();

            var ms = rx.Matches(str, istart);
            var xs = new FastList<DyRegexMatch>();

            for (var i = 0; i < ms.Count; i++)
                xs.Add(new DyRegexMatch(ms[i]));

            return new DyTuple(xs.ToArray());
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Match" => Func.Member(name, Match, -1, new Par("input"), new Par("start", DyInteger.Zero), new Par("len", DyNil.Instance)),
                "Matches" => Func.Member(name, Matches, -1, new Par("input"), new Par("start", DyInteger.Zero)),
                "Replace" => Func.Static(name, Replace, -1, new Par("input"), new Par("replacement")),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject New(ExecutionContext ctx, DyObject arg)
        {
            if (arg.TypeId != DyType.String)
                return ctx.InvalidType(arg);

            return new DyRegex(this, arg.GetString());
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Regex" => Func.Static(name, New, -1, new Par("pattern")),
                "Replace" => Func.Static(name, StaticReplace, -1, new Par("pattern"), new Par("input"), new Par("replacement")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
