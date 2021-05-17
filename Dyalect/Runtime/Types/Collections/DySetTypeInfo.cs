using System.Linq;
using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    internal sealed class DySetTypeInfo : DyTypeInfo
    {
        public DySetTypeInfo() : base(DyType.Set) { }

        public override string TypeName => DyTypeNames.Set;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Len | SupportedOperations.Iter;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DySet)left;
            return (DyBool)self.Equals(ctx, right);
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DySet)arg;
            return DyInteger.Get(self.Count);
        }

        private DyObject AddItem(ExecutionContext _, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return (DyBool)set.Add(value);
        }
        
        private DyObject Remove(ExecutionContext _, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return (DyBool)set.Remove(value);
        }
        
        private DyObject Contains(ExecutionContext _, DyObject self, DyObject value)
        {
            var set = (DySet)self;
            return (DyBool)set.Contains(value);
        }
        
        private DyObject Clear(ExecutionContext _, DyObject self)
        {
            var set = (DySet)self;
            set.Clear();
            return DyNil.Instance;
        }
        
        private DyObject ToArray(ExecutionContext _, DyObject self)
        {
            var set = (DySet)self;
            return set.ToArray();
        }
        
        private DyObject ToTuple(ExecutionContext _, DyObject self)
        {
            var set = (DySet)self;
            return set.ToTuple();
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
            return (DyBool)set.Overlaps(ctx, value);
        }

        private DyObject IsSubsetOf(ExecutionContext ctx, DyObject self, DyObject other)
        {
            var seq = (DySet)self;
            return (DyBool)seq.IsSubsetOf(ctx, other);
        }

        private DyObject IsSupersetOf(ExecutionContext ctx, DyObject self, DyObject other)
        {
            var seq = (DySet)self;
            return (DyBool)seq.IsSupersetOf(ctx, other);
        }
        
        protected override DyObject? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
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