using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Linker;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Linq;
using System.Text;

namespace Dyalect.Library.Types
{
    public sealed class DyStringBuilder : DyForeignObject<DyStringBuilderTypeInfo>
    {
        internal StringBuilder Builder;

        public DyStringBuilder(RuntimeContext rtx, Unit unit, StringBuilder builder) : base(rtx, unit) => Builder = builder;

        public override bool Equals(DyObject? other) =>
            other is DyString || other is DyStringBuilder ? Builder.ToString() == other.ToString() 
            : base.Equals(other);

        public override object ToObject() => Builder.ToString();

        public override string ToString() => Builder.ToString();

        public override DyObject Clone()
        {
            var clone = (DyStringBuilder)MemberwiseClone();
            clone.Builder = new StringBuilder(Builder.ToString());
            return clone;
        }
    }

    public sealed class DyStringBuilderTypeInfo : ForeignTypeInfo
    {
        public DyStringBuilderTypeInfo() : base() { }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(((DyStringBuilder)arg).ToString());

        public override string TypeName => "StringBuilder";

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
            var sb = ((DyStringBuilder)self).Builder;

            if (index.TypeId != DyType.Integer)
                return ctx.InvalidType(index);

            if (len.TypeId != DyType.Integer)
                return ctx.InvalidType(len);

            var i = (long)index.ToObject();
            var ln = (long)len.ToObject();

            if (i + ln >= sb.Length)
                return ctx.IndexOutOfRange();

            sb.Remove((int)i, (int)ln);
            return self;
        }

        private DyObject Insert(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
        {
            var sb = ((DyStringBuilder)self).Builder;

            if (index.TypeId != DyType.Integer)
                return ctx.InvalidType(index);

            var i = (long)index.ToObject();
            var str = DyString.ToString(value, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            if (i < 0 || i >= sb.Length)
                return ctx.IndexOutOfRange();

            sb.Insert((int)i, str);
            return self;
        }

        protected override DyObject? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "insert" => DyForeignFunction.Member(name, Insert, -1, new Par("index"), new Par("value")),
                "remove" => DyForeignFunction.Member(name, Remove, -1, new Par("index"), new Par("len")),
                "replace" => DyForeignFunction.Member(name, Replace, -1, new Par("old"), new Par("new")),
                "append" => DyForeignFunction.Member(name, Append, -1, new Par("value")),
                "appendLine" => DyForeignFunction.Member(name, AppendLine, -1, new Par("value")),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };


        private DyObject New(ExecutionContext ctx, DyObject arg)
        {
            if (arg.TypeId != DyType.Nil)
            {
                var vals = DyIterator.Run(ctx, arg);
                var arr = vals.Select(o => DyString.ToString(o, ctx)).ToArray();
                var sb = new StringBuilder(string.Join("", arr));
                return new DyStringBuilder(ctx.RuntimeContext, DeclaringUnit, sb);
            }
            else
                return new DyStringBuilder(ctx.RuntimeContext, DeclaringUnit, new StringBuilder());
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "StringBuilder")
                return DyForeignFunction.Static(name, New, -1, new Par("values", DyNil.Instance));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
