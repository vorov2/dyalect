using Dyalect.Debug;
using System.Collections.Generic;
using System.Text;
namespace Dyalect.Runtime.Types;

internal sealed class DyDictionaryTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(DyType.Dictionary);

    public override int ReflectedTypeId => DyType.Dictionary;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
        | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
        | SupportedOperations.Iter;

    public DyDictionaryTypeInfo() => AddMixin(DyType.Collection);

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
    {
        var len = ((DyDictionary)arg).Count;
        return DyInteger.Get(len);
    }

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx)
    {
        var map = (DyDictionary)arg;
        var sb = new StringBuilder();
        sb.Append("Dictionary (");
        var i = 0;

        foreach (var kv in map.Dictionary)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(kv.Key.ToLiteral(ctx) + ": " + kv.Value.ToLiteral(ctx));
            i++;
        }

        sb.Append(')');
        return new DyString(sb.ToString());
    }

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
    {
        var map = (DyDictionary)arg;
        var sb = new StringBuilder();
        sb.Append("Dictionary (");
        var i = 0;

        foreach (var kv in map.Dictionary)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(kv.Key.ToString(ctx) + ": " + kv.Value.ToString(ctx));
            i++;
        }

        sb.Append(')');
        return new DyString(sb.ToString());
    }

    protected override DyObject ContainsOp(DyObject self, DyObject field, ExecutionContext ctx) =>
        ((DyDictionary)self).ContainsKey(field) ? DyBool.True : DyBool.False;

    protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

    protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
    {
        self.SetItem(index, value, ctx);
        return DyNil.Instance;
    }

    private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject key, DyObject value)
    {
        var map = (DyDictionary)self;
        if (!map.TryAdd(key, value))
            return ctx.KeyAlreadyPresent(key);
        return DyNil.Instance;
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
            return DyNil.Instance;
        return value!;
    }

    private DyObject RemoveItem(ExecutionContext ctx, DyObject self, DyObject key) =>
        ((DyDictionary)self).Remove(key) ? DyBool.True : DyBool.False;

    private DyObject ClearItems(ExecutionContext ctx, DyObject self)
    {
        ((DyDictionary)self).Clear();
        return DyNil.Instance;
    }

    private DyObject Compact(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var map = (DyDictionary)self;

        foreach (var (key, value) in map.Dictionary)
        {
            if (functor.NotNil())
            {
                var res = functor.Invoke(ctx, value);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                if (ReferenceEquals(res, DyBool.True))
                    map.Dictionary.Remove(key);
            }
            else if (ReferenceEquals(value, DyNil.Instance))
                map.Dictionary.Remove(key);
        }

        return DyNil.Instance;
    }

    private DyObject[] GetArray(DyObject self)
    {
        var map = ((DyDictionary)self).Dictionary;
        var xs = new List<DyLabel>();

        foreach (var (key, value) in map)
        {
            if (key.TypeId == DyType.String)
                xs.Add(new DyLabel(key.GetString(), value));
        }

        return xs.ToArray();
    }

    private DyObject ToTuple(ExecutionContext ctx, DyObject self) => new DyTuple(GetArray(self));

    private DyObject ContainsValue(ExecutionContext ctx, DyObject self, DyObject value)
    {
        var map = (DyDictionary)self;
        return map.ContainsValue(value) ? DyBool.True : DyBool.False;
    }

    private DyObject GetAndRemove(ExecutionContext ctx, DyObject self, DyObject key)
    {
        var map = (DyDictionary)self;
        return map.GetAndRemove(key);
    }

    protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx)
    {
        return name switch
        {
            Method.Add => Func.Member(name, AddItem, -1, new Par("key"), new Par("value")),
            Method.TryAdd => Func.Member(name, TryAddItem, -1, new Par("key"), new Par("value")),
            Method.TryGet => Func.Member(name, TryGetItem, -1, new Par("key")),
            Method.Remove => Func.Member(name, RemoveItem, -1, new Par("key")),
            Method.Clear => Func.Member(name, ClearItems),
            Method.ToTuple => Func.Member(name, ToTuple),
            Method.Compact => Func.Member(name, Compact, -1, new Par("predicate", DyNil.Instance)),
            Method.ContainsValue => Func.Member(name, ContainsValue, -1, new Par("value")),
            Method.GetAndRemove => Func.Member(name, GetAndRemove, -1, new Par("value")),
            _ => base.InitializeInstanceMember(self, name, ctx),
        };
    }

    private DyObject New(ExecutionContext ctx, DyObject values)
    {
        if (ReferenceEquals(values, DyNil.Instance))
            return new DyDictionary();

        var xs = (DyTuple)values;

        if (xs.Count == 1)
        {
            var el = xs.GetValue(0);

            if (el is DyTuple t)
                return new DyDictionary(t.ConvertToDictionary(ctx));
        }

        return new DyDictionary(xs.ConvertToDictionary(ctx));
    }

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
        name switch
        {
            Method.Dictionary or Method.FromTuple => Func.Static(name, New, 0, new Par("values", DyNil.Instance)),
            _ => base.InitializeStaticMember(name, ctx)
        };

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Tuple => new DyTuple(GetArray(self)),
            _ => base.CastOp(self, targetType, ctx)
        };
}
