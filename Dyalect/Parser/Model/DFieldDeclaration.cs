using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DFieldDeclaration : DNode
    {
        public DFieldDeclaration(Location loc) : base(NodeType.Field, loc) { }

        public bool IsPrivate => Name?.Length > 0 && Name[0] == '_';

        public string Name { get; set; } = null!;

        protected internal override string? GetName() => Name;

        internal override void ToString(StringBuilder sb) => sb.Append(Name);
    }
}
