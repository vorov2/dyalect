namespace Dyalect.Runtime.Types;

public sealed class DyVariant : DyObject
{
    internal readonly string Constructor;
    internal readonly DyTuple Tuple;

    public override string TypeName => nameof(DyType.Variant);
    
    public DyVariant(string constructor, DyTuple values) : base(DyType.Variant) =>
        (Constructor, Tuple) = (constructor, values);

    internal DyVariant(DyErrorCode code, params object[] args) : this(code.ToString(), args) { }

    internal DyVariant(string code, params object[] args) : base(DyType.Variant)
    {
        Constructor = code;

        if (args is not null && args.Length > 0)
        {
            var arr = new DyObject[args.Length];

            for (var i = 0; i < args.Length; i++)
                arr[i] = TypeConverter.ConvertFrom(args[i]);

            Tuple = new DyTuple(arr);
        }
        else
            Tuple = DyTuple.Empty;
    }

    public override string? GetConstructor() => Constructor;

    public override int GetHashCode() => Constructor.GetHashCode();

    public override object ToObject() => Tuple.ToObject();

    public override bool Equals(DyObject? other) => other is DyVariant v && v.Constructor == Constructor && v.Tuple.Equals(Tuple);

    public override DyObject Clone() => new DyVariant(Constructor, (DyTuple)Tuple.Clone());
}
