using Dyalect.Debug;
using System.Text;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyMapTypeInfo : DyTypeInfo
    {
        public DyMapTypeInfo() : base(DyType.Map) { }

        public override string TypeName => DyTypeNames.Map;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
            | SupportedOperations.Iter;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyMap)arg).Count;
            return DyInteger.Get(len);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var map = (DyMap)arg;
            var sb = new StringBuilder();
            sb.Append("Map {");
            var i = 0;

            foreach (var kv in map.Map)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(kv.Key.ToString(ctx) + " = " + kv.Value.ToString(ctx));
                i++;
            }

            sb.Append('}');
            return new DyString(sb.ToString());
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
        }

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject key, DyObject value)
        {
            var map = (DyMap)self;
            if (!map.TryAdd(key, value))
                return ctx.KeyAlreadyPresent();
            return DyNil.Instance;
        }

        private DyObject TryAddItem(ExecutionContext ctx, DyObject self, DyObject key, DyObject value)
        {
            var map = (DyMap)self;
            if (!map.TryAdd(key, value))
                return DyBool.False;
            return DyBool.True;
        }

        private DyObject TryGetItem(ExecutionContext ctx, DyObject self, DyObject key)
        {
            var map = (DyMap)self;
            if (!map.TryGet(key, out var value))
                return DyNil.Instance;
            return value!;
        }

        private DyObject RemoveItem(ExecutionContext ctx, DyObject self, DyObject key) =>
            ((DyMap)self).Remove(key) ? DyBool.True : DyBool.False;

        private DyObject ClearItems(ExecutionContext ctx, DyObject self)
        {
            ((DyMap)self).Clear();
            return DyNil.Instance;
        }

        protected override DyObject? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            return name switch
            {
                "add" => Func.Member(name, AddItem, -1, new Par("key"), new Par("value")),
                "tryAdd" => Func.Member(name, TryAddItem, -1, new Par("key"), new Par("value")),
                "tryGet" => Func.Member(name, TryGetItem, -1, new Par("key")),
                "remove" => Func.Member(name, RemoveItem, -1, new Par("key")),
                "clear" => Func.Member(name, ClearItems),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };
        }

        private DyObject New(ExecutionContext ctx, DyObject values)
        {
            if (values == DyNil.Instance)
                return new DyMap();
            else if (values is DyTuple tup)
                return new DyMap(tup.ConvertToDictionary());
            else
                return ctx.InvalidType(values);
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Map")
                return Func.Static(name, New, -1, new Par("values", DyNil.Instance));
            else if (name == "fromTuple")
                return Func.Static(name, New, -1, new Par("values"));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
