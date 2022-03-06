using System.Linq;
using System.Text;
using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    internal sealed class DySetTypeInfo : DyTypeInfo
    {
        public DySetTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Set) { }

        public override string TypeName => DyTypeNames.Set;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Len | SupportedOperations.Iter;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DySet)left;
            return self.Equals(ctx, right) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DySet)arg;
            return ctx.RuntimeContext.Integer.Get(self.Count);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DySet)arg;
            var sb = new StringBuilder("Set (");
            var c = 0;

            foreach (var v in self)
            {
                if (c++ > 0)
                    sb.Append(", ");

                sb.Append(v.ToString(ctx));

                if (ctx.HasErrors)
                    return ctx.RuntimeContext.Nil.Instance;
            }

            sb.Append(')');
            return new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, sb.ToString());
        }

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return set.Add(value) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
        }
        
        private DyObject Remove(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return set.Remove(value) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
        }
        
        private DyObject Contains(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return set.Contains(value) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
        }
        
        private DyObject Clear(ExecutionContext ctx, DyObject self)
        {
            var set = (DySet)self;
            set.Clear();
            return ctx.RuntimeContext.Nil.Instance;
        }
        
        private DyObject ToArray(ExecutionContext ctx, DyObject self)
        {
            var set = (DySet)self;
            return set.ToArray(ctx);
        }
        
        private DyObject ToTuple(ExecutionContext ctx, DyObject self)
        {
            var set = (DySet)self;
            return set.ToTuple(ctx);
        }
        
        private DyObject IntersectWith(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            set.IntersectWith(ctx, value);
            return ctx.RuntimeContext.Nil.Instance;
        }
        
        private DyObject UnionWith(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            set.UnionWith(ctx, value);
            return ctx.RuntimeContext.Nil.Instance;
        }
        
        private DyObject ExceptWith(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            set.ExceptWith(ctx, value);
            return ctx.RuntimeContext.Nil.Instance;
        }
        
        private DyObject Overlaps(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return set.Overlaps(ctx, value) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
        }

        private DyObject IsSubsetOf(ExecutionContext ctx, DyObject self, DyObject other)
        {
            var seq = (DySet)self;
            return seq.IsSubsetOf(ctx, other) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
        }

        private DyObject IsSupersetOf(ExecutionContext ctx, DyObject self, DyObject other)
        {
            var seq = (DySet)self;
            return seq.IsSupersetOf(ctx, other) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
        }
        
        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "add" => Func.Member(name, AddItem, -1, new Par("value")),
                "remove" => Func.Member(name, Remove, -1, new Par("value")),
                "contains" => Func.Member(name, Contains, -1, new Par("value")),
                "clear" => Func.Member(name, Clear),
                "toArray" => Func.Member(name, ToArray),
                "toTuple" => Func.Member(name, ToTuple),
                "except" => Func.Member(name, ExceptWith, -1, new Par("with")),
                "intersect" => Func.Member(name, IntersectWith, -1, new Par("with")),
                "union" => Func.Member(name, UnionWith, -1, new Par("with")),
                "overlaps" => Func.Member(name, Overlaps, -1, new Par("with")),
                "isSubset" => Func.Member(name, IsSubsetOf, -1, new Par("of")),
                "isSuperset" => Func.Member(name, IsSupersetOf, -1, new Par("of")),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject New(ExecutionContext ctx, DyObject arg)
        {
            var xs = ((DyTuple)arg).Values;
            var set = new DySet(DecType);

            foreach (var x in xs)
                set.Add(x);

            return set;
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Set" => Func.Static(name, New, 0, new Par("values", StaticNil.Instance)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}