namespace Dyalect.Runtime.Types;

public sealed class DyFloat : DyObject
{
    public static readonly DyFloat Zero = new(0D);
    public static readonly DyFloat One = new(1D);
    public static readonly DyFloat NaN = new(double.NaN);
    public static readonly DyFloat PositiveInfinity = new(double.PositiveInfinity);
    public static readonly DyFloat NegativeInfinity = new(double.NegativeInfinity);
    public static readonly DyFloat Epsilon = new(double.Epsilon);
    public static readonly DyFloat Min = new(double.MinValue);
    public static readonly DyFloat Max = new(double.MaxValue);

    public readonly double Value;

    public override string TypeName => nameof(Dy.Float); 

    public DyFloat(double value) : base(Dy.Float) => Value = value;

    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(DyObject? obj) => obj is DyFloat f && Value == f.Value;

    public override string ToString() => Value.ToString(InvariantCulture.NumberFormat);

    public override object ToObject() => Value;

    public override DyObject Clone() => this;
}
