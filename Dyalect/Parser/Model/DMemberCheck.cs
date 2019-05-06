using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DMemberCheck : DNode
    {
        public DMemberCheck(Location loc) : base(NodeType.MemberCheck, loc)
        {

        }

        public DNode Target { get; set; }

        public string Name { get; set; }

        protected internal override string GetName() => Name;

        internal override void ToString(StringBuilder sb)
        {
            Target.ToString(sb);
            sb.Append('.');
            sb.Append(Name);
            sb.Append('?');
        }
    }
}
