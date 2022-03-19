using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dyalect.Library.Core
{
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

        private DyObject StaticReplace(ExecutionContext ctx, DyObject input, DyObject pattern, DyObject replacement,
            DyObject ignoreCase)
        {
            if (input.TypeId != DyType.String)
                return ctx.InvalidType(input);

            if (replacement.TypeId != DyType.String)
                return ctx.InvalidType(replacement);

            if (pattern.TypeId != DyType.String)
                return ctx.InvalidType(pattern);

            var str = Regex.Replace(input.GetString(), pattern.GetString(), replacement.GetString(),
                ignoreCase.GetBool(ctx) ? RegexOptions.None : RegexOptions.IgnoreCase);
            return new DyString(str);
        }

        private DyObject Replace(ExecutionContext ctx, DyObject self, DyObject input, DyObject replacement)
        {
            if (input.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, input);

            if (replacement.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, replacement);

            var rx = ((DyRegex)self).Regex;
            var str = rx.Replace(input.GetString(), replacement.GetString());
            return new DyString(str);
        }

        private DyObject Split(ExecutionContext ctx, DyObject self, DyObject input, DyObject count, DyObject index,
            DyObject removeEmptyEntries)
        {
            if (input.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, input);

            if (count.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, count);

            if (index.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, index);

            var icount = (int)count.GetInteger();

            if (icount == 0)
                icount = int.MaxValue;

            var rx = ((DyRegex)self).Regex;
            var arr = rx.Split(input.GetString(), icount, (int)index.GetInteger());
            var objs = new List<DyObject>();
            var flag = removeEmptyEntries.GetBool(ctx);

            for (var i = 0; i < arr.Length; i++)
                if (!flag || !string.IsNullOrEmpty(arr[i]))
                    objs.Add(new DyString(arr[i]));

            return new DyTuple(objs.ToArray());
        }

        private DyObject StaticSplit(ExecutionContext ctx, DyObject input, DyObject pattern, DyObject count, DyObject index,
            DyObject ignoreCase, DyObject removeEmptyEntries)
        {
            if (input.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, input);

            if (pattern.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, pattern);

            if (count.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, count);

            if (index.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, index);

            var icount = (int)count.GetInteger();

            if (icount == 0)
                icount = int.MaxValue;

            var rx = new Regex(pattern.GetString(), ignoreCase.GetBool(ctx) ? (RegexOptions.IgnoreCase|RegexOptions.Compiled) : RegexOptions.Compiled);
            var arr = rx.Split(input.GetString(), icount, (int)index.GetInteger());
            var objs = new List<DyObject>();
            var flag = removeEmptyEntries.GetBool(ctx);

            for (var i = 0; i < arr.Length; i++)
                if (!flag || !string.IsNullOrEmpty(arr[i]))
                    objs.Add(new DyString(arr[i]));

            return new DyTuple(objs.ToArray());
        }

        private DyObject Match(ExecutionContext ctx, DyObject self, DyObject input, DyObject start, DyObject len)
        {
            if (input.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, input);

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
                return ctx.InvalidType(DyType.String, input);

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
                "Match" => Func.Member(name, Match, -1, new Par("input"), new Par("index", DyInteger.Zero), new Par("count", DyNil.Instance)),
                "Matches" => Func.Member(name, Matches, -1, new Par("input"), new Par("index", DyInteger.Zero)),
                "Replace" => Func.Member(name, Replace, -1, new Par("input"), new Par("replacement")),
                "Split" => Func.Member(name, Split, -1, new Par("input"), new Par("count", DyInteger.Zero),
                    new Par("index", DyInteger.Zero), new Par("removeEmptyEntries", DyBool.False)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject New(ExecutionContext ctx, DyObject arg, DyObject ignoreCase, DyObject singleline, DyObject multiline)
        {
            if (arg.TypeId != DyType.String)
                return ctx.InvalidType(arg);

            return new DyRegex(this, arg.GetString(), ignoreCase.GetBool(ctx), singleline.GetBool(ctx), multiline.GetBool(ctx));
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Regex" => Func.Static(name, New, -1, new Par("pattern"), new Par("ignoreCase", DyBool.False),
                    new Par("singleline", DyBool.False), new Par("multiline", DyBool.False)),
                "Replace" => Func.Static(name, StaticReplace, -1, new Par("input"), new Par("pattern"), new Par("replacement"), new Par("ignoreCase", DyBool.False)),
                "Split" => Func.Static(name, StaticSplit, -1, new Par("input"), new Par("pattern"),
                    new Par("count", DyInteger.Zero), new Par("index", DyInteger.Zero),
                    new Par("ignoreCase", DyBool.False), new Par("removeEmptyEntries", DyBool.False)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
