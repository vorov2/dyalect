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
            | SupportedOperations.Get | SupportedOperations.Len | SupportedOperations.Iter;

        private DyObject Add(ExecutionContext _, DyObject self, DyObject value)
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
        
        protected override DyObject? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "add" => Func.Member(name, Add, -1, new Par("value")),
                "remove" => Func.Member(name, Remove, -1, new Par("value")),
                "contains" => Func.Member(name, Contains, -1, new Par("value")),
                "clear" => Func.Member(name, Clear),
                "toArray" => Func.Member(name, ToArray),
                "toTuple" => Func.Member(name, ToTuple),
                "intersectWith" => Func.Member(name, IntersectWith, -1, new Par("values")),
                "unionWith" => Func.Member(name, UnionWith, -1, new Par("values")),
                "overlaps" => Func.Member(name, Overlaps, -1, new Par("values")),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject New(ExecutionContext ctx) => new DySet();

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Map" => Func.Static(name, New),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}