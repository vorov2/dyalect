using System.Linq;
using System.Text;
using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    internal sealed class DySetTypeInfo : DyTypeInfo
    {
        public override string TypeName => DyTypeNames.Set;

        public override int ReflectedTypeCode => DyType.Set;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Len | SupportedOperations.Iter;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DySet)left;
            return self.Equals(ctx, right) ? DyBool.True : DyBool.False;
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DySet)arg;
            return DyInteger.Get(self.Count);
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
                    return DyNil.Instance;
            }

            sb.Append(')');
            return new DyString(sb.ToString());
        }

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return set.Add(value) ? DyBool.True : DyBool.False;
        }
        
        private DyObject Remove(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return set.Remove(value) ? DyBool.True : DyBool.False;
        }
        
        private DyObject Contains(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return set.Contains(value) ? DyBool.True : DyBool.False;
        }
        
        private DyObject Clear(ExecutionContext ctx, DyObject self)
        {
            var set = (DySet)self;
            set.Clear();
            return DyNil.Instance;
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
            return DyNil.Instance;
        }
        
        private DyObject UnionWith(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            set.UnionWith(ctx, value);
            return DyNil.Instance;
        }
        
        private DyObject ExceptWith(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            set.ExceptWith(ctx, value);
            return DyNil.Instance;
        }
        
        private DyObject Overlaps(ExecutionContext ctx, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return set.Overlaps(ctx, value) ? DyBool.True : DyBool.False;
        }

        private DyObject IsSubsetOf(ExecutionContext ctx, DyObject self, DyObject other)
        {
            var seq = (DySet)self;
            return seq.IsSubsetOf(ctx, other) ? DyBool.True : DyBool.False;
        }

        private DyObject IsSupersetOf(ExecutionContext ctx, DyObject self, DyObject other)
        {
            var seq = (DySet)self;
            return seq.IsSupersetOf(ctx, other) ? DyBool.True : DyBool.False;
        }
        
        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Add" => Func.Member(name, AddItem, -1, new Par("value")),
                "Remove" => Func.Member(name, Remove, -1, new Par("value")),
                "Contains" => Func.Member(name, Contains, -1, new Par("value")),
                "Clear" => Func.Member(name, Clear),
                "ToArray" => Func.Member(name, ToArray),
                "ToTuple" => Func.Member(name, ToTuple),
                "Except" => Func.Member(name, ExceptWith, -1, new Par("with")),
                "Intersect" => Func.Member(name, IntersectWith, -1, new Par("with")),
                "Union" => Func.Member(name, UnionWith, -1, new Par("with")),
                "Overlaps" => Func.Member(name, Overlaps, -1, new Par("with")),
                "IsSubset" => Func.Member(name, IsSubsetOf, -1, new Par("of")),
                "IsSuperset" => Func.Member(name, IsSupersetOf, -1, new Par("of")),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject New(ExecutionContext ctx, DyObject arg)
        {
            var xs = ((DyTuple)arg).Values;
            var set = new DySet();

            foreach (var x in xs)
                set.Add(x);

            return set;
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Set" => Func.Static(name, New, 0, new Par("values", DyNil.Instance)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}