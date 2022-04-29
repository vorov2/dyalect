using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Parser.Model
{
    public abstract class DPattern : DNode
    {
        protected DPattern(Location loc, NodeType type) : base(type, loc) { }
    }

    public sealed class DNamePattern : DPattern
    {
        public DNamePattern(Location loc) : base(loc, NodeType.NamePattern) { }

        public string Name { get; set; } = null!;

        protected internal override string? GetName() => Name;

        internal bool IsConstructor { get; set; }

        internal override void ToString(StringBuilder sb) => sb.Append(Name);
    }

    public sealed class DIntegerPattern : DPattern
    {
        public DIntegerPattern(Location loc) : base(loc, NodeType.IntegerPattern) { }

        public long Value { get; set; }

        internal override void ToString(StringBuilder sb) => sb.Append(Value.ToString(CI.NumberFormat));

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object? obj) => obj is DIntegerPattern i && i.Value == Value;
    }

    public sealed class DFloatPattern : DPattern
    {
        public DFloatPattern(Location loc) : base(loc, NodeType.FloatPattern) { }

        public double Value { get; set; }

        internal override void ToString(StringBuilder sb) => sb.Append(Value.ToString(CI.NumberFormat));

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object? obj) => obj is DFloatPattern f && f.Value == Value;
    }

    public sealed class DBooleanPattern : DPattern
    {
        public DBooleanPattern(Location loc) : base(loc, NodeType.BooleanPattern) { }

        public bool Value { get; set; }

        internal override void ToString(StringBuilder sb) => sb.Append(Value ? "true" : "false");

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object? obj) => obj is DBooleanPattern b && b.Value == Value;
    }

    public sealed class DCharPattern : DPattern
    {
        public DCharPattern(Location loc) : base(loc, NodeType.CharPattern) { }

        public char Value { get; set; }

        internal override void ToString(StringBuilder sb) => sb.Append(StringUtil.Escape(Value.ToString(), quote: "'"));

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object? obj) => obj is DCharPattern c && c.Value == Value;
    }

    public sealed class DStringPattern : DPattern
    {
        public DStringPattern(Location loc) : base(loc, NodeType.StringPattern) { }

        public DStringLiteral Value { get; set; } = null!;

        internal override void ToString(StringBuilder sb) => Value.ToString(sb);

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object? obj) => obj is DStringPattern s && s.Value.Value == Value.Value;
    }

    public sealed class DNotPattern : DPattern
    {
        public DNotPattern(Location loc) : base(loc, NodeType.NotPattern) { }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("not ");
            Pattern.ToString(sb);
        }

        public DPattern Pattern { get; set; } = null!;

        public override int GetHashCode() => HashCode.Combine("not", Pattern);

        public override bool Equals(object? obj) => obj is DNotPattern p && Pattern.Equals(p.Pattern);
    }

    public sealed class DComparisonPattern : DPattern
    {
        public DComparisonPattern(Location loc) : base(loc, NodeType.ComparisonPattern) { }

        public BinaryOperator Operator { get; init; }

        public DPattern Pattern { get; init; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Operator.ToSymbol());
            Pattern.ToString(sb);
        }

        public override int GetHashCode() => HashCode.Combine(Operator, Pattern);

        public override bool Equals(object? obj) => obj is DComparisonPattern p 
            && Operator == p.Operator && Pattern.Equals(p.Pattern);
    }

    public sealed class DNilPattern : DPattern
    {
        public DNilPattern(Location loc) : base(loc, NodeType.NilPattern) { }

        internal override void ToString(StringBuilder sb) => sb.Append("nil");

        public override int GetHashCode() => 0;

        public override bool Equals(object? obj) => obj is DNilPattern;
    }

    public abstract class DSequencePattern : DPattern
    {
        protected DSequencePattern(Location loc, NodeType nodeType) : base(loc, nodeType) { }

        public List<DNode> Elements { get; } = new List<DNode>();

        protected internal override int GetElementCount() => Elements.Count;

        protected internal override List<DNode> ListElements() => Elements;
    }

    public sealed class DTuplePattern : DSequencePattern
    {
        public DTuplePattern(Location loc) : base(loc, NodeType.TuplePattern) { }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('(');
            Elements.ToString(sb);
            sb.Append(')');
        }
    }

    public sealed class DArrayPattern : DSequencePattern
    {
        public DArrayPattern(Location loc) : base(loc, NodeType.ArrayPattern) { }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('[');
            Elements.ToString(sb);
            sb.Append(']');
        }
    }

    public sealed class DRangePattern : DPattern
    {
        public DRangePattern(Location loc) : base(loc, NodeType.RangePattern) { }

        public DPattern From { get; set; } = null!;

        public DPattern To { get; set; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            From.ToString(sb);
            sb.Append("..");
            To.ToString(sb);
        }
    }

    public sealed class DLabelPattern : DPattern
    {
        public DLabelPattern(Location loc) : base(loc, NodeType.LabelPattern) { }

        public string Label { get; set; } = null!;

        public DPattern Pattern { get; set; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Label);
            sb.Append(':');
            Pattern?.ToString(sb);
        }
    }

    public sealed class DAsPattern : DPattern
    {
        public DAsPattern(Location loc) : base(loc, NodeType.AsPattern) { }

        public string Name { get; set; } = null!;

        public DPattern Pattern { get; set; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            Pattern?.ToString(sb);
            sb.Append(" " + Name);
        }
    }

    public sealed class DMethodCheckPattern : DPattern
    {
        public DMethodCheckPattern(Location loc) : base(loc, NodeType.MethodCheckPattern) { }

        public string Name { get; set; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('.');
            sb.Append(Name);
        }
    }

    public sealed class DWildcardPattern : DPattern
    {
        public DWildcardPattern(Location loc) : base(loc, NodeType.WildcardPattern) { }

        internal override void ToString(StringBuilder sb) => sb.Append('_');
    }

    public sealed class DTypeTestPattern : DPattern
    {
        public DTypeTestPattern(Location loc) : base(loc, NodeType.TypeTestPattern) { }

        public Qualident TypeName { get; set; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(TypeName?.ToString());
        }
    }

    public sealed class DAndPattern : DPattern
    {
        public DAndPattern(Location loc) : base(loc, NodeType.AndPattern) { }

        public DPattern Left { get; set; } = null!;

        public DPattern Right { get; set; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            Left.ToString(sb);
            sb.Append(" and ");
            Right.ToString(sb);
        }
    }

    public sealed class DOrPattern : DPattern
    {
        public DOrPattern(Location loc) : base(loc, NodeType.OrPattern) { }

        public DPattern Left { get; set; } = null!;

        public DPattern Right { get; set; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            Left.ToString(sb);
            sb.Append(" or ");
            Right.ToString(sb);
        }
    }

    public sealed class DCtorPattern : DPattern
    {
        public DCtorPattern(Location loc) : base(loc, NodeType.CtorPattern) { }

        public string Constructor { get; set; } = null!;

        public Qualident? TypeName { get; set; }

        public List<DNode> Arguments { get; } = new();

        internal override void ToString(StringBuilder sb)
        {
            if (TypeName is not null)
            {
                sb.Append(TypeName);
                sb.Append('.');
            }

            sb.Append(Constructor);
            sb.Append('(');
            if (Arguments != null && Arguments.Count > 0)
                Arguments.ToString(sb);
            sb.Append(')');
        }
    }
}
