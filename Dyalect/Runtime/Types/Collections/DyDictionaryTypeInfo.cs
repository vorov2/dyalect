using Dyalect.Debug;
using System.Collections.Generic;
using System.Text;
using Dyalect.Codegen;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyDictionaryTypeInfo : DyTypeInfo
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

    [InstanceMethod(Method.Add)]
    internal static void AddItem(ExecutionContext ctx, DyDictionary self, DyObject key, DyObject value)
    {
        if (!self.TryAdd(key, value))
            ctx.KeyAlreadyPresent(key);
    }

    [InstanceMethod(Method.TryAdd)]
    internal static bool TryAddItem(DyDictionary self, DyObject key, DyObject value) => 
        self.TryAdd(key, value);

    [InstanceMethod(Method.TryGet)]
    internal static DyObject? TryGetItem(DyDictionary self, DyObject key)
    {
        if (!self.TryGet(key, out var value))
            return null;
        return value;
    }

    [InstanceMethod(Method.Remove)]
    internal static bool RemoveItem(DyDictionary self, DyObject key) =>
        self.Remove(key);

    [InstanceMethod(Method.Clear)]
    internal static void ClearItems(DyDictionary self) => self.Clear();

    [InstanceMethod(Method.ToTuple)]
    internal static DyObject ToTuple(DyObject self) => new DyTuple(GetArray(self));

    [InstanceMethod(Method.Compact)]
    internal static void Compact(ExecutionContext ctx, DyDictionary self, [Default]DyObject predicate)
    {
        foreach (var (key, value) in self.Dictionary)
        {
            if (predicate is not null)
            {
                var res = predicate.Invoke(ctx, value);

                if (ctx.HasErrors)
                    return;

                if (ReferenceEquals(res, DyBool.True))
                    self.Dictionary.Remove(key);
            }
            else if (ReferenceEquals(value, DyNil.Instance))
                self.Dictionary.Remove(key);
        }
    }

    private static DyObject[] GetArray(DyObject self)
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

    [InstanceMethod(Method.ContainsValue)]
    internal static bool ContainsValue(DyDictionary self, DyObject value) =>
        self.ContainsValue(value);
    
    [InstanceMethod(Method.GetAndRemove)]
    internal static DyObject GetAndRemove(DyDictionary self, DyObject key) =>
        self.GetAndRemove(key);

    [StaticMethod(Method.Dictionary)]
    internal static DyObject New(ExecutionContext ctx, [VarArg]DyTuple values)
    {
        if (values.Count == 0)
            return new DyDictionary();

        if (values.Count == 1)
        {
            var el = values.GetValue(0);

            if (el is DyTuple t)
                return new DyDictionary(t.ConvertToDictionary(ctx));
        }

        return new DyDictionary(values.ConvertToDictionary(ctx));
    }

    [StaticMethod(Method.FromTuple)]
    internal static DyObject FromTuple(ExecutionContext ctx, [VarArg]DyTuple values) => New(ctx, values);

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Tuple => new DyTuple(GetArray(self)),
            _ => base.CastOp(self, targetType, ctx)
        };
}
