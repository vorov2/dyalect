namespace Dyalect.Runtime.Types;

public sealed class DyVariant : DyObject, IProduction
{
    internal static readonly DyVariant Eta = new(string.Empty, DyTuple.Empty);
    internal readonly DyTuple Fields;

    public string Constructor { get; }

    public override string TypeName => nameof(Dy.Variant);
    
    public DyVariant(string constructor, DyTuple values) : base(Dy.Variant) =>
        (Constructor, Fields) = (constructor, values);

    internal DyVariant(DyError code, params object[] args) : this(code.ToString(), args) { }

    internal DyVariant(string code, params object[] args) : base(Dy.Variant)
    {
        Constructor = code;

        if (args is not null && args.Length > 0)
        {
            var arr = new DyObject[args.Length];

            for (var i = 0; i < args.Length; i++)
                arr[i] = TypeConverter.ConvertFrom(args[i]);

            Fields = new DyTuple(arr);
        }
        else
            Fields = DyTuple.Empty;
    }

    public override int GetHashCode() => Constructor.GetHashCode();

    public override object ToObject() => Fields.ToObject();

    public override bool Equals(DyObject? other) => other is DyVariant v && v.Constructor == Constructor && v.Fields.Equals(Fields);

    public override DyObject Clone()
    {
        var tup = Fields.Clone();

        if (ReferenceEquals(tup, Fields))
            return this;

        return new DyVariant(Constructor, tup);
    }
}
