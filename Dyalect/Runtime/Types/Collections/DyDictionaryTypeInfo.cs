using Dyalect.Codegen;
using System.Text;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyDictionaryTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Dictionary);

    public override int ReflectedTypeId => Dy.Dictionary;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len | SupportedOperations.Iter | SupportedOperations.In;

    public DyDictionaryTypeInfo() => AddMixins(Dy.Lookup, Dy.Collection);

    #region Operations
    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg)
    {
        var len = ((DyDictionary)arg).Count;
        return DyInteger.Get(len);
    }

    protected override DyObject IterateOp(ExecutionContext ctx, DyObject self) => DyIterator.Create((DyDictionary)self);

    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) =>
        ToStringOrLiteral(ctx, arg, literal: true);

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        ToStringOrLiteral(ctx, arg, literal: false);

    private DyObject ToStringOrLiteral(ExecutionContext ctx, DyObject arg, bool literal)
    {
        var map = (DyDictionary)arg;
        var sb = new StringBuilder();
        sb.Append("Dictionary(");
        var i = 0;

        foreach (var kv in map.Dictionary)
        {
            if (i > 0)
                sb.Append(", ");

            if (literal)
                sb.Append(kv.Key.ToLiteral(ctx) + ": " + kv.Value.ToLiteral(ctx));
            else
                sb.Append(kv.Key.ToString(ctx) + ": " + kv.Value.ToString(ctx));

            i++;
        }

        sb.Append(')');
        return new DyString(sb.ToString());
    }

    protected override DyObject InOp(ExecutionContext ctx, DyObject self, DyObject field) =>
        ((DyDictionary)self).ContainsKey(field) ? True : False;

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index) => ((DyDictionary)self).GetItem(index, ctx);

    protected override DyObject SetOp(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        ((DyDictionary)self).SetItem(index, value, ctx);
        return Nil;
    }
    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Tuple => new DyTuple(((DyDictionary)self).GetArrayOfLabels()),
            _ => base.CastOp(ctx, self, targetType)
        };
    #endregion

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
    internal static DyObject ToTuple(DyDictionary self) => new DyTuple(self.GetArrayOfLabels());

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

                if (ReferenceEquals(res, True))
                    self.Dictionary.Remove(key);
            }
            else if (value.Is(Dy.Nil))
                self.Dictionary.Remove(key);
        }
    }

    [InstanceMethod]
    internal static bool ContainsKey(DyDictionary self, DyObject key) => self.ContainsKey(key);

    [InstanceMethod]
    internal static bool ContainsValue(DyDictionary self, DyObject value) => self.ContainsValue(value);
    
    [InstanceMethod(Method.GetAndRemove)]
    internal static DyObject GetAndRemove(DyDictionary self, DyObject key) => self.GetAndRemove(key);

    [StaticMethod(Method.Dictionary)]
    internal static DyObject New(ExecutionContext ctx, [VarArg]DyTuple values)
    {
        if (values.Count == 0)
            return new DyDictionary();

        if (values.Count == 1)
        {
            var el = values[0];

            if (el is DyTuple t)
                return new DyDictionary(t.ConvertToDictionary(ctx));
        }

        return new DyDictionary(values.ConvertToDictionary(ctx));
    }

    [StaticMethod(Method.FromTuple)]
    internal static DyObject FromTuple(ExecutionContext ctx, [VarArg]DyTuple values) => New(ctx, values);
}
