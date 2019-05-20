using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public abstract class DPattern : DNode
    {
        protected DPattern(Location loc, NodeType type) : base(type, loc)
        {

        }
    }

    public sealed class DNamePattern : DPattern
    {
        public DNamePattern(Location loc) : base(loc, NodeType.NamePattern)
        {

        }

        public string Name { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Name);
        }
    }

    public sealed class DIntegerPattern : DPattern
    {
        public DIntegerPattern(Location loc) : base(loc, NodeType.IntegerPattern)
        {

        }

        public long Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Value.ToString(CI.NumberFormat));
        }
    }

    public sealed class DFloatPattern : DPattern
    {
        public DFloatPattern(Location loc) : base(loc, NodeType.FloatPattern)
        {

        }

        public double Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Value.ToString(CI.NumberFormat));
        }
    }

    public sealed class DBooleanPattern : DPattern
    {
        public DBooleanPattern(Location loc) : base(loc, NodeType.BooleanPattern)
        {

        }

        public bool Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Value ? "true" : "false");
        }
    }

    public sealed class DCharPattern : DPattern
    {
        public DCharPattern(Location loc) : base(loc, NodeType.CharPattern)
        {

        }

        public char Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(StringUtil.Escape(Value.ToString(), quote: "'"));
        }
    }

    public sealed class DStringPattern : DPattern
    {
        public DStringPattern(Location loc) : base(loc, NodeType.StringPattern)
        {

        }

        public DStringLiteral Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            Value.ToString(sb);
        }
    }

    public sealed class DTuplePattern : DPattern
    {
        public DTuplePattern(Location loc) : base(loc, NodeType.TuplePattern)
        {

        }

        public List<DPattern> Elements { get; } = new List<DPattern>();

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('(');
            Elements.ToString(sb);
            sb.Append(')');
        }
    }

    public sealed class DLabelPattern : DPattern
    {
        public DLabelPattern(Location loc) : base(loc, NodeType.TuplePattern)
        {

        }

        public string Label { get; set; }

        public DPattern Pattern { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Label);
            sb.Append(": ");
            Pattern.ToString(sb);
        }
    }
}
