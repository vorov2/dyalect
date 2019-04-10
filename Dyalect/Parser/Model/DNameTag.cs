using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DNameTag : DNode
    {
        public DNameTag(Location loc) : base(NodeType.NameTag, loc)
        {

        }

        public string Name { get; set; }

        public DNode Expression { get; set; }

        protected internal override string GetName() => Name;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Name);
            sb.Append(": ");
            Expression.ToString(sb);
        }
    }
}
