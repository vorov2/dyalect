using System.IO;
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

    private readonly double value;

    public override string TypeName => nameof(Dy.Float); 

    public DyFloat(double value) : base(Dy.Float) =>
        this.value = value;

    public override int GetHashCode() => value.GetHashCode();

    public override bool Equals(DyObject? obj) => obj is DyFloat f && value == f.value;

    public override string ToString() => value.ToString(InvariantCulture.NumberFormat);

    public override object ToObject() => value;

    protected internal override double GetFloat() => value;

    protected internal override long GetInteger() => (long)value;

    public override DyObject Clone() => this;

    internal override void Serialize(BinaryWriter writer)
    {
        writer.Write(TypeId);
        writer.Write(value);
    }
}
