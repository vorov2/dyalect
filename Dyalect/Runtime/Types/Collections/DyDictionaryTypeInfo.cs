using Dyalect.Debug;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyDictionaryTypeInfo : DyTypeInfo
    {
        public DyDictionaryTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Dictionary) { }

        public override string TypeName => DyTypeNames.Dictionary;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
            | SupportedOperations.Iter;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyDictionary)arg).Count;
            return ctx.RuntimeContext.Integer.Get(len);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var map = (DyDictionary)arg;
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
            return ctx.RuntimeContext.Nil.Instance;
        }

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject key, DyObject value)
        {
            var map = (DyDictionary)self;
            if (!map.TryAdd(key, value))
                return ctx.KeyAlreadyPresent();
            return ctx.RuntimeContext.Nil.Instance;
        }

        private DyObject TryAddItem(ExecutionContext ctx, DyObject self, DyObject key, DyObject value)
        {
            var map = (DyDictionary)self;
            if (!map.TryAdd(key, value))
                return DyBool.False;
            return DyBool.True;
        }

        private DyObject TryGetItem(ExecutionContext ctx, DyObject self, DyObject key)
        {
            var map = (DyDictionary)self;
            if (!map.TryGet(key, out var value))
                return ctx.RuntimeContext.Nil.Instance;
            return value!;
        }

        private DyObject RemoveItem(ExecutionContext ctx, DyObject self, DyObject key) =>
            ((DyDictionary)self).Remove(key) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

        private DyObject ClearItems(ExecutionContext ctx, DyObject self)
        {
            ((DyDictionary)self).Clear();
            return ctx.RuntimeContext.Nil.Instance;
        }

        private DyObject Compact(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (!Is(funObj, DyFunction.Type) && !Is(funObj, DyNil.Type))
                return ctx.InvalidType(funObj);

            var fun = funObj as DyFunction;
            var map = (DyDictionary)self;
            var newMap = new DyDictionary();

            foreach (var (key, value) in map.Map)
            {
                var res = fun is not null ? fun.Call(ctx, value) : value;

                if (ctx.HasErrors)
                    return ctx.RuntimeContext.Nil.Instance;

                if (!ReferenceEquals(res, ctx.RuntimeContext.Nil.Instance))
                    newMap[key] = res;
            }

            return newMap;
        }

        private DyObject ToTuple(ExecutionContext ctx, DyObject self)
        {
            var map = ((DyDictionary)self).Map;
            var xs = new List<DyLabel>();

            foreach (var (key, value) in map)
            {
                if (Is(key, DyString.Type))
                    xs.Add(new DyLabel(key.GetString(), value));
            }

            return new DyTuple(xs.ToArray());
        }

        private DyObject Contains(ExecutionContext ctx, DyObject self, DyObject key)
        {
            var map = (DyDictionary)self;
            return (DyBool)map.ContainsKey(key);
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            return name switch
            {
                "add" => Func.Member(ctx, name, AddItem, -1, new Par("key"), new Par("value")),
                "tryAdd" => Func.Member(ctx, name, TryAddItem, -1, new Par("key"), new Par("value")),
                "tryGet" => Func.Member(ctx, name, TryGetItem, -1, new Par("key")),
                "remove" => Func.Member(ctx, name, RemoveItem, -1, new Par("key")),
                "clear" => Func.Member(ctx, name, ClearItems),
                "toTuple" => Func.Member(ctx, name, ToTuple),
                "compact" => Func.Member(ctx, name, Compact, -1, new Par("by", StaticNil.Instance)),
                "contains" => Func.Member(ctx, name, Contains, -1, new Par("key")),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };
        }

        private DyObject New(ExecutionContext ctx, DyObject values)
        {
            if (ReferenceEquals(values, ctx.RuntimeContext.Nil.Instance))
                return new DyDictionary(ctx.RuntimeContext);

            if (values is DyTuple tup)
                return new DyDictionary(ctx.RuntimeContext, tup.ConvertToDictionary(ctx));
            
            return ctx.InvalidType(values);
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name is "Dictionary" or "fromTuple")
                return Func.Static(ctx, name, New, -1, new Par("values", StaticNil.Instance));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
