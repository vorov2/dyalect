﻿using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DAccess : DNode
    {
        public DAccess(Location loc) : base(NodeType.Access, loc) { }

        public DNode Target { get; set; } = null!;

        public string Name { get; set; } = null!;

        protected internal override string? GetName() => Name;

        internal override void ToString(StringBuilder sb)
        {
            Target.ToString(sb);
            sb.Append('.');
            sb.Append(Name);
        }
    }
}
