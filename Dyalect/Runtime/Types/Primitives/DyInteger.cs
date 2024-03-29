﻿namespace Dyalect.Runtime.Types;

public sealed class DyInteger : DyObject
{
    public static readonly DyInteger Zero = new(0L);
    public static readonly DyInteger MinusOne = new(-1L);
    public static readonly DyInteger One = new(1L);
    public static readonly DyInteger Two = new(2L);
    public static readonly DyInteger Three = new(3L);
    public static readonly DyInteger Max = new(long.MaxValue);
    public static readonly DyInteger Min = new(long.MinValue);

    public readonly long Value;

    public override string TypeName => nameof(Dy.Integer);

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

    public DyInteger(long value) : base(Dy.Integer) => this.Value = value;

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString(InvariantCulture);

    public override bool Equals(DyObject? obj) => obj is DyInteger i && Value == i.Value;

    public override object ToObject() => Value == (int)Value ? Convert.ChangeType(Value, BCL.Int32) : Value;

    public override DyObject Clone() => this;
}
