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

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
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

        private DyObject Replace(ExecutionContext ctx, DyObject self, DyObject input, DyObject replacement)
        {
            if (!input.IsString(ctx)) return Default();
            if (!replacement.IsString(ctx)) return Default();
            var rx = ((DyRegex)self).Regex;

            try
            {
                var str = rx.Replace(input.GetString(), replacement.GetString());
                return new DyString(str);
            }
            catch (RegexMatchTimeoutException)
            {
                return ctx.Timeout();
            }
        }

        private DyObject Split(ExecutionContext ctx, DyObject self, DyObject input, DyObject count, DyObject index)
        {
            if (!input.IsString(ctx)) return Default();
            if (count.NotNil() && !count.IsInteger(ctx)) return Default();
            if (!index.IsInteger(ctx)) return Default();
            var icount = int.MaxValue;

            if (count.NotNil())
                icount = (int)count.GetInteger();

            var idx = (int)index.GetInteger();
            var str = input.GetString();

            if (idx < 0 || idx >= str.Length)
                return ctx.IndexOutOfRange(index);

            var rx = (DyRegex)self;

            try
            {
                var arr = rx.Regex.Split(str, icount, idx);
                var objs = new List<DyObject>();

                for (var i = 0; i < arr.Length; i++)
                    if (!rx.RemoveEmptyEntries || !string.IsNullOrEmpty(arr[i]))
                        objs.Add(new DyString(arr[i]));

                return new DyTuple(objs.ToArray());
            }
            catch (RegexMatchTimeoutException)
            {
                return ctx.Timeout();
            }
        }

        private DyObject Match(ExecutionContext ctx, DyObject self, DyObject input, DyObject index, DyObject count)
        {
            if (!input.IsString(ctx)) return Default();
            var str = input.GetString();
            var istart = (int)index.GetInteger();
            var ilen = count.TypeId == DyType.Nil ? str.Length : (int)count.GetInteger();
            var rx = ((DyRegex)self).Regex;

            if (istart + ilen > str.Length)
                return ctx.IndexOutOfRange();

            try
            {
                var m = rx.Match(str, istart, ilen);
                return new DyRegexMatch(m);
            }
            catch (RegexMatchTimeoutException)
            {
                return ctx.Timeout();
            }
        }

        private DyObject Matches(ExecutionContext ctx, DyObject self, DyObject input, DyObject index)
        {
            if (!input.IsString(ctx)) return Default();
            if (!index.IsInteger(ctx)) return Default();
            var str = input.GetString();
            var idx = (int)index.GetInteger();
            var rx = ((DyRegex)self).Regex;

            if (idx > str.Length)
                return ctx.IndexOutOfRange();

            var ms = rx.Matches(str, idx);
            var xs = new FastList<DyRegexMatch>();

            for (var i = 0; i < ms.Count; i++)
                xs.Add(new DyRegexMatch(ms[i]));

            return new DyTuple(xs.ToArray());
        }

        private DyObject IsMatch(ExecutionContext ctx, DyObject self, DyObject input, DyObject index)
        {
            if (!input.IsString(ctx)) return Default();
            if (!index.IsInteger(ctx)) return Default();
            var idx = (int)index.GetInteger();
            var str = input.GetString();

            if (idx < 0 || idx >= str.Length)
                return ctx.IndexOutOfRange(index);

            var rx = ((DyRegex)self).Regex;

            try
            {
                return rx.IsMatch(str, idx) ? DyBool.True : DyBool.False;
            }
            catch (RegexMatchTimeoutException) 
            {
                return ctx.Timeout();
            }
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Match" => Func.Member(name, Match, -1, new Par("input"), new Par("index", DyInteger.Zero), new Par("count", DyNil.Instance)),
                "Matches" => Func.Member(name, Matches, -1, new Par("input"), new Par("index", DyInteger.Zero)),
                "Replace" => Func.Member(name, Replace, -1, new Par("input"), new Par("replacement")),
                "Split" => Func.Member(name, Split, -1, new Par("input"), new Par("count", DyNil.Instance), new Par("index", DyInteger.Zero)),
                "IsMatch" => Func.Member(name, IsMatch, -1, new Par("input"), new Par("index", DyInteger.Zero)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject New(ExecutionContext ctx, DyObject arg, DyObject ignoreCase, DyObject singleline, DyObject multiline, DyObject removeEmptyEntries)
        {
            if (!arg.IsString(ctx)) return Default();
            return new DyRegex(this, arg.GetString(), ignoreCase.IsTrue(), singleline.IsTrue(), multiline.IsTrue(), removeEmptyEntries.IsTrue());
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Regex" => Func.Static(name, New, -1, new Par("pattern"), new Par("ignoreCase", DyBool.False),
                    new Par("singleline", DyBool.False), new Par("multiline", DyBool.False), new Par("removeEmptyEntries", DyBool.False)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
