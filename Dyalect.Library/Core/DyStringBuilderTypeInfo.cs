﻿using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Linq;
using System.Text;

namespace Dyalect.Library.Core
{
    public sealed class DyStringBuilderTypeInfo : DyForeignTypeInfo
    {
        public override string ReflectedTypeName => "StringBuilder";

        public DyStringBuilder Create(StringBuilder sb) => new(this, sb);

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
            new DyString(((DyStringBuilder)arg).ToString());

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neg
            | SupportedOperations.Not | SupportedOperations.Len;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
            DyInteger.Get(((DyStringBuilder)arg).Builder.Length);

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var a = ((DyStringBuilder)left).Builder;
            var b = ((DyStringBuilder)right).Builder;
            return a.ToString() == b.ToString() ? DyBool.True : DyBool.False;
        }

        private DyObject Append(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var sb = ((DyStringBuilder)self).Builder;
            var str = DyString.ToString(value, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            sb.Append(str);
            return self;
        }

        private DyObject AppendLine(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var sb = ((DyStringBuilder)self).Builder;
            var str = DyString.ToString(value, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            sb.AppendLine(str);
            return self;
        }

        private DyObject Replace(ExecutionContext ctx, DyObject self, DyObject old, DyObject @new)
        {
            var a = DyString.ToString(old, ctx);
            var b = DyString.ToString(@new, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var sb = ((DyStringBuilder)self).Builder;
            sb.Replace(a, b);
            return self;
        }

        private DyObject Remove(ExecutionContext ctx, DyObject self, DyObject index, DyObject len)
        {
            if (!index.IsInteger(ctx)) return Default();
            if (!len.IsInteger(ctx)) return Default();
            var sb = ((DyStringBuilder)self).Builder;
            var i = index.GetInteger();
            var ln = len.GetInteger();

            if (i + ln >= sb.Length)
                return ctx.IndexOutOfRange();

            sb.Remove((int)i, (int)ln);
            return self;
        }

        private DyObject Insert(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
        {
            if (!index.IsInteger(ctx)) return Default();
            var sb = ((DyStringBuilder)self).Builder;

            var i = index.GetInteger();
            var str = DyString.ToString(value, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            if (i < 0 || i >= sb.Length)
                return ctx.IndexOutOfRange();

            sb.Insert((int)i, str);
            return self;
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Insert" => Func.Member(name, Insert, -1, new Par("index"), new Par("value")),
                "Remove" => Func.Member(name, Remove, -1, new Par("index"), new Par("count")),
                "Replace" => Func.Member(name, Replace, -1, new Par("value"), new Par("other")),
                "Append" => Func.Member(name, Append, -1, new Par("value")),
                "AppendLine" => Func.Member(name, AppendLine, -1, new Par("value", DyString.Empty)),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };

        private DyObject New(ExecutionContext ctx, DyObject arg)
        {
            if (arg.TypeId != DyType.Nil)
            {
                var vals = DyIterator.ToEnumerable(ctx, arg);
                var arr = vals.Select(o => DyString.ToString(o, ctx)).ToArray();
                var sb = new StringBuilder(string.Join("", arr));
                return new DyStringBuilder(this, sb);
            }
            else
                return new DyStringBuilder(this, new StringBuilder());
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "StringBuilder")
                return Func.Static(name, New, 0, new Par("values"));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
