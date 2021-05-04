﻿using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DApplication : DNode
    {
        public DApplication(DNode target, Location loc) : base(NodeType.Application, loc) =>
            Target = target;

        public DNode Target { get; internal set; }

        public List<DNode> Arguments { get; } = new();

        public bool NilSafety { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            Target.ToString(sb);

            if (NilSafety)
                sb.Append('?');

            sb.Append('(');
            Arguments.ToString(sb);
            sb.Append(')');
        }
    }
}
