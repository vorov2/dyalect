using System.IO;
namespace Dyalect.Runtime.Types;

public sealed class DyInteger : DyNumber
{
    public static readonly DyInteger Zero = new(0L);
    public static readonly DyInteger MinusOne = new(-1L);
    public static readonly DyInteger One = new(1L);
    public static readonly DyInteger Two = new(2L);
    public static readonly DyInteger Three = new(3L);
    public static readonly DyInteger Max = new(long.MaxValue);
    public static readonly DyInteger Min = new(long.MinValue);

    public override string TypeName => nameof(DyType.Integer); 
    
    public static DyInteger Get(long i) =>
        i switch
        {
            -1 => MinusOne,
            0 => Zero,
            1 => One,
            2 => Two,
            3 => Three,
            _ => new DyInteger(i)
        };

    internal readonly long Value;

    public DyInteger(long value) : base(DyType.Integer) => this.Value = value;

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString(CI.Default);

    public override bool Equals(DyObject? obj) => obj is DyInteger i && Value == i.Value;

    public override object ToObject() => Value == (int)Value ? System.Convert.ChangeType(Value, BCL.Int32) : Value;

    protected internal override double GetFloat() => Value;

    protected internal override long GetInteger() => Value;

    public override DyObject Clone() => this;

    internal override void Serialize(BinaryWriter writer)
    {
        writer.Write(TypeId);
        writer.Write(Value);
    }
}
